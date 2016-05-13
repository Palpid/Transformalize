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
using System;
using System.Linq;
using System.Reflection;
using Cfg.Net;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Extensions;
using Pipeline.Logging;
using Pipeline.Transforms;

namespace Pipeline.Configuration.Ext {
    public static class ProcessValidate {

        public static void Validate(this Process p, Action<string> error, Action<string> warn) {
            ValidateDuplicateEntities(p, error);
            ValidateDuplicateFields(p, error);
            ValidateRelationships(p, error, warn);
            ValidateEntityConnections(p, error);
            ValidateActionConnections(p, error);
            ValidateTemplateActionConnections(p, error);
            ValidateTransformConnections(p, error);
            ValidateMapConnections(p, error);
            ValidateMapTransforms(p, error);
            ValidateSearchTypes(p, error);
            ValidateTransforms(p, error);
            ValidateShouldRuns(p, error, warn);
            ValidateScripts(p, error);
            ValidateEntityFields(p, error);
            ValidateCalculatedFields(p, error);
        }

        static void ValidateCalculatedFields(Process p, Action<string> error) {
            foreach (var entity in p.Entities) {
                foreach (var field in entity.CalculatedFields.Where(f => !f.Produced)) {
                    var transform = field.Transforms.FirstOrDefault();
                    if (transform != null && Transform.Transforms().Contains(transform.Method) && !transform.Parameters.Any()) {
                        if (Transform.TransformProducers().Contains(transform.Method)) {
                            continue;
                        }

                        if (Transform.Transforms().Contains(transform.Method) && !transform.Parameters.Any()) {
                            error($"The transform {transform.Method} in {entity.Alias}.{field.Alias} requires input.  If using short-hand, use copy().  Otherwise, set the parameter attribute, or define a parameters collection.");
                        }
                    }
                }
            }
        }

        static void ValidateEntityFields(Process p, Action<string> error) {
            if (p.Mode != "meta" && !p.Actions.Any()) {
                foreach (var entity in p.Entities) {
                    if (!entity.Fields.Any(f => f.Input)) {
                        error($"The {entity.Alias} doesn't have any input fields defined.");
                    }
                }
            }
        }

        static void ValidateScripts(Process process, Action<string> error) {
            var scriptsRegistered = process.Scripts.Select(s => s.Name);
            var scriptReferences = process.GetAllTransforms().Where(t => t.Scripts.Any()).SelectMany(t => t.Scripts).Select(s => s.Name).Distinct();
            var problems = scriptReferences.Except(scriptsRegistered).ToArray();
            if (problems.Length <= 0)
                return;
            foreach (var problem in problems) {
                error($"The {problem} script is not registered in the scripts collection.");
            }
        }

        static void ValidateMapTransforms(Process p, Action<string> error) {
            foreach (var transform in p.GetAllTransforms().Where(t => t.Method == "map")) {
                if (p.Maps.All(m => m.Name != transform.Map)) {
                    error($"A map transform references an invalid map: {transform.Map}.");
                }
                var map = p.Maps.First(m => m.Name == transform.Map);
                foreach (var item in map.Items.Where(i => i.Parameter != string.Empty)) {
                    Field field;
                    if (!p.TryGetField(item.Parameter, out field)) {
                        error($"A map transform references an invalid field: {item.Parameter}.");
                    }
                }
            }
        }


        static void ValidateMapConnections(Process p, Action<string> error) {
            foreach (var map in p.Maps.Where(m => m.Query != string.Empty).Where(map => p.Connections.All(c => c.Name != map.Connection))) {
                error($"The {map.Name} map references an invalid connection: {map.Connection}.");
            }
        }
        static void ValidateTransformConnections(Process p, Action<string> error) {

            var methodsWithConnections = new[] { "mail", "run" };

            foreach (var transform in p.GetAllTransforms().Where(t => methodsWithConnections.Any(nc => nc == t.Method))) {
                var connection = p.Connections.FirstOrDefault(c => c.Name == transform.Connection);
                if (connection == null) {
                    error($"The {transform.Method} transform references an invalid connection: {transform.Connection}.");
                    continue;
                }

                switch (transform.Method) {
                    case "mail":
                        if (connection.Provider != "mail") {
                            error($"The {transform.Method} transform references the wrong type of connection: {connection.Provider}.");
                        }
                        break;
                }
            }
        }

