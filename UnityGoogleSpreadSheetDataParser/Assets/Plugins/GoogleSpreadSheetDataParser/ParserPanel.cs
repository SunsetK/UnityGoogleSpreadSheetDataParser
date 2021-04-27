#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetParser
{
    public class ParserPanel : EditorWindow
    {
        private CodeGenerator _codeGenerator;
        private ConfigData _configData;
        private string _configDataPath;

        public void OnEnable()
        {
            _configDataPath = $"{Application.dataPath}/{ConfigConstants.ConfigFilePath}/{ConfigConstants.ConfigFileName}";
            _configData = LoadFile(_configDataPath);
            _codeGenerator = new CodeGenerator(ConfigConstants.TempleteFilePath, ConfigConstants.TempleteDataTypes);
        }

        [MenuItem("Tools/DataParser")]
        static private void Init()
        {
            ParserPanel window = EditorWindow.GetWindow<ParserPanel>();
            window.position = new Rect(100, 100, 1100, 500);
        }

        public void OnGUI()
        {
            GUILayout.Label("[Parser Config]", EditorStyles.boldLabel);
            _configData.DataPath = EditorGUILayout.TextField("Data Path", _configData.DataPath);
            _configData.FilePath = EditorGUILayout.TextField("File(.csv) Path", _configData.FilePath);
            _configData.EncodingKey = EditorGUILayout.TextField("Encoding Key", _configData.EncodingKey);

            string EditTemplete = $"Edit Templete Data Code";
            if(GUILayout.Button(EditTemplete, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(EditTemplete)).x + 10)))
                AssetDatabase.OpenAsset((MonoScript)AssetDatabase.LoadAssetAtPath(ConfigConstants.TempleteFilePath, typeof(MonoScript)));

            GUILayout.Label("[Datas]", EditorStyles.boldLabel);

            int index = 0;
            GUILayout.BeginScrollView(new Vector2(0, 300), GUILayout.Width(1100), GUILayout.Height(300));
            foreach(var data in _configData.SpreadSheetDatas)
            {
                GUILayout.BeginHorizontal();

                string TableNameLabel = $"[{index.ToString("D2")}] DataName ";
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(TableNameLabel)).x;
                data.TableName = EditorGUILayout.TextField(TableNameLabel, data.TableName, GUILayout.Width(250));

                string UriKeyLabel = "UriKey ";
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(UriKeyLabel)).x;
                data.UriKey = EditorGUILayout.TextField(UriKeyLabel, data.UriKey, GUILayout.Width(400));

                string OpenButton = "Open";
                if(GUILayout.Button(OpenButton, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(OpenButton)).x + 10)))
                    Application.OpenURL(string.Format(ConfigConstants.Uri, data.UriKey));

                string ParseButton = "Parse";
                if(GUILayout.Button(ParseButton, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(OpenButton)).x + 10)))
                    EditorCoroutineUtility.StartCoroutine(Parse(data), this);

                string RemoveButton = "Remove";
                if(GUILayout.Button(RemoveButton, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(RemoveButton)).x + 10)))
                    _configData.SpreadSheetDatas.Remove(data);

                string generageDataKeyToggle = "Generate DataKey";
                float originalValue = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(generageDataKeyToggle)).x + 1;
                data.GenerateDataKey = EditorGUILayout.Toggle(generageDataKeyToggle, data.GenerateDataKey);
                EditorGUIUtility.labelWidth = originalValue;

                string generageEnumToggle = "Generate Enum";
                float originalValue2 = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(generageEnumToggle)).x + 10;
                data.GenerageEnum = EditorGUILayout.Toggle(generageEnumToggle, data.GenerageEnum);
                EditorGUIUtility.labelWidth = originalValue2;

                GUILayout.EndHorizontal();

                index++;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Space(300f);
            if(GUILayout.Button("Add Item"))
                _configData.SpreadSheetDatas.Add(new SpreadSheetData());
            GUILayout.Space(300f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Help"))
                Application.OpenURL("https://docs.google.com/document/d/1BqD1TY5IbmcEdXTd7Nc9LBBJHt3zanA4u9XQu5o2E88/edit?usp=sharing");

            if(GUILayout.Button("Save"))
                SaveConfig();

            if(GUILayout.Button("Parse All"))
                EditorCoroutineUtility.StartCoroutine(ParseAll(_configData.SpreadSheetDatas), this);

            GUILayout.EndHorizontal();
        }

        private IEnumerator Parse(SpreadSheetData data)
        {
            yield return DownloadCsv(data);

            var csvPath = $"{Application.dataPath}{_configData.FilePath}{data.TableName}{ConfigConstants.OriginalExtensionName}";
            var exportPath = $"{Application.dataPath}{_configData.DataPath}{data.TableName}{ConfigConstants.DataCodeExtension}";
            _codeGenerator.GenerateByCsv(csvPath, exportPath, data.TableName, data.GenerageEnum);

            if(data.GenerateDataKey)
                DataKeyGenerator.Generate(csvPath, data.TableName);

            Debug.Log($"{exportPath} Save.");
        }

        private IEnumerator ParseAll(IEnumerable<SpreadSheetData> datas)
        {
            foreach(var data in datas)
                yield return Parse(data);

            Debug.Log($"{datas.Count()}개 데이터 파싱 완료");
        }

        private void SaveConfig()
        {
            SaveFile(_configDataPath, _configData);
        }

        private IEnumerator DownloadCsv(SpreadSheetData data)
        {
            if(string.IsNullOrEmpty(data.UriKey) || string.IsNullOrEmpty(data.TableName))
            {
                Debug.Log("uriKey또는 TableName이 비어있어 서버로부터 csv데이터를 다운받을 수 없습니다.");
            }
            else
            {
                string str = string.Format(ConfigConstants.DownloadPath, data.UriKey, data.TableName);

                Debug.Log("[" + data.TableName + "] Waite for Download... : " + str);

                WWW www = new WWW(str);
                yield return www;

                Debug.Log("[" + data.TableName + "] Download Success!");

                try
                {
                    var path = $"{Application.dataPath}{_configData.FilePath}{data.TableName}{ConfigConstants.OriginalExtensionName}";
                    Debug.Log($"{path} Save.");
                    System.IO.File.WriteAllBytes($"{Application.dataPath}{_configData.FilePath}{data.TableName}{ConfigConstants.OriginalExtensionName}", www.bytes);
                }
                catch(Exception e)
                {
                    Debug.Log($"데이터 파일 저장실패 : {e.ToString()}");
                }
                yield return null;
            }
        }

        private void SaveFile(string path, object target)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                Stream file = File.Create(path);
                bf.Serialize(file, target);
                file.Close();

                Debug.Log("데이터 파일 저장 완료!");
            }
            catch
            {
                Debug.Log("데이터 파일 저장 실패!");
            }
        }

        private ConfigData LoadFile(string path)
        {
            ConfigData target = null;
            if(File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Stream streamRead = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                target = (ConfigData)bf.Deserialize(streamRead);
                streamRead.Close();

                Debug.Log("환경설정 데이터 로드 성공!");
            }
            else
            {
                target = new ConfigData();

                Debug.Log("환경설정 데이터가 존재하지않습니다. 데이터를 새로 만듭니다.");
            }

            return target;
        }
    }
}

#endif