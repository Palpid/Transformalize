﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Transformalize.Libs.NLog;

namespace Transformalize.Main.Providers.File {

    public class Lines {

        private readonly FileSystemInfo _fileInfo;
        private readonly FileInspectionRequest _request;
        private readonly List<Line> _storage = new List<Line>();
        private char _bestDelimiter;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public Lines(FileSystemInfo fileInfo, FileInspectionRequest request) {
            _fileInfo = fileInfo;
            _request = request;
            _storage.AddRange(new LineLoader(fileInfo, request).Load());
        }

        public char FindDelimiter() {

            if (_bestDelimiter != default(char))
                return _bestDelimiter;

            var max = 0;
            var candidates = new Dictionary<char, int>();

            foreach (var delimiter in _request.Delimiters.Keys) {
                foreach (var line in _storage) {
                    var count = line.Values[delimiter].Length - 1;
                    if (count > 0 && _storage.All(l => l.Values[delimiter].Length - 1 == count)) {
                        candidates[delimiter] = count;
                        if (count > max) {
                            max = count;
                        }
                    }
                }
            }

            if (!candidates.Any()) {
                _log.Warn("Can't find a delimiter for {0}.  Defaulting to single column.", _fileInfo.Name);
                return default(char);
            }

            _bestDelimiter = candidates.First(kv => kv.Value.Equals(max)).Key;
            _log.Info("Delimiter is '{0}'", _bestDelimiter);
            return _bestDelimiter;
        }

        public Fields InitialFieldTypes() {

            var fields = new Fields();
            var delimiter = FindDelimiter();
            var firstLine = _storage[0];

            if (delimiter == default(char)) {
                var field = new Field(_request.DefaultType, _request.DefaultLength, FieldType.NonKey, true, string.Empty) {
                    Name = firstLine.Content
                };
                fields.Add(field);
                return fields;
            }

            var names = firstLine.Values[delimiter];

            for (var i = 0; i < names.Length; i++) {
                var name = names[i];
                var field = new Field(_request.DefaultType, _request.DefaultLength, FieldType.NonKey, true, string.Empty) {
                    Name = name
                };
                if (_storage.Any(x => x.Values[delimiter][i].Contains(delimiter) || _storage.Skip(1).All(y=> y.Quote != default(char) && y.Values[delimiter][i].StartsWith(y.Quote.ToString(CultureInfo.InvariantCulture)) && y.Values[delimiter][i].EndsWith(y.Quote.ToString(CultureInfo.InvariantCulture))))) {
                    field.QuotedWith = _storage.Skip(1).First().Quote;
                }
                fields.Add(field);
            }

            return fields;
        }

    }
}