        static void ValidateTemplateActionConnections(Process p, Action<string> error) {
            foreach (var action in p.Templates.SelectMany(template => template.Actions.Where(a => a.Connection != string.Empty).Where(action => p.Connections.All(c => c.Name != action.Connection)))) {
                error($"The {action.Type} template action references an invalid connection: {action.Connection}.");
            }
        }

        static void ValidateActionConnections(Process p, Action<string> error) {
            foreach (var action in p.Actions.Where(action => action.Connection != string.Empty).Where(action => p.Connections.All(c => c.Name != action.Connection))) {
                error($"The {action.Type} action references an invalid connection: {action.Connection}.");
            }
        }

        static void ValidateEntityConnections(Process p, Action<string> error) {
            foreach (var entity in p.Entities.Where(entity => p.Connections.All(c => c.Name != entity.Connection))) {
                error($"The {entity.Name} entity references an invalid connection: {entity.Connection}.");
            }
        }

        static void ValidateRelationships(Process p, Action<string> error, Action<string> warn) {
            // count check
            if (p.Entities.Count > 1 && p.Relationships.Count + 1 < p.Entities.Count) {
                error($"You have {p.Entities.Count} entities so you need {p.Entities.Count - 1} relationships. You have {p.Relationships.Count} relationships.");
            }

            //entity alias, name check, and if that passes, do field alias, name check
            foreach (var relationship in p.Relationships) {
                var problem = false;

                // validate (and modify) left side
                Entity leftEntity;
                if (p.TryGetEntity(relationship.LeftEntity, out leftEntity)) {
                    relationship.Summary.LeftEntity = leftEntity;
                    foreach (var leftField in relationship.GetLeftJoinFields()) {
                        Field field;
                        if (leftEntity.TryGetField(leftField, out field)) {
                            relationship.Summary.LeftFields.Add(field);
                        } else {
                            error($"A relationship references a left-field that doesn't exist: {leftField}");
                            problem = true;
                        }
                    }
                } else {
                    error($"A relationship references a left-entity that doesn't exist: {relationship.LeftEntity}");
                    problem = true;
                }

                //validate (and modify) right side
                Entity rightEntity;
                if (p.TryGetEntity(relationship.RightEntity, out rightEntity)) {
                    relationship.Summary.RightEntity = rightEntity;
                    foreach (var rightField in relationship.GetRightJoinFields()) {
                        Field field;
                        if (rightEntity.TryGetField(rightField, out field)) {
                            relationship.Summary.RightFields.Add(field);
                        } else {
                            error($"A relationship references a right-field that doesn't exist: {rightField}");
                            problem = true;
                        }
                    }
                } else {
                    error($"A relationship references a right-entity that doesn't exist: {relationship.RightEntity}");
                    problem = true;
                }

                //if everything is cool, set the foreign key flags
                if (!problem && relationship.Summary.IsAligned()) {
                    for (var i = 0; i < relationship.Summary.LeftFields.Count; i++) {
                        var leftField = relationship.Summary.LeftFields[i];
                        var rightField = relationship.Summary.RightFields[i];

                        leftField.KeyType |= KeyType.Foreign;
                        if (!leftField.Output) {
                            warn($"Foreign key {leftField.Alias} on left side must be output. Overriding output to true.");
                            leftField.Output = true;
                        }

                        if (leftField.Type != rightField.Type) {
                            warn($"The {leftField.Alias} and {rightField.Alias} relationship fields do not have the same type.");
                        }
                    }
                }

            }

        }


