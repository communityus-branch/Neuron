using System.IO;
using UnityEngine;

namespace Static_Interface.API.Utils
{
    public static class IOUtil
    {
        public static string GetDataFolder()
        {
#if UNITY_EDITOR
            return Application.persistentDataPath;
#elif UNITY_STANDALONE
            return Directory.GetParent(Application.dataPath).FullName;
#else
            return Application.persistentDataPath;
#endif
        }

        public static string GetDataSubFolder(string folderName)
        {
            return Path.Combine(GetDataFolder(), folderName);
        }

        public static string GetExtensionsDir()
        {
            //Todo: change to steam workshop folder
            return GetDataSubFolder("Extensions");
        }
    }
}