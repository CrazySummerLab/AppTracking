#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace CrazySummerLab
{
    /// <summary>
    /// PostProcessor script to automatically fill all required dependencies
    /// </summary>
    public class AppTrackingPostProcessor
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PBXProject project = new PBXProject();
                string projectPath = PBXProject.GetPBXProjectPath(buildPath);
                project.ReadFromFile(projectPath);
               
                // add write info plist
                WriteInfoPlist(buildPath);
                // If loaded, add `AppTrackingTransparency` Framework
                WriteFramework(project);
                // Localization Popup Message
                WriteLocalization(buildPath, project);
                // write all project file
                project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
            }
        }

        private static void WriteInfoPlist(string buildPath)
        {
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(buildPath + "/Info.plist"));
            // Get root
            PlistElementDict rootDict = plist.root;
            // Add NSUserTrackingUsageDescription
            rootDict.SetString("NSUserTrackingUsageDescription", AppTrackingInspector.Settings.localizedPopupMessageDictionary[(int)SystemLanguage.English]);
            // Check if SKAdNetworkItems already exists
            PlistElementArray SKAdNetworkItems = null;
            if (rootDict.values.ContainsKey("SKAdNetworkItems"))
            {
                try
                {
                    SKAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementArray: {0}", e.Message));
                }
            }
            // If not exists, create it
            if (SKAdNetworkItems == null)
                SKAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");
            if (AppTrackingInspector.Settings.SkAdNetworkIds != null)
            {
                List<string> networkIdsWithoutDuplicates = AppTrackingInspector.Settings.SkAdNetworkIds.Distinct().ToList();
                string plistContent = File.ReadAllText(buildPath + "/Info.plist");
                for (int i = 0; i < networkIdsWithoutDuplicates.Count; i++)
                {
                    if (!plistContent.Contains(networkIdsWithoutDuplicates[i]))
                    {
                        PlistElementDict SKAdNetworkIdentifierDict = SKAdNetworkItems.AddDict();
                        SKAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", networkIdsWithoutDuplicates[i]);
                    }
                }
            }
            File.WriteAllText(buildPath + "/Info.plist", plist.WriteToString());
        }

        private static void WriteFramework(PBXProject project)
        {
            string targetId = project.TargetGuidByName("Unity-iPhone");
#if UNITY_2019_3_OR_NEWER
            targetId = project.GetUnityFrameworkTargetGuid();
#endif
            project.AddFrameworkToProject(targetId, "AppTrackingTransparency.framework", true);
            project.AddFrameworkToProject(targetId, "AdSupport.framework", false);
            project.AddFrameworkToProject(targetId, "StoreKit.framework", false);
        }

        private static void WriteLocalization(string buildPath, PBXProject project)
        {
            string mainTargetId = project.TargetGuidByName("Unity-iPhone");
#if UNITY_2019_3_OR_NEWER
            mainTargetId = project.GetUnityMainTargetGuid();
#endif

            foreach (var localizedMessagePair in AppTrackingInspector.Settings.localizedPopupMessageDictionary)
            {
                int systemLanguage = localizedMessagePair.Key;
                string localizedMessageString = localizedMessagePair.Value;

                AddUserTrackingDescriptionLocalizedString(localizedMessageString, Get2LetterISOCodeFromSystemLanguage((SystemLanguage)systemLanguage), buildPath, project, mainTargetId);
            }
        }

        private static void AddUserTrackingDescriptionLocalizedString(string localizedUserTrackingDescription, string localeCode, string buildPath, PBXProject project, string targetGuid)
        {
            const string resourcesDirectoryName = "CrazySummerLabLocalizationResources";
            var resourcesDirectoryPath = Path.Combine(buildPath, resourcesDirectoryName);
            var localeSpecificDirectoryName = localeCode + ".lproj";
            var localeSpecificDirectoryPath = Path.Combine(resourcesDirectoryPath, localeSpecificDirectoryName);
            var infoPlistStringsFilePath = Path.Combine(localeSpecificDirectoryPath, "InfoPlist.strings");

            if (!AppTrackingInspector.Settings.useLocalizationValues)
            {
                if (!File.Exists(infoPlistStringsFilePath)) return;
                File.Delete(infoPlistStringsFilePath);
                return;
            }

            // Create intermediate directories as needed.
            if (!Directory.Exists(resourcesDirectoryPath))
            {
                Directory.CreateDirectory(resourcesDirectoryPath);
            }
            if (!Directory.Exists(localeSpecificDirectoryPath))
            {
                Directory.CreateDirectory(localeSpecificDirectoryPath);
            }

            var localizedDescriptionLine = "\"NSUserTrackingUsageDescription\" = \"" + localizedUserTrackingDescription + "\";\n";
            // File already exists, update it in case the value changed between builds.
            if (File.Exists(infoPlistStringsFilePath))
            {
                var output = new List<string>();
                var lines = File.ReadAllLines(infoPlistStringsFilePath);
                var keyUpdated = false;
                foreach (var line in lines)
                {
                    if (line.Contains("NSUserTrackingUsageDescription"))
                    {
                        output.Add(localizedDescriptionLine);
                        keyUpdated = true;
                    }
                    else
                    {
                        output.Add(line);
                    }
                }
                if (!keyUpdated)
                {
                    output.Add(localizedDescriptionLine);
                }
                File.WriteAllText(infoPlistStringsFilePath, string.Join("\n", output.ToArray()) + "\n");
            }
            // File doesn't exist, create one.
            else
            {
                File.WriteAllText(infoPlistStringsFilePath, "/* Localized versions of Info.plist keys */\n" + localizedDescriptionLine);
            }
            var guid = project.AddFolderReference(localeSpecificDirectoryPath, Path.Combine(resourcesDirectoryName, localeSpecificDirectoryName), PBXSourceTree.Source);
            project.AddFileToBuild(targetGuid, guid);
        }

        private static string Get2LetterISOCodeFromSystemLanguage(SystemLanguage lang)
        {
            string res = "EN";
            switch (lang)
            {
                case SystemLanguage.Afrikaans: res = "AF"; break;
                case SystemLanguage.Arabic: res = "AR"; break;
                case SystemLanguage.Basque: res = "EU"; break;
                case SystemLanguage.Belarusian: res = "BY"; break;
                case SystemLanguage.Bulgarian: res = "BG"; break;
                case SystemLanguage.Catalan: res = "CA"; break;
                case SystemLanguage.Chinese: res = "ZH"; break;
                case SystemLanguage.Czech: res = "CS"; break;
                case SystemLanguage.Danish: res = "DA"; break;
                case SystemLanguage.Dutch: res = "NL"; break;
                case SystemLanguage.English: res = "EN"; break;
                case SystemLanguage.Estonian: res = "ET"; break;
                case SystemLanguage.Faroese: res = "FO"; break;
                case SystemLanguage.Finnish: res = "FI"; break;
                case SystemLanguage.French: res = "FR"; break;
                case SystemLanguage.German: res = "DE"; break;
                case SystemLanguage.Greek: res = "EL"; break;
                case SystemLanguage.Hebrew: res = "IW"; break;
                case SystemLanguage.Hungarian: res = "HU"; break;
                case SystemLanguage.Icelandic: res = "IS"; break;
                case SystemLanguage.Indonesian: res = "IN"; break;
                case SystemLanguage.Italian: res = "IT"; break;
                case SystemLanguage.Japanese: res = "JA"; break;
                case SystemLanguage.Korean: res = "KO"; break;
                case SystemLanguage.Latvian: res = "LV"; break;
                case SystemLanguage.Lithuanian: res = "LT"; break;
                case SystemLanguage.Norwegian: res = "NO"; break;
                case SystemLanguage.Polish: res = "PL"; break;
                case SystemLanguage.Portuguese: res = "PT"; break;
                case SystemLanguage.Romanian: res = "RO"; break;
                case SystemLanguage.Russian: res = "RU"; break;
                case SystemLanguage.SerboCroatian: res = "SH"; break;
                case SystemLanguage.Slovak: res = "SK"; break;
                case SystemLanguage.Slovenian: res = "SL"; break;
                case SystemLanguage.Spanish: res = "ES"; break;
                case SystemLanguage.Swedish: res = "SV"; break;
                case SystemLanguage.Thai: res = "TH"; break;
                case SystemLanguage.Turkish: res = "TR"; break;
                case SystemLanguage.Ukrainian: res = "UK"; break;
                case SystemLanguage.Unknown: res = "EN"; break;
                case SystemLanguage.Vietnamese: res = "VI"; break;
            }
            return res.ToLower();
        }
    }
}
#endif