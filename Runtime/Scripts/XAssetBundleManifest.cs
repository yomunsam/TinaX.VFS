using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX.VFSKitInternal;

namespace TinaX.VFSKit
{
    public class XAssetBundleManifest
    {
        private readonly Dictionary<string, AssetBundleInfo> mDict_AssetBundleInfos;

        private List<XAssetBundleManifest> mCollaborations = new List<XAssetBundleManifest>();

        public XAssetBundleManifest(BundleManifest manifest)
        {
            mDict_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();
            foreach(var item in manifest.assetBundleInfos)
            {
                if (!mDict_AssetBundleInfos.ContainsKey(item.name))
                {
                    mDict_AssetBundleInfos.Add(item.name, item);
                }
                else
                {
                    mDict_AssetBundleInfos[item.name] = item;
                }
            }
        }
    
        /// <summary>
        /// 添加协同Manifest： VFS的Manifest之间可能有依赖关系（比如Extension Group中的资源可能会依赖Main Package中的资源，那么Extension Group的Manifest在查询依赖的时候，可能会查到Main Package的Manifest上来
        /// </summary>
        /// <param name="manifest"></param>
        public void AddCollaborationManifest(XAssetBundleManifest manifest)
        {
            if (!mCollaborations.Contains(manifest))
            {
                mCollaborations.Add(manifest);
            }
        }

        public void RemoveCollaborationManifest(int hashCode)
        {
            for(int i = mCollaborations.Count -1; i >= 0; i--)
            {
                if(mCollaborations[i].GetHashCode() == hashCode)
                {
                    mCollaborations.RemoveAt(i);
                }
            }
        }

        public void RemoveCollaborationManifest(ref XAssetBundleManifest manifest)
        {
            if (manifest != null)
                this.RemoveCollaborationManifest(manifest.GetHashCode());
        }

        public string[] GetAllAssetBundles()
        {
            List<string> names = new List<string>();
            foreach(var item in mDict_AssetBundleInfos) { names.Add(item.Key); }
            return names.ToArray();
        }

        public string[] GetDirectDependencies(string assetBundleName)
        {
            if(mDict_AssetBundleInfos.TryGetValue(assetBundleName,out var infos))
            {
                return infos.dependencies;
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public bool TryGetDirectDependencies(string assetBundleName, out string[] dependencies)
        {
            if (mDict_AssetBundleInfos.TryGetValue(assetBundleName, out var infos))
            {
                dependencies = infos.dependencies;
                return true;
            }
            else
            {
                dependencies = default;
                return false;
            }
        }

        public string[] GetAllDependencies(string assetBundleName)
        {
            List<string> Dependencies = new List<string>();
            if(this.TryGetAllDependenciesRecursion(assetBundleName,ref Dependencies))
            {
                return Dependencies.ToArray();
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public bool TryGetAllDependencies(string assetBundleName, out string[] dependencies)
        {
            List<string> list_dependencies = new List<string>();
            if(this.TryGetAllDependenciesRecursion(assetBundleName,ref list_dependencies))
            {
                dependencies = list_dependencies.ToArray();
                return true;
            }
            else
            {
                dependencies = default;
                return false;
            }
        }

        /// <summary>
        /// 递归获取所有依赖,
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="dependencies"></param>
        private bool TryGetAllDependenciesRecursion(string assetBundleName , ref List<string> dependencies)
        {
            if (this.TryGetDirectDependencies(assetBundleName, out var _dependencies))
            {
                //找到了
                if (_dependencies != null && _dependencies.Length > 0)
                {
                    //递归找到这些依赖的依赖
                    foreach (var ab in _dependencies)
                    {
                        if (!dependencies.Contains(ab))
                        {
                            this.GetAllDependenciesRecursion_WithCollaborations(ab, ref dependencies);
                            dependencies.Add(ab);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        private void GetAllDependenciesRecursion_WithCollaborations(string assetBundleName, ref List<string> dependencies)
        {
            if (this.TryGetDirectDependencies(assetBundleName, out var _dependencies))
            {
                //找到了
                if (_dependencies != null && _dependencies.Length > 0)
                {
                    //递归找到这些依赖的依赖
                    foreach (var ab in _dependencies)
                    {
                        if (!dependencies.Contains(ab))
                        {
                            this.GetAllDependenciesRecursion_WithCollaborations(ab, ref dependencies);
                            dependencies.Add(ab);
                        }
                    }
                }
            }
            else
            {
                //本地没找到这个assetbundle文件, 尝试在协同Manifest里面找一圈
                foreach (var manifest in mCollaborations)
                {
                    if (manifest.TryGetAllDependenciesRecursion(assetBundleName, ref dependencies))
                    {
                        //找到了
                        break;
                    }
                }
            }
        }


    }
}
