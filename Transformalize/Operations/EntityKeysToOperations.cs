﻿/*
Transformalize - Replicate, Transform, and Denormalize Your Data...
Copyright (C) 2013 Dale Newman

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Core.Entity_;
using Transformalize.Core.Field_;
using Transformalize.Extensions;
using Transformalize.Libs.NLog;
using Transformalize.Libs.Rhino.Etl.Core;
using Transformalize.Libs.Rhino.Etl.Core.Operations;
using Transformalize.Providers;
using Transformalize.Providers.SqlServer;

namespace Transformalize.Operations {
    public class EntityKeysToOperations : AbstractOperation {
        private readonly Entity _entity;
        private readonly string _operationColumn;
        private const string KEYS_TABLE_VARIABLE = "@KEYS";
        private readonly string[] _fields;
        private readonly Field[] _key;

        public EntityKeysToOperations(Entity entity, string operationColumn = "operation") {
            _entity = entity;
            _operationColumn = operationColumn;
            _fields = new FieldSqlWriter(_entity.All).ExpandXml().Input().Keys().ToArray();
            _key = new FieldSqlWriter(_entity.PrimaryKey).ToArray();
        }
        
        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var batch in _entity.InputKeys.Partition(_entity.InputConnection.BatchSize))
            {
                var sql = SelectByKeys(batch);
                var row = new Row();
                row[_operationColumn] = new EntityDataExtract(_fields, sql, _entity.InputConnection.ConnectionString);
                yield return row;
            }
        }

        public string SelectByKeys(IEnumerable<Row> rows) {
            var sql = "SET NOCOUNT ON;\r\n" +
                      SqlTemplates.CreateTableVariable(KEYS_TABLE_VARIABLE, _key, false) +
                      SqlTemplates.BatchInsertValues(50, KEYS_TABLE_VARIABLE, _key, rows, ((SqlServerConnection)_entity.InputConnection).InsertMultipleValues()) + Environment.NewLine +
                      SqlTemplates.Select(_entity.All, _entity.Name, KEYS_TABLE_VARIABLE);

            Trace(sql);

            return sql;
        }
    }
}