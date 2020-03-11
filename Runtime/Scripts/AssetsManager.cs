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

        private List<VFSAsset> mAssets_Sync = new List<VFSAsset>();
        private Dictionary<string, VFSAsset> mDict_Assets_KeyName_Sync = new Dictionary<string, VFSAsset>();
        private Dictionary<int, VFSAsset> mDict_Assets_KeyHashCode_Sync = new Dictionary<int, VFSAsset>();

#if UNITY_EDITOR
        private List<EditorAsset> mEditorAssets = new List<EditorAsset>();
        private Dictionary<string, EditorAsset> mDict_EditorAssets_KeyName = new Dictionary<string, EditorAsset>();
        private Dictionary<int, EditorAsset> mDict_EditorAssets_KeyHashCode = new Dictionary<int, EditorAsset>();
#endif

        public void Register(VFSAsset asset)
        {
            lock (this)
            {
                if (!mAssets.Contains(asset))
                    mAssets.Add(asset);

                if (mDict_Assets_KeyName.ContainsKey(asset.AssetPathLower))
                    mDict_Assets_KeyName[asset.AssetPathLower] = asset;
                else
                    mDict_Assets_KeyName.Add(asset.AssetPathLower, asset);
            }
        }

        public void RegisterSync(VFSAsset asset)
        {
            if (!mAssets_Sync.Contains(asset))
                mAssets_Sync.Add(asset);

            if (mDict_Assets_KeyName_Sync.ContainsKey(asset.AssetPathLower))
                mDict_Assets_KeyName_Sync[asset.AssetPathLower] = asset;
            else
                mDict_Assets_KeyName_Sync.Add(asset.AssetPathLower, asset);
        }



        /// <summary>
        /// 资源在加载完成之后，调用一次这个
        /// </summary>
        /// <param name="asset"></param>
        public void RegisterHashCode(VFSAsset asset)
        {
            if (asset == null || asset.Asset == null) return;

            int hashCode = asset.Asset.GetHashCode();
            if (!mDict_Assets_KeyHashCode.ContainsKey(hashCode))
            {
                mDict_Assets_KeyHashCode.Add(hashCode, asset);
            }
        }

        public void RegisterHashCodeSync(VFSAsset asset)
        {
            if (asset == null || asset.Asset == null) return;

            int hashCode = asset.Asset.GetHashCode();
            if (!mDict_Assets_KeyHashCode_Sync.ContainsKey(hashCode))
            {
                mDict_Assets_KeyHashCode_Sync.Add(hashCode, asset);
            }
        }

#if UNITY_EDITOR
        public void Register(EditorAsset asset)
        {
            if (!mEditorAssets.Contains(asset))
                mEditorAssets.Add(asset);
            if (mDict_EditorAssets_KeyName.ContainsKey(asset.AssetPathLower))
                mDict_EditorAssets_KeyName[asset.AssetPathLower] = asset;
            else
                mDict_EditorAssets_KeyName.Add(asset.AssetPathLower, asset);


            if (mDict_EditorAssets_KeyHashCode.ContainsKey(asset.AssetHashCode))
                mDict_EditorAssets_KeyHashCode[asset.AssetHashCode] = asset;
            else
                mDict_EditorAssets_KeyHashCode.Add(asset.AssetHashCode, asset);
        }
#endif

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
                        mDict_Assets_KeyName.Remove(asset.AssetPathLower);
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
                        mDict_Assets_KeyName.Remove(asset.AssetPathLower);
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

        public bool TryGetAssetSync(string path, out VFSAsset asset)
        {
            lock (this)
            {
                if (mDict_Assets_KeyName_Sync.TryGetValue(path, out asset))
                {
                    if (asset.LoadState == AssetLoadState.Unloaded)
                    {
                        mAssets_Sync.Remove(asset);
                        mDict_Assets_KeyHashCode_Sync.Remove(asset.GetHashCode());
                        mDict_Assets_KeyName_Sync.Remove(asset.AssetPathLower);
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

        public bool TryGetAssetSync(int hashCode, out VFSAsset asset)
        {
            lock (this)
            {
                if (mDict_Assets_KeyHashCode_Sync.TryGetValue(hashCode, out asset))
                {
                    if (asset.LoadState == AssetLoadState.Unloaded)
                    {
                        mAssets_Sync.Remove(asset);
                        mDict_Assets_KeyHashCode_Sync.Remove(hashCode);
                        mDict_Assets_KeyName_Sync.Remove(asset.AssetPathLower);
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

#if UNITY_EDITOR
        public bool TryGetEditorAsset(string path , out EditorAsset asset)
        {
            if (mDict_EditorAssets_KeyName.TryGetValue(path, out asset))
            {
                if (asset.LoadState == AssetLoadState.Unloaded)
                {
                    mEditorAssets.Remove(asset);
                    mDict_EditorAssets_KeyHashCode.Remove(asset.AssetHashCode);
                    mDict_EditorAssets_KeyName.Remove(asset.AssetPathLower);
                    asset = null;
                    return false;
                }
                else
                    return true;
            }
            else
                return false;
        }

        public bool TryGetEditorAsset(int hashCode, out EditorAsset asset)
        {
            if (mDict_EditorAssets_KeyHashCode.TryGetValue(hashCode, out asset))
            {
                if (asset.LoadState == AssetLoadState.Unloaded)
                {
                    mEditorAssets.Remove(asset);
                    mDict_EditorAssets_KeyHashCode.Remove(asset.AssetHashCode);
                    mDict_EditorAssets_KeyName.Remove(asset.AssetPathLower);
                    asset = null;
                    return false;
                }
                else
                    return true;
            }
            else
                return false;
        }

#endif

        public void Refresh()
        {
            lock (this)
            {
                for (var i = mAssets.Count - 1; i >= 0; i--)
                {
                    if (mAssets[i].LoadState == AssetLoadState.Unloaded)
                    {
                        mDict_Assets_KeyName.Remove(mAssets[i].AssetPathLower);
                        if (mDict_Assets_KeyHashCode.ContainsKey(mAssets[i].AssetHashCode))
                        {
                            mDict_Assets_KeyHashCode.Remove(mAssets[i].AssetHashCode);
                        }
                        mAssets.RemoveAt(i);
                    }
                }
            }

            lock(this)
            {
                for (var i = mAssets_Sync.Count - 1; i >= 0; i--)
                {
                    if (mAssets_Sync[i].LoadState == AssetLoadState.Unloaded)
                    {
                        mDict_Assets_KeyName_Sync.Remove(mAssets_Sync[i].AssetPathLower);
                        if (mDict_Assets_KeyHashCode_Sync.ContainsKey(mAssets_Sync[i].AssetHashCode))
                            mDict_Assets_KeyHashCode_Sync.Remove(mAssets_Sync[i].AssetHashCode);

                        mAssets_Sync.RemoveAt(i);
                    }
                }
            }

#if UNITY_EDITOR
            for(var i = mEditorAssets.Count -1; i >=0; i++)
            {
                if(mEditorAssets[i].LoadState == AssetLoadState.Unloaded)
                {
                    mDict_EditorAssets_KeyHashCode.Remove(mEditorAssets[i].AssetHashCode);
                    mDict_EditorAssets_KeyName.Remove(mEditorAssets[i].AssetPathLower);
                    mEditorAssets.RemoveAt(i);
                }
            }
#endif
        }




    }
}
