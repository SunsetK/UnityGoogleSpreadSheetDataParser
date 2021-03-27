using System;
using System.Collections.Generic;

namespace GoogleSpreadSheetParser
{
    [Serializable]
    public class ConfigData
    {
        public ConfigData()
        {
            DataPath = "";
            FilePath = "";
            EncodingKey = "";
            SpreadSheetDatas = new List<SpreadSheetData>();
        }

        public string FilePath
        {
            get; set;
        }

        public string DataPath
        {
            get; set;
        }

        public string EncodingKey
        {
            get; set;
        }

        public List<SpreadSheetData> SpreadSheetDatas
        {
            get; set;
        }
    }
}