#if UNITY_IOS
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CrazySummerLab
{
    [CustomEditor(typeof(AppTrackingSettings))]
    public class AppTrackingInspector : UnityEditor.Editor
    {
        private static string SETTINGS_ASSET_PATH = "Assets/CrazySummerLab.AppTracking/Scripts/Editor/AppTrackingSettings.asset";

        private static AppTrackingSettings settings;
        public static AppTrackingSettings Settings
        {
            get
            {
                if(settings == null)
                {
                    settings = (AppTrackingSettings)AssetDatabase.LoadAssetAtPath(SETTINGS_ASSET_PATH, typeof(AppTrackingSettings));
                    if (settings == null)
                        settings = CreateDefaultSettings();
                }
                return settings;
            }
        }
        private static AppTrackingSettings CreateDefaultSettings()
        {
            AppTrackingSettings asset = CreateInstance(typeof(AppTrackingSettings)) as AppTrackingSettings;
            AssetDatabase.CreateAsset(asset, SETTINGS_ASSET_PATH);
            CreateLocalizationDictionary();
            return asset;
        }
        private static void CreateLocalizationDictionary()
        {
            if (settings != null && (settings.localizedPopupMessageDictionary == null || settings.localizedPopupMessageDictionary.Count == 0))
            {
                settings.useLocalizationValues = true;
                settings.localizedPopupMessageDictionary = new LanguagesDictionary
                {
                    { (int)SystemLanguage.English, "Pressing 'Allow' uses device info for more relevant ad content" },
                    { (int)SystemLanguage.French, "'Autoriser' permet d'utiliser les infos du téléphone pour afficher des contenus publicitaires plus pertinents" },
                    { (int)SystemLanguage.German, "'Erlauben' drücken benutzt Gerätinformationen für relevantere Werbeinhalte" },
                    { (int)SystemLanguage.Catalan, "Prement 'Permetre', s'utilitza la informació del dispositiu per a obtindre contingut publicitari més rellevant" },
                    { (int)SystemLanguage.Spanish, "Presionando 'Permitir', se usa la información del dispositivo para obtener contenido publicitario más relevante" },
                    { (int)SystemLanguage.Chinese, "点击'允许'以使用设备信息获得更加相关的广告内容" },
                    { (int)SystemLanguage.Japanese, "'許可'をクリックすることで、デバイス情報を元により最適な広告を表示することができます" },
                    { (int)SystemLanguage.Korean, "'허용'을 누르면 더 관련성 높은 광고 콘텐츠를 제공하기 위해 기기 정보가 사용됩니다" }
                };
            }
            if (settings != null && settings.localizedPopupMessageDictionary != null && settings.localizedPopupMessageDictionary.ContainsKey((int)SystemLanguage.ChineseSimplified))
            {
                settings.localizedPopupMessageDictionary.Remove((int)SystemLanguage.ChineseSimplified);
            }

            if (settings != null && settings.localizedPopupMessageDictionary != null && settings.localizedPopupMessageDictionary.ContainsKey((int)SystemLanguage.ChineseTraditional))
            {
                settings.localizedPopupMessageDictionary.Remove((int)SystemLanguage.ChineseTraditional);
            }
            AssetDatabase.ForceReserializeAssets(new string[] { SETTINGS_ASSET_PATH });
        }

        #region custom menu item and inspector GUI
        private int languageKeySelectedIndex = (int)SystemLanguage.English;
        private Vector2 localizedPopupMessageScrollPosition;
        [MenuItem("CrazySummerLab/App Tracking/AppTrackingSettings", false, 0)]
        static void SelectSettings()
        {
            Selection.activeObject = Settings;
        }
        public override void OnInspectorGUI()
        {
            settings = target as AppTrackingSettings;
            CreateLocalizationDictionary();

            FontStyle fontStyle = EditorStyles.label.fontStyle;
            bool wordWrap = GUI.skin.textField.wordWrap;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            GUI.skin.textField.wordWrap = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("App Tracking Transparency", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Present the app-tracking authorization request to the end user with this customizable message", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // TODO
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useLocalizationValues"), new GUIContent("Use Localization Values", "Toggle ON to use localization values. Otherwise English will default. If you have your own localization solution, be aware as they might conflict"), true);

            // Show all available languages
            languageKeySelectedIndex = EditorGUILayout.Popup("Language", languageKeySelectedIndex, Enum.GetNames(typeof(SystemLanguage)));
            SystemLanguage selectedLanguage = Enum.GetValues(typeof(SystemLanguage)).Cast<SystemLanguage>().ToList()[languageKeySelectedIndex];
            localizedPopupMessageScrollPosition = EditorGUILayout.BeginScrollView(localizedPopupMessageScrollPosition, GUILayout.Height(80));

            // Language does not have localized value. Create one and default to EN
            if (!settings.localizedPopupMessageDictionary.ContainsKey((int)selectedLanguage))
            {
                settings.localizedPopupMessageDictionary.Add((int)selectedLanguage, "Pressing 'Allow' uses device info for more relevant ad content");
            }

            settings.localizedPopupMessageDictionary[(int)selectedLanguage] = GUILayout.TextArea(settings.localizedPopupMessageDictionary[(int)selectedLanguage], GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save All Languages", GUILayout.Width(300), GUILayout.Height(50)))
            {
                AssetDatabase.ForceReserializeAssets(new string[] { SETTINGS_ASSET_PATH });
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            DrawHorizontalLine(Color.grey);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetwork", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetworkIds specified will be automatically added to your Info.plist file.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("NOTICE: This plugin does not include the ability to show ads.\nYou will need to use your favorite ads platform SDK.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load SkAdNetworkIds from file (xml or json)", GUILayout.Width(300), GUILayout.Height(50)))
            {
                LoadSkAdNetworkIdsFromFile();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SkAdNetworkIds"), new GUIContent("SkAdNetworkIds"), true);

            serializedObject.ApplyModifiedProperties();
            GUI.skin.textField.wordWrap = wordWrap;
            EditorStyles.label.fontStyle = fontStyle;
        }

        private void LoadSkAdNetworkIdsFromFile()
        {
            SerializedProperty networkIdsSerializedProperty = serializedObject.FindProperty("SkAdNetworkIds");
            string path = EditorUtility.OpenFilePanel("Select SkAdNetworkIds file", "", "txt,json,xml");
            if (path.Length != 0)
            {
                int addedIds = 0;
                string fileContent = File.ReadAllText(path);
                var regex = new Regex(@"[a-z0-9]+\.skadnetwork");
                MatchCollection collection = regex.Matches(fileContent);
                foreach (Match match in collection)
                {
                    string skAdNetworkId = match.Value;
                    bool alreadyAdded = false;
                    int listSize = networkIdsSerializedProperty.arraySize;

                    if (listSize > 0)
                    {
                        for (int i = 0; i < listSize && !alreadyAdded; i++)
                        {
                            if (networkIdsSerializedProperty.GetArrayElementAtIndex(i).stringValue == skAdNetworkId)
                            {
                                alreadyAdded = true;
                            }
                        }
                    }

                    if (!alreadyAdded)
                    {
                        networkIdsSerializedProperty.InsertArrayElementAtIndex(Mathf.Max(0, listSize - 1));
                        networkIdsSerializedProperty.GetArrayElementAtIndex(Mathf.Max(0, listSize - 1)).stringValue = skAdNetworkId;
                        addedIds++;
                    }
                }

                if (addedIds > 0)
                {
                    EditorUtility.DisplayDialog("SkAdNetwork IDs import", string.Format("Successfully added {0} SkAdNetwork IDs", addedIds), "Done");
                }
                else
                {
                    EditorUtility.DisplayDialog("SkAdNetwork IDs import", "No new SkAdNetwork IDs found to be added", "Done");
                }
            }
        }

        private void DrawHorizontalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        #endregion
    }
}
#endif