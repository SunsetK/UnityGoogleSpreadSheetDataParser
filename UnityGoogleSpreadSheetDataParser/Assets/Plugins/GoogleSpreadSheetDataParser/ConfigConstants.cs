namespace GoogleSpreadSheetParser
{
    public class ConfigConstants
    {
        public static readonly string TempleteFilePath = "Assets/Plugins/GoogleSpreadSheetDataParser/Templete.cs";
        public static readonly string ConfigFilePath = "Plugins/GoogleSpreadSheetDataParser";
        public static readonly string ConfigFileName = "DataParser.config";
        public static readonly string DownloadPath = "https://docs.google.com/spreadsheets/d/{0}/export?sheet={1}&format=csv";
        public static readonly string Uri = "https://docs.google.com/spreadsheets/d/{0}/edit?usp=sharing";
        public static string[] TempleteDataTypes = new string[] { "int[]", "long[]", "float[]", "double[]", "string[]", "bool[]", "int", "long", "float", "double", "string", "bool", "{1}" };

        public static readonly string OriginalExtensionName = ".csv";
        public static readonly string DataCodeExtension = ".cs";
    }
}