using System.Collections.Generic;
using Transformalize.Libs.fastJSON;
using Transformalize.Libs.NLog;

namespace Transformalize.Main.Providers.ElasticSearch {

    public class ElasticSearchEntityCreator : IEntityCreator {
        private readonly Logger _log = LogManager.GetLogger("tfl");
        private readonly Dictionary<string, string> _types = new Dictionary<string, string>() {
            {"int64", "long"},
            {"int16","integer"},
            {"int","integer"},
            {"int32","integer"},
            {"datetime","date"},
            {"bool","boolean"},
            {"decimal","float"},
            {"guid","string"}
        };

        private readonly List<string> _analyzers = new List<string> {
            "standard",
            "simple",
            "whitespace",
            "keyword",
            "pattern",
            "snowball",
            "arabic",
            "armenian",
            "basque",
            "brazilian",
            "bulgarian",
            "catalan",
            "chinese",
            "cjk",
            "czech",
            "danish",
            "dutch",
            "english",
            "finnish",
            "french",
            "galician",
            "german",
            "greek",
            "hindi",
            "hungarian",
            "indonesian",
            "italian",
            "norwegian",
            "persian",
            "portuguese",
            "romanian",
            "russian",
            "spanish",
            "swedish",
            "turkish",
            "thai",
            string.Empty
        };
        public IEntityExists EntityExists { get; set; }

        public ElasticSearchEntityCreator() {
            EntityExists = new ElasticSearchEntityExists();
        }

        public void Create(AbstractConnection connection, Process process, Entity entity) {

            var client = ElasticSearchClientFactory.Create(connection, entity);

            client.Client.IndicesCreate(client.Index, "{ \"settings\":{}}");

            var fields = GetFields(entity);
            var properties = new Dictionary<string, object>() { { "properties", fields } };
            var body = new Dictionary<string, object>() { { client.Type, properties } };
            var json = JSON.Instance.ToJSON(body);

            var response = client.Client.IndicesPutMapping(client.Index, client.Type, json);

            if (response.Success)
                return;

            _log.Error(response.Error.ExceptionMessage);
            throw new TransformalizeException("Error writing ElasticSearch mapping.");
        }

        public Dictionary<string, object> GetFields(Entity entity) {
            var fields = new Dictionary<string, object>();
            foreach (var field in entity.OutputFields()) {
                var alias = field.Alias.ToLower();
                var type = _types.ContainsKey(field.SimpleType) ? _types[field.SimpleType] : field.SimpleType;
                if (type.Equals("string")) {
                    foreach (var searchType in field.SearchTypes) {
                        var analyzer = searchType.Analyzer.ToLower();
                        if (_analyzers.Contains(analyzer)) {
                            if (fields.ContainsKey(alias)) {
                                fields[alias + searchType.Name.ToLower()] = new Dictionary<string, object>() { { "type", type }, { "analyzer", analyzer } };
                            } else {
                                if (analyzer.Equals(string.Empty)) {
                                    fields[alias] = new Dictionary<string, object>() { { "type", type } };
                                } else {
                                    fields[alias] = new Dictionary<string, object>() { { "type", type }, { "analyzer", analyzer } };
                                }
                            }
                        } else {
                            _log.Warn("Analyzer '{0}' specified in search type '{1}' is not supported.  Please use a built-in analyzer for Elasticsearch.", analyzer, searchType.Name);
                            if (!fields.ContainsKey(alias)) {
                                fields[alias] = new Dictionary<string, object>() { { "type", type } };
                            }
                        }
                    }
                } else {
                    fields[alias] = new Dictionary<string, object>() { { "type", type } };
                }
            }
            if (!fields.ContainsKey("tflbatchid")) {
                fields.Add("tflbatchid", new Dictionary<string, object> { { "type", "long" } });
            }
            return fields;
        }

        public Dictionary<string, string> GetFieldMap(Entity entity) {
            var map = new Dictionary<string, string>();
            foreach (var field in entity.OutputFields()) {
                var alias = field.Alias.ToLower();
                if (field.SimpleType.Equals("string")) {
                    foreach (var searchType in field.SearchTypes) {
                        if (map.ContainsKey(alias)) {
                            map[alias + searchType.Name.ToLower()] = alias;
                        } else {
                            map[alias] = alias;
                        }
                    }
                } else {
                    map[alias] = alias;
                }
            }
            if (!map.ContainsKey("tflbatchid")) {
                map.Add("tflbatchid", "tflbatchid");
            }
            return map;
        }

    }
}