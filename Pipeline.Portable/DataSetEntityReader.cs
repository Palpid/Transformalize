#region license
// Transformalize
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Collections.Generic;
using System.Linq;
using Pipeline.Context;
using Pipeline.Contracts;

namespace Pipeline {
    public class DataSetEntityReader : IRead {

        readonly InputContext _input;
        private readonly IRowFactory _rowFactory;

        public DataSetEntityReader(InputContext input, IRowFactory rowFactory)
        {
            _input = input;
            _rowFactory = rowFactory;
        }

        public IEnumerable<IRow> Read() {
            return GetTypedDataSet(_input.Entity.Name);
        }

        public object GetVersion() {
            return null;
        }

        public IEnumerable<IRow> GetTypedDataSet(string name) {
            var rows = new List<IRow>();

            var lookup = _input.Entity.Fields.ToDictionary(k => k.Name, v => v);
            foreach (var row in _input.Entity.Rows) {
                var pipelineRow = _rowFactory.Create();
                foreach (var pair in row) {
                    if (!lookup.ContainsKey(pair.Key))
                        continue;
                    var field = lookup[pair.Key];
                    pipelineRow[field] = field.Convert(pair.Value);
                }
                rows.Add(pipelineRow);
            }
            return rows;
        }

    }
}