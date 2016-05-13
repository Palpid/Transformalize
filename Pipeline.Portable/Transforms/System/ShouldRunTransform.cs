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

using Pipeline.Context;
using Pipeline.Contracts;

namespace Pipeline.Transforms.System {
    public class ShouldRunTransform : BaseTransform, ITransform {
        private readonly ITransform _transform;

        public ShouldRunTransform(PipelineContext context, ITransform transform) : base(context) {
            _transform = transform;
        }

        public IRow Transform(IRow row) {

            return Context.Transform.ShouldRun(row) ? _transform.Transform(row) : row;
        }
    }
}