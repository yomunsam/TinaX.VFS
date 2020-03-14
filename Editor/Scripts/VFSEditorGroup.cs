using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.VFSKit;
using UnityEngine;
using System.IO;
using TinaX.IO;
using TinaX.VFSKitInternal;
using TinaX.VFSKitInternal.Utils;

namespace TinaXEditor.VFSKit
{
    public class VFSEditorGroup : VFSGroup
    {
        public VFSEditorGroup(VFSGroupOption option) : base(option) { }
     
        /// <summary>
        /// 制作 Manifest
        /// </summary>
        /// <param name="packages_root_path"></param>
        /// <param name="unity_manifest"></param>
        public void MakeVFSManifest(string packages_root_path,List<string> assetbundleNames, ref AssetBundleManifest unity_manifest)
        {
            List<AssetBundleInfo> infos = new List<AssetBundleInfo>();
            foreach(var assetbundle in assetbundleNames)
            {
                var bundle = new AssetBundleInfo();
                bundle.name = assetbundle;
                bundle.dependencies = unity_manifest.GetDirectDependencies(assetbundle);

                infos.Add(bundle);
            }

            var bundleManifest = new BundleManifest();
            bundleManifest.assetBundleInfos = infos.ToArray();

            string save_path = base.GetManifestFilePath(packages_root_path);
            XConfig.SaveJson(bundleManifest, save_path, AssetLoadType.SystemIO);
        }

        /// <summary>
        /// 制作 AssetBundles Hash File , 
        /// </summary>
        /// <param name="package_root_path">packages根目录</param>
        /// <param name="folder_path">当前直接能索引到assetbundle name的目录</param>
        /// <param name="assetbundleNames"></param>
        public void MakeAssetBundleFilesHash(string package_root_path,string folder_path, List<string> assetbundleNames)
        {
            List<FilesHashBook.FileHash> Infos = new List<FilesHashBook.FileHash>();

            foreach (var ab in assetbundleNames)
            {
                var full_path = Path.Combine(folder_path, ab);
                if (File.Exists(full_path))
                {
                    var hashInfo = new FilesHashBook.FileHash();
                    hashInfo.p = ab;
                    hashInfo.h = XFile.GetMD5(full_path, true);

                    Infos.Add(hashInfo);
                }
            }
            var hashbook = new FilesHashBook();
            hashbook.Files = Infos.ToArray();

            string save_path = base.GetAssetBundleHashsFilePath(package_root_path);
            XConfig.SaveJson(hashbook, save_path, AssetLoadType.SystemIO);
        }

        public void SaveGroupOptionFile(string package_root_path)
        {
            string target_path = VFSUtil.GetExtensionPackages_GroupOptions_FilePath(package_root_path, this.GroupName);
            XFile.DeleteIfExists(target_path);
            XDirectory.CreateIfNotExists(Path.GetDirectoryName(target_path));

            string json = JsonUtility.ToJson(base.mOption);
            File.WriteAllText(target_path, json);
        }

        public void SetVirtualDiskFileHash(FilesHashBook hashbook)
        {
            base.FilesHash_VirtualDisk = hashbook;
        }

    }
}
