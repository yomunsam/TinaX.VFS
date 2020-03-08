using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKitInternal
{
    public class AssetsManager
    {
        private List<VFSAsset> mAssets = new List<VFSAsset>();
        private Dictionary<string, VFSAsset> mDict_Assets_KeyName = new Dictionary<string, VFSAsset>();
        private Dictionary<int, VFSAsset> mDict_Assets_KeyHashCode = new Dictionary<int, VFSAsset>();


        public void Register(VFSAsset asset)
        {
            if (!mAssets.Contains(asset))
                mAssets.Add(asset);

            if (mDict_Assets_KeyName.ContainsKey(asset.AssetPath))
                mDict_Assets_KeyName[asset.AssetPath] = asset;
            else
                mDict_Assets_KeyName.Add(asset.AssetPath, asset);

            int hashCode = asset.GetHashCode();
            if (mDict_Assets_KeyHashCode.ContainsKey(hashCode))
                mDict_Assets_KeyHashCode[hashCode] = asset;
            else
                mDict_Assets_KeyHashCode.Add(hashCode, asset);
        }


        public bool TryGetAsset(string path,out VFSAsset asset)
        {
            lock (this)
            {
                if (mDict_Assets_KeyName.TryGetValue(path, out asset))
                {
                    if (asset.LoadState == AssetLoadState.Unloaded)
                    {
                        mAssets.Remove(asset);
                        mDict_Assets_KeyHashCode.Remove(asset.GetHashCode());
                        mDict_Assets_KeyName.Remove(asset.AssetPath);
                        asset = null;
                        return false;
                    }
                    else
                        return true;
            }
                else
                    return false;
            }
        }

        public bool TryGetAsset(int hashCode, out VFSAsset asset)
        {
            lock (this)
            {
                if (mDict_Assets_KeyHashCode.TryGetValue(hashCode, out asset))
                {
                    if (asset.LoadState == AssetLoadState.Unloaded)
                    {
                        mAssets.Remove(asset);
                        mDict_Assets_KeyHashCode.Remove(hashCode);
                        mDict_Assets_KeyName.Remove(asset.AssetPath);
                        asset = null;
                        return false;
                    }
                    else
                        return true;
                }
                else
                    return false;
            }
        }


        public void Refresh()
        {
            for(var i = mAssets.Count -1; i>=0; i--)
            {
                if(mAssets[i].LoadState == AssetLoadState.Unloaded)
                {
                    mDict_Assets_KeyName.Remove(mAssets[i].AssetPath);
                    mDict_Assets_KeyHashCode.Remove(mAssets[i].GetHashCode());
                    mAssets.RemoveAt(i);
                }
            }
        }


    }
}
