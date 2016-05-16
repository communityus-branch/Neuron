using Static_Interface.API.EventFramework;

namespace Static_Interface.API.AssetsFramework
{
    public class AssetBundleLoadedEvent : Event
    {
        public AssetBundle AssetBundle { get; }
        public AssetBundleLoadedEvent(AssetBundle assetBundle) : base(false)
        {
            AssetBundle = assetBundle;
        }
    }
}