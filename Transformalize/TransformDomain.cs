﻿using System.Collections.Generic;

namespace Transformalize {

    public static class TransformDomain {

        public static List<string> List = new List<string>{
            "abs",
            "add",
            "any",
            "append",
            "bytes",
            "bytesize",
            "camelize",
            "ceiling",
            "coalesce",
            "commonprefix",
            "commonprefixes",
            "compress",
            "concat",
            "connection",
            "convert",
            "copy",
            "cs",
            "csharp",
            "dasherize",
            "dateadd",
            "datediff",
            "datemath",
            "datepart",
            "decompress",
            "dehumanize",
            "distinct",
            "endswith",
            "eval",
            "exclude",
            "fileext",
            "filename",
            "filepath",
            "floor",
            "format",
            "formatphone",
            "formatxml",
            "frommetric",
            "fromroman",
            "geohashencode",
            "geohashneighbor",
            "hashcode",
            "htmldecode",
            "htmlencode",
            "humanize",
            "hyphenate",
            "iif",
            "in",
            "include",
            "insert",
            "invert",
            "isdefault",
            "isempty",
            "ismatch",
            "javascript",
            "join",
            "js",
            "last",
            "left",
            "lower",
            "map",
            "match",
            "matchcount",
            "multiply",
            "next",
            "now",
            "ordinalize",
            "padleft",
            "padright",
            "pascalize",
            "pluralize",
            "razor",
            "regexreplace",
            "remove",
            "replace",
            "right",
            "round",
            "rounddownto",
            "roundto",
            "roundupto",
            "singularize",
            "slice",
            "slugify",
            "splitlength",
            "startswith",
            "substring",
            "sum",
            "tag",
            "timeago",
            "timeahead",
            "timezone",
            "titleize",
            "tolower",
            "tometric",
            "toordinalwords",
            "toroman",
            "tostring",
            "totime",
            "toupper",
            "towords",
            "toyesno",
            "trim",
            "trimend",
            "trimstart",
            "underscore",
            "upper",
            "urlencode",
            "velocity",
            "vingetmodelyear",
            "vingetworldmanufacturer",
            "web",
            "xmldecode",
            "xpath"
        };

        private static HashSet<string> _hashSet;
        public static HashSet<string> HashSet => _hashSet ?? (_hashSet = new HashSet<string>(List));

        private static string _commaDelimited;
        public static string CommaDelimited => _commaDelimited ?? (_commaDelimited = string.Join(",", List));
    }
}