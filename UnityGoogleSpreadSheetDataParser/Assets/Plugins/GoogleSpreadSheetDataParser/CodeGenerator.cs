using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleSpreadSheetParser
{
    public class CodeGenerator
    {
        private string[] _templeteDataTypes;
        private List<string> _templeteCodeLines = new List<string>();
        private Dictionary<string, string> _templeteDataLines = new Dictionary<string, string>();
        private int _editStartLineIndex = -1;

        public CodeGenerator(string path, string[] templeteDataTypes)
        {
            _templeteDataTypes = templeteDataTypes;
            ParseTempleteCodeLines(path);
        }

        public void GenerateByTsv(string tsvPath, string exportPath, string dataName)
        {
            var lines = GetLines(tsvPath);
            var dataTypes = GetColumns(lines[0]);
            var columnNames = GetColumns(lines[1]);
            var codeLines = GenerateCodeLine(dataName, dataTypes, columnNames);
            WriteFile(codeLines, exportPath);
        }

        private List<string> GenerateCodeLine(string dataName, string[] dataTypes, string[] columnNames)
        {
            List<string> result = new List<string>();
            int index = 0;
            foreach(var line in _templeteCodeLines)
            {
                if(index == _editStartLineIndex)
                {
                    for(int i = 0; i < columnNames.Length; i++)
                    {
                        var dataFormat = _templeteDataLines[dataTypes[i]];
                        result.Add(dataFormat.Replace("{0}", columnNames[i]));
                    }
                }

                result.Add(line.Replace("{0}", dataName));
                index++;
            }

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
            return value.Replace("\"", "").Split('\t').Select(t => ClearString(t)).ToArray();
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