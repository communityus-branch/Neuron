using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Static_Interface.API.Utils;
using Object = UnityEngine.Object;

namespace Static_Interface.API.AssetsFramework
{
    public static class AssetManager
    {
        //Todo: on asset bundle loaded
        private static readonly List<AssetBundle> InternalBundles = new List<AssetBundle>();
        public static ReadOnlyCollection<AssetBundle> Bundles => InternalBundles.AsReadOnly();

        public static AssetBundle GetAssetBundle(string bundleName)
        {
            return InternalBundles.FirstOrDefault(a => a.Name == bundleName);
        }

        internal static AssetBundle LoadAssetBundle(string bundleName, string file)
        {
            AssetBundle assetBundle = InternalBundles.FirstOrDefault(a => a.FilePath == file);
            if (assetBundle != null)
            {
                return assetBundle;
            }
            assetBundle = GetAssetBundle(bundleName);
            if (assetBundle != null)
                throw new Exception("An asset with the same name but different location has been already loaded!");

            LogUtils.Debug("Loading asset bundle: " + bundleName + "(" + file + ")");
            assetBundle = new AssetBundle(bundleName, file);
            assetBundle.LoadAllAssets<Object>();
            InternalBundles.Add(assetBundle);
            return assetBundle;
        }

        internal static void ClearAssetBundles()
        {
            InternalBundles.Clear();
        }
    }
}