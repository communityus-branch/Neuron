using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.Utils;
using Object = UnityEngine.Object;

namespace Static_Interface.API.AssetsFramework
{
    public static class AssetManager
    {
        private static readonly List<Asset> Assets = new List<Asset>();

        public static Asset GetAsset(string assetName)
        {
            return Assets.FirstOrDefault(a => a.Name == assetName);
        }

        public static Asset LoadAsset(string assetName, string file, bool forceNew = false)
        {
            Asset asset = Assets.FirstOrDefault(a => a.FilePath == file);
            if (asset != null && !forceNew)
            {
                return asset;
            }
            asset = GetAsset(assetName);
            if (asset != null) throw new Exception("An asset with the same name but different location has been already loaded!");
            LogUtils.Debug("Loading asset: " + assetName + "(" + file + ")");
            asset = new Asset(assetName, file);
            asset.LoadAllAssets<Object>();
            Assets.Add(asset);
            return asset;
        }

        internal static void ClearAssets()
        {
            Assets.Clear();
        }
    }
}