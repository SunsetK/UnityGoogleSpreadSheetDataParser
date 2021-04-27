using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GoogleSpreadSheetParser
{
    public static class DataKeyGenerator
    {
        private static readonly string __DataKeyCsFilePath = "Assets/Scripts/Constants/DataKeys.cs";
        private static List<string> _allCondeLine = new List<string>();
        private static List<string> _allDataLines = new List<string>();

        public static void Generate(string csvPath, string dataName)
        {
            _allCondeLine = GetLines(__DataKeyCsFilePath);
            _allDataLines = GetLines(csvPath);

            GenerateCode(dataName);

            WriteFile(_allCondeLine, __DataKeyCsFilePath);
        }

        private static void GenerateCode(string dataName)
        {
            bool isFoundDefinition = false;
            int startIndex = 0;
            int endIndex = 0;
            foreach(var line in _allCondeLine)
            {
                if(line.Contains($"public static class {dataName}"))
                {
                    isFoundDefinition = true;
                }
                else if(isFoundDefinition)
                {
                    if(line.Contains("}"))
                        break;
                }

                if(isFoundDefinition == false)
                    startIndex++;

                endIndex++;
            }

            if(isFoundDefinition)
                _allCondeLine.RemoveRange(startIndex, (endIndex - startIndex) + 1);

            startIndex = 2; // 시작지점 매직넘버 ;;
            _allCondeLine.Insert(startIndex++, $"        public static class {dataName}");
            _allCondeLine.Insert(startIndex++, $"        {{");

            var columnNames = GetColumns(_allDataLines[1]);
            int keyIndex = Array.IndexOf(columnNames, "Key");
            int stringKeyIndex = Array.IndexOf(columnNames, "StringKey");
            int noteyIndex = Array.IndexOf(columnNames, "Note");
            var columns = _allDataLines.GetRange(2, _allDataLines.Count - 2);
            foreach(var data in columns)
            {
                var column = GetColumns(data);
                string StringKey = column[stringKeyIndex];
                string note = null;

                if(noteyIndex > -1)
                    note = column[noteyIndex];

                if(string.IsNullOrEmpty(StringKey) == false)
                {
                    var Key = column[keyIndex];
                    if(note != null)
                        _allCondeLine.Insert(startIndex++, $"            /// <summary>{note}</summary>");

                    _allCondeLine.Insert(startIndex++, $"            public static readonly int {StringKey} = {Key};");
                }
            }
            _allCondeLine.Insert(startIndex++, $"        }}");
            _allCondeLine.Insert(startIndex++, $"");
        }

        private static string[] GetColumns(string value)
        {
            Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            var result = csvParser.Split(value).Select(t => ClearString(t)).ToArray();
            return csvParser.Split(value).Select(t => ClearString(t)).ToArray();
        }

        private static List<string> GetLines(string path)
        {
            var lines = new List<string>();
            using(StreamReader sr = new StreamReader(path))
            {
                var line = sr.ReadLine();
                while(line != null)
                {
                    lines.Add(line);
                    line = sr.ReadLine();
                }
            }

            return lines;
        }

        private static void WriteFile(IEnumerable<string> lines, string path)
        {
            using(StreamWriter sw = new StreamWriter(path, false))
            {
                foreach(var line in lines)
                    sw.WriteLine(line);
            }
        }

        private static string ClearString(string value)
        {
            int startIndex = value.IndexOf("/*");
            if(startIndex != -1)
                value = value.Remove(startIndex, value.Length - startIndex);

            startIndex = value.IndexOf("#");
            if(startIndex != -1)
                value = value.Remove(startIndex, value.Length - startIndex);

            value = value.Replace("\"", "");

            if(string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }
    }
}