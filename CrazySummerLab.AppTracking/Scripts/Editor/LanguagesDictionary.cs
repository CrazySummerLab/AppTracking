#if UNITY_IOS
using System;

namespace CrazySummerLab
{
    [Serializable]
    public class LanguagesDictionary : SerializableDictionary<int, string> { }
}
#endif