        static void ValidateDuplicateFields(Process p, Action<string> error) {
            var fieldDuplicates = p.Entities
                .SelectMany(e => e.GetAllFields())
                .Where(f => !f.PrimaryKey && !f.System)
                .Concat(p.CalculatedFields)
                .GroupBy(f => f.Alias.ToLower())
                .Where(group => @group.Count() > 1)
                .Select(group => @group.Key)
                .ToArray();
            foreach (var duplicate in fieldDuplicates) {
                error($"The entity field '{duplicate}' occurs more than once. Remove, alias, or prefix one.");
            }
        }


        static void ValidateDuplicateEntities(Process p, Action<string> error) {
            var entityDuplicates = p.Entities
                .GroupBy(e => e.Alias)
                .Where(group => @group.Count() > 1)
                .Select(group => @group.Key)
                .ToArray();
            foreach (var duplicate in entityDuplicates) {
                error($"The '{duplicate}' entity occurs more than once. Remove or alias one.");
            }
        }


        static void ValidateShouldRuns(Process p, Action<string> error, Action<string> warn) {
            foreach (var entity in p.Entities) {
                foreach (var field in entity.GetAllFields().Where(f => f.Transforms.Any(t => t.RunField != string.Empty))) {
                    foreach (var t in field.Transforms.Where(t => t.RunField != string.Empty)) {
                        Field runField;
                        if (entity.TryGetField(t.RunField, out runField)) {
                            var runValue = runField.Type == "bool" && t.RunValue == Constants.DefaultSetting ? "true" : t.RunValue;
                            try {
                                var value = Constants.ConversionMap[runField.Type](runValue);
                                t.ShouldRun = row => Utility.Evaluate(row[runField], t.RunOperator, value);
                            } catch (Exception ex) {
                                error($"Trouble converting {runValue} to {runField.Type}. {ex.Message}");
                            }
                        } else {
                            warn($"Run Field {t.RunField} does not exist in {entity.Alias}, so it will not be evaluated.");
                        }
                    }
                }
            }
        }


        static void ValidateSearchTypes(Process p, Action<string> error) {
            foreach (var name in p.GetAllFields().Select(f => f.SearchType).Distinct()) {
                if (p.SearchTypes.All(st => st.Name != name)) {
                    error($"Search type {name} is invalid.");
                }
            }
        }

