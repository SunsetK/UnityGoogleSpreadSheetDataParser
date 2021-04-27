using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GoogleSpreadSheetParser
{
    public class CodeGenerator
    {
        private string[] _templeteDataTypes;
        private List<string> _templeteCodeLines = new List<string>();
        private Dictionary<string, string> _templeteDataLines = new Dictionary<string, string>();
        private int _editStartLineIndex = -1;
        private List<string> _allDataLines = new List<string>();
        private bool _generateEnum;

        public CodeGenerator(string path, string[] templeteDataTypes)
        {
            _templeteDataTypes = templeteDataTypes;
            ParseTempleteCodeLines(path);
        }

        public void GenerateByCsv(string csvPath, string exportPath, string dataName, bool generateEnum)
        {
            _allDataLines = GetLines(csvPath);
            _generateEnum = generateEnum;
            var dataTypes = GetColumns(_allDataLines[0]);
            var columnNames = GetColumns(_allDataLines[1]);
            var codeLines = GenerateCodeLine(dataName, dataTypes, columnNames);
            WriteFile(codeLines, exportPath);
        }

        private List<string> GetLineDistinctColumns(int index)
        {
            List<string> result = new List<string>();
            foreach(var c in _allDataLines.GetRange(2, _allDataLines.Count - 2))
            {
                var columns = GetColumns(c);
                var value = columns[index];
                if(result.Any(t => t == value) == false)
                {
                    if(string.IsNullOrEmpty(value) == false)
                        result.Add(value);
                }
            }

            return result;
        }

        private List<string> GenerateCodeLine(string dataName, string[] dataTypes, string[] columnNames)
        {
            var result = new List<string>();
            int index = 0;
            var tempEnumLines = new Dictionary<string, string[]>();

            foreach(var line in _templeteCodeLines)
            {
                if(index == _editStartLineIndex)
                {
                    for(int i = 0; i < columnNames.Length; i++)
                    {
                        string dataFormat = "";
                        if(_templeteDataLines.ContainsKey(dataTypes[i]))
                        {
                            dataFormat = _templeteDataLines[dataTypes[i]];
                        }
                        else
                        {
                            // enum type 선언
                            var typeName = dataTypes[i];
                            dataFormat = _templeteDataLines["{1}"];
                            tempEnumLines.Add(typeName, GetLineDistinctColumns(i).ToArray());
                        }

                        result.Add(dataFormat.Replace("{1}", dataTypes[i]).Replace("{2}", columnNames[i]));
                    }
                }
                else if(index == _editStartLineIndex + 1)
                {
                    if(_generateEnum)
                    {
                        foreach(var enumData in tempEnumLines)
                            result.AddRange(GetEnumCodeLine(enumData));
                    }
                }

                result.Add(line.Replace("{0}", dataName));
                index++;
            }

            return result;
        }

        private List<string> GetEnumCodeLine(KeyValuePair<string, string[]> data)
        {
            // 땜빵임 ㅋㅋ
            var result = new List<string>();
            result.Add($"    public enum {data.Key}");
            result.Add("    {");
            foreach(var d in data.Value)
                result.Add($"        {d},");

            result.Add("    }");

            return result;
        }

        private void WriteFile(IEnumerable<string> lines, string path)
        {
            using(StreamWriter sw = new StreamWriter(path, false))
            {
                foreach(var line in lines)
                    sw.WriteLine(line);
            }
        }

        private string[] GetColumns(string value)
        {
            Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            var result = csvParser.Split(value).Select(t => ClearString(t)).ToArray();
            return csvParser.Split(value).Select(t => ClearString(t)).ToArray();
        }

        private void ParseTempleteCodeLines(string path)
        {
            foreach(var line in GetLines(path))
            {
                var str = ClearString(line);
                if(string.IsNullOrEmpty(str) == false)
                {
                    var templeteDataType = _templeteDataTypes.FirstOrDefault(t => line.Contains(t));
                    if(templeteDataType != null)
                    {
                        if(_editStartLineIndex == -1)
                            _editStartLineIndex = _templeteCodeLines.Count();

                        _templeteDataLines.Add(templeteDataType, str);
                    }
                    else
                    {
                        _templeteCodeLines.Add(str);
                    }
                }
            }
        }

        private string ClearString(string value)
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

        private List<string> GetLines(string path)
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
    }
}