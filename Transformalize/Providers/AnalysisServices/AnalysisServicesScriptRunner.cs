﻿using System;
using Microsoft.AnalysisServices;
using Transformalize.Libs.NLog;

namespace Transformalize.Providers.AnalysisServices
{
    public class AnalysisServicesScriptRunner : IScriptRunner
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly AnalysisServicesConnection _connection;

        public AnalysisServicesScriptRunner(AnalysisServicesConnection connection)
        {
            _connection = connection;
        }

        public IScriptReponse Execute(string script)
        {
            var response = new ScriptResponse();
            var server = new Server();

            try
            {
                _log.Debug("Connecting to {0} on {1}.", _connection.Database, _connection.Server);
                server.Connect(_connection.ConnectionString);

                var results = server.Execute(script);

                foreach (XmlaResult result in results)
                {
                    foreach (XmlaMessage message in result.Messages)
                    {
                        response.Messages.Add(message.Description);
                    }
                }
                response.Success = true;
            }
            catch (Exception e)
            {
                _log.Debug(e.Message + (e.InnerException != null ? " " + e.InnerException.Message : string.Empty));
                response.Messages.Add(e.Message);
            }
            finally
            {
                if (server.Connected)
                {
                    _log.Debug("Disconnecting from {0} on {1}.", _connection.Database, _connection.Server);
                    server.Disconnect();
                }
            }
            return response;
        }
    }
}