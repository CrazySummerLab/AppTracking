#if UNITY_IOS
using System.Collections.Generic;
using UnityEngine;

namespace CrazySummerLab
{
    public class AppTrackingSettings : ScriptableObject
    {
        public bool useLocalizationValues;
        public LanguagesDictionary localizedPopupMessageDictionary;
        public List<string> SkAdNetworkIds;
    }
}
#endif