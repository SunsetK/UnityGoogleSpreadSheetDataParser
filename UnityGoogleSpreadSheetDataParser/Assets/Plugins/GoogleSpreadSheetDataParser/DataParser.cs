using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GoogleSpreadSheetParser
{
#if UNITY_EDITOR

    public class DataParser
    {
        public static T[] Parse<T>(string path)
        {
            var allLines = GetLines(path);
            var typeLine = allLines[0];
            var nameLine = allLines[1];
            var dataLines = allLines.Where(t => t != typeLine && t != nameLine);

            return GetDatas<T>(nameLine, typeLine, dataLines).ToArray();
        }

        private static IEnumerable<T> GetDatas<T>(string nameLine, string typeLine, IEnumerable<string> dataLines)
        {
            var columnNames = GetColumns(nameLine);
            var columnTypes = GetColumns(typeLine);
            foreach(var line in dataLines)
            {
                var columns = GetColumns(line);
                var type = typeof(T);
                var inst = (T)Activator.CreateInstance(type);

                int index = 0;
                foreach(var name in columnNames)
                {
                    PropertyInfo property = type.GetProperty(name);
                    switch(columnTypes[index])
                    {
                        case "int":
                            property.SetValue(inst, GetIntValue(columns[index]), null);
                            break;

                        case "long":
                            property.SetValue(inst, GetLongValue(columns[index]), null);
                            break;

                        case "float":
                            property.SetValue(inst, GetFloatValue(columns[index]), null);
                            break;

                        case "double":
                            property.SetValue(inst, GetDoubleValue(columns[index]), null);
                            break;

                        case "bool":
                            property.SetValue(inst, GetBoolValue(columns[index]) ? true : false, null);
                            break;

                        case "string":
                            property.SetValue(inst, columns[index], null);
                            break;

                        case "int[]":
                            property.SetValue(inst, GetIntArrayValue(columns[index]), null);
                            break;

                        case "long[]":
                            property.SetValue(inst, GetLongArrayValue(columns[index]), null);
                            break;

                        case "float[]":
                            property.SetValue(inst, GetFloatArrayValue(columns[index]), null);
                            break;

                        case "double[]":
                            property.SetValue(inst, GetDoubleArrayValue(columns[index]), null);
                            break;

                        case "bool[]":
                            property.SetValue(inst, GetBoolArrayValue(columns[index]), null);
                            break;

                        case "string[]":
                            if(columns[index] != null)
                            {
                                Regex cSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                                var result = cSVParser.Split(columns[index]);
                                property.SetValue(inst, result, null);
                            }
                            else
                            {
                                property.SetValue(inst, new string[] { }, null);
                            }
                            break;

                        default:
                            // TODO 나중에 열거형 배열 필요해지면 그때 만들기....
                            property.SetValue(inst, GetEnumValue(columnTypes[index], columns[index]), null);
                            break;
                    }

                    index++;
                }

                yield return inst;
            }
        }

        private static int GetEnumValue(string typeName, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return 0;
            }
            else
            {
                Type type = Type.GetType($"MukbangSimulation.Data.{typeName}"); // 상황에 따라 경로가 다를 수 있을까?
                return (int)Enum.Parse(type, value);
            }
        }

        private static int GetIntValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return 0;
            else
                return Convert.ToInt32(value);
        }

        private static long GetLongValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return 0;
            else
                return Convert.ToInt64(value);
        }

        private static float GetFloatValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return 0f;
            else
                return Convert.ToSingle(value);
        }

        private static double GetDoubleValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return 0d;
            else
                return Convert.ToDouble(value);
        }

        private static bool GetBoolValue(string value)
        {
            if(string.IsNullOrEmpty(value))
                return false;
            else
                return (value.ToLower() is "true") ? true : false;
        }

        private static int[] GetIntArrayValue(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new int[] { };
            }
            else
            {
                var values = value.Split(',').Select(t => GetIntValue(t));
                return values.ToArray();
            }
        }

        private static long[] GetLongArrayValue(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new long[] { };
            }
            else
            {
                var values = value.Split(',').Select(t => GetLongValue(t));
                return values.ToArray();
            }
        }

        private static float[] GetFloatArrayValue(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new float[] { };
            }
            else
            {
                var values = value.Split(',').Select(t => GetFloatValue(t));
                return values.ToArray();
            }
        }

        private static double[] GetDoubleArrayValue(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new double[] { };
            }
            else
            {
                var values = value.Split(',').Select(t => GetDoubleValue(t));
                return values.ToArray();
            }
        }

        private static bool[] GetBoolArrayValue(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return new bool[] { };
            }
            else
            {
                var values = value.Split(',').Select(t => GetBoolValue(t));
                return values.ToArray();
            }
        }

        private static string[] GetColumns(string value)
        {
            Regex cSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            var result = cSVParser.Split(value).Select(t => ClearString(t)).ToArray();
            return cSVParser.Split(value).Select(t => ClearString(t)).ToArray();
        }

        private static List<string> GetLines(string path)
        {
            var lines = new List<string>();
            var file = Resources.Load(path) as TextAsset;
            StringReader reader = new StringReader(file.text);

            while(reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                lines.Add(line);
            }

            return lines;
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

#endif
}