        static void ValidateTransform(IContext context, Transform lastTransform, Action<string> error) {

            var t = context.Transform;
            var input = context.Process.ParametersToFields(context.Transform.Parameters, context.Field).First();

            // check input types
            switch (t.Method) {
                case "timezone":
                case "datepart":
                case "next":
                case "timeago":
                case "timeahead":
                case "last":
                    if (input.Type != "datetime") {
                        error($"The {t.Method} expects a datetime input, but {input.Alias} is {input.Type}.");
                    }
                    break;
                case "toyesno":
                    if (input.Type != "bool") {
                        error($"The {t.Method} expects a bool input, but {input.Alias} is {input.Type}.");
                    }
                    break;
                case "totime":
                    if (!input.Type.In("double", "single", "real", "decimal", "float")) {
                        error($"The {t.Method} expects an irrational (non-whole) numeric input like a double, single, real, decimal, or float type.");
                    }
                    break;
                case "contains":
                case "decompress":
                case "htmldecode":
                case "insert":
                case "left":
                case "padleft":
                case "padright":
                case "regexreplace":
                case "remove":
                case "replace":
                case "right":
                case "splitlength":
                case "substring":
                case "tolower":
                case "lower":
                case "toupper":
                case "upper":
                case "trim":
                case "trimend":
                case "trimstart":
                case "xmldecode":
                case "fileext":
                case "filepath":
                case "filename":
                case "xpath":
                    if (input.Type != "string") {
                        error($"The {t.Method} expects a string input. {input.Alias} is {input.Type}.");
                    }
                    break;
                case "tostring":
                    if (input.Type == "string") {
                        error($"The {t.Method} method is already a string.");
                    }
                    break;
            }

            // check parameters
            switch (t.Method) {
                case "shorthand":
                    if (string.IsNullOrEmpty(context.Transform.T)) {
                        error("The shorthand transform requires t attribute.");
                    }
                    break;
                case "format":
                    if (!context.Transform.Parameters.Any()) {
                        error("The format transform requires parameters.  In long-hand, add <parameters/> collection, in short-hand, preceed format method with copy(field1,field2,etc).");
                    }

                    if (t.Format == string.Empty) {
                        error("The format transform requires a format parameter.");
                    } else {
                        if (t.Format.IndexOf('{') == -1) {
                            error("The format transform's format must contain a curly braced place-holder.");
                        } else if (t.Format.IndexOf('}') == -1) {
                            error("The format transform's format must contain a curly braced place-holder.");
                        }
                    }
                    break;
                case "left":
                case "right":
                    if (t.Length == 0) {
                        error($"The {t.Method} transform requires a length parameter.");
                    }
                    break;
                case "copy":
                    if (t.Parameter == string.Empty && !t.Parameters.Any()) {
                        error("The copy transform requires at least one parameter.");
                    }
                    break;
                case "fromsplit":
                case "fromxml":
                    if (!t.Parameters.Any()) {
                        error($"The {t.Method} transform requires a collection of output fields.");
                    }
                    if (t.Method == "fromsplit" && t.Separator == Constants.DefaultSetting) {
                        error("The fromsplit method requires a separator.");
                    }
                    break;
                case "padleft":
                    if (t.TotalWidth == 0) {
                        error("The padleft transform requires total width.");
                    }
                    if (t.PaddingChar == default(char)) {
                        error("The padleft transform requires a padding character.");
                    }
                    break;
                case "padright":
                    if (t.TotalWidth == 0) {
                        error("The padright transform requires total width.");
                    }
                    if (t.PaddingChar == default(char)) {
                        error("The padright transform requires a padding character.");
                    }
                    break;
                case "map":
                    if (t.Map == string.Empty) {
                        error("The map method requires a map method");
                    }
                    break;
                case "splitlength":
                    if (t.Separator == Constants.DefaultSetting) {
                        error("The splitlength transform requires a separator.");
                    }
                    break;
                case "insert":
                case "remove":
                    if (input.Type != "string") {
                        error($"The {t.Method} only works on strings.  {input.Alias} is {input.Type}.");
                    }
                    if (t.StartIndex == 0) {
                        error($"The {t.Method} transform requires a start-index greater than 0.");
                    }
                    if (t.Method == "insert" && t.Value == string.Empty) {
                        error($"The {t.Method} transform requires a value.");
                    }
                    break;
                case "contains":
                    if (t.Value == string.Empty) {
                        error("The contains validator requires a value.");
                    }
                    break;
                case "is":
                    if (t.Type == Constants.DefaultSetting) {
                        error("The is validator requires a type.");
                    }
                    break;
                case "trimstart":
                case "trimend":
                case "trim":
                    if (t.TrimChars == string.Empty) {
                        error("The {t.Transform} transform requires trim-chars.");
                    }
                    break;
                case "join":
                    if (t.Separator == Constants.DefaultSetting) {
                        error($"The {t.Method} transform requires a separator.");
                    }
                    break;
                case "timezone":
                    if (input.Type != "datetime") {
                        error($"The {t.Method} expects a datetime input, but {input.Alias} is {input.Type}.");
                    }
                    if (t.FromTimeZone == Constants.DefaultSetting) {
                        error($"The {t.Method} transform requires from-time-zone to be set.");
                    }
                    if (t.ToTimeZone == Constants.DefaultSetting) {
                        error($"The {t.Method} transform requires to-time-zone to be set.");
                    }
                    break;
                case "replace":
                    if (t.OldValue == string.Empty) {
                        error($"The {t.Method} transform requires an old-value.");
                    }
                    break;
                case "regexreplace":
                    if (t.Pattern == string.Empty) {
                        error($"The {t.Method} transform requires a pattern.");
                    }
                    break;
                case "next":
                case "last":
                    if (string.IsNullOrEmpty(t.DayOfWeek)) {
                        error($"The {t.Method} transform requires a day-of-week.");
                    }
                    break;
                case "javascript":
                case "js":
                    if (t.Script == string.Empty) {
                        error($"The {t.Method} transform requires a script.");
                    }
                    break;
                case "razor":
                    if (t.Template == string.Empty) {
                        error($"The {t.Method} transform requires a template.");
                    }
                    if (t.ContentType == string.Empty) {
                        t.ContentType = "raw"; //other would be html
                    }
                    break;
                case "any":
                    if (string.IsNullOrEmpty(t.Operator)) {
                        error("The any transform requires an operator.");

                    }
                    if (string.IsNullOrEmpty(t.Value)) {
                        error("The any transform requires a value.");
                    }
                    break;
                case "connection":
                    if (string.IsNullOrEmpty(t.Name)) {
                        error("The connection transform requires a name.");

                    }
                    if (string.IsNullOrEmpty(t.Property)) {
                        error("The connection transform requires a property.");
                    }
                    var props = typeof(Connection).GetRuntimeProperties().Where(prop => prop.GetCustomAttribute(typeof(CfgAttribute), true) != null).Select(prop => prop.Name).ToArray();
                    if (!t.Property.In(props)) {
                        error($"The connection property {t.Property} is not allowed.  The allowed properties are {(string.Join(", ", props))}.");
                    }
                    break;
                case "xpath":
                    if (string.IsNullOrEmpty(t.XPath)) {
                        error("The xpath transform requires a xpath expression (or a the name of a field that contains an xpath expression).");
                    }
                    if (!string.IsNullOrEmpty(t.NameSpace) && string.IsNullOrEmpty(t.Url)) {
                        error("If you set a namespace, you must also set the url that references the name space.");
                    }
                    break;
                default:
                    break;
            }

            // check output types
            if (context.Transform == lastTransform) {
                switch (lastTransform.Method) {
                    case "convert":
                        if (lastTransform.Type != Constants.DefaultSetting && lastTransform.Type != context.Field.Type) {
                            error($"The {context.Field.Alias} field is a {context.Field.Type}, but your last transform is converting to a {lastTransform.Type}.");
                        }
                        break;
                    case "next":
                    case "last":
                    case "timezone":
                        if (context.Field.Type != "datetime") {
                            error($"The {lastTransform.Method} returns a datetime, but {context.Field.Alias} is a {context.Field.Type}.");
                        }
                        break;
                    case "datepart":
                        var returnType = DatePartTransform.PartReturns[lastTransform.TimeComponent];
                        if (returnType != context.Field.Type) {
                            error($"The {lastTransform.Method} returns a {returnType}, but {context.Field.Alias} is a {context.Field.Type}.");
                        }
                        break;
                    case "filename":
                    case "filepath":
                    case "fileext":
                    case "totime":
                        if (context.Field.Type != "string") {
                            error($"The {lastTransform.Method} returns a string, but {context.Field.Alias} is a {context.Field.Type}.");
                        }
                        break;
                    case "any":
                        if (!context.Field.Type.StartsWith("bool")) {
                            error($"The {lastTransform.Method} returns a bool, but {context.Field.Alias} is a {context.Field.Type}.");
                        }
                        break;
                }
            }
        }

        static void ValidateTransforms(Process p, Action<string> error) {

            foreach (var e in p.Entities) {
                foreach (var f in e.GetAllFields()) {
                    if (f.Transforms.Any()) {
                        var lastTransform = f.Transforms.Last();
                        foreach (var t in f.Transforms) {
                            var context = new PipelineContext(new NullLogger(), p, e, f, t);
                            ValidateTransform(context, lastTransform, error);
                        }
                    }
                }
            }
            foreach (var f in p.CalculatedFields) {
                if (f.Transforms.Any()) {
                    var lastTransform = f.Transforms.Last();
                    foreach (var t in f.Transforms) {
                        //TODO: Once the calculated columns are broken into their own virtual entity, replace null with it.
                        var context = new PipelineContext(new NullLogger(), p, null, f, t);
                        ValidateTransform(context, lastTransform, error);
                    }
                }
            }

        }

    }
}