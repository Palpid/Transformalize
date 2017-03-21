﻿#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Text.RegularExpressions;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms {
    public class RegexIsMatchTransform : BaseTransform {
        private readonly Regex _regex;
        private readonly Field[] _input;

        public RegexIsMatchTransform(IContext context) : base(context, "bool") {
            _input = MultipleInput();
#if NETS10
            _regex = new Regex(context.Transform.Pattern);
#else
            _regex = new Regex(context.Transform.Pattern, RegexOptions.Compiled);
#endif
        }

        public override IRow Transform(IRow row) {
            foreach (var field in _input) {
                var match = _regex.Match(row[field].ToString());
                if (match.Success) {
                    row[Context.Field] = true;
                    break;
                } else {
                    row[Context.Field] = false;
                }
            }
            Increment();
            return row;
        }
    }
}