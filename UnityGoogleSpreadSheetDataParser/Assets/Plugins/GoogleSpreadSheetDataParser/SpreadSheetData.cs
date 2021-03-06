using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleSpreadSheetParser
{
    [Serializable]
    public class SpreadSheetData
    {
        public string TableName
        {
            get; set;
        }

        public string UriKey
        {
            get; set;
        }

        public bool GenerateDataKey
        {
            get; set;
        }

        public bool GenerageEnum
        {
            get; set;
        }
    }
}