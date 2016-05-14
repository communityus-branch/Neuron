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
        private static readonly List<AssetBundle> InternalBundles = new List<AssetBundle>();
        public static ReadOnlyCollection<AssetBundle> Bundles => InternalBundles.AsReadOnly();
         
        public static AssetBundle GetAssetBundle(string assetName)
        {
            return InternalBundles.FirstOrDefault(a => a.Name == assetName);
        }

        public static AssetBundle LoadAssetBundle(string bundle, string file, bool forceNew = false)
        {
            AssetBundle assetBundle = InternalBundles.FirstOrDefault(a => a.FilePath == file);
            if (assetBundle != null && !forceNew)
            {
                return assetBundle;
            }
            assetBundle = GetAssetBundle(bundle);
            if (assetBundle != null) throw new Exception("An asset with the same name but different location has been already loaded!");
            LogUtils.Debug("Loading asset: " + bundle + "(" + file + ")");
            assetBundle = new AssetBundle(bundle, file);
            assetBundle.LoadAllAssets<Object>();
            InternalBundles.Add(assetBundle);
            return assetBundle;
        }

        internal static void ClearAssetBundles()
        {
            InternalBundles.Clear();
        }
}