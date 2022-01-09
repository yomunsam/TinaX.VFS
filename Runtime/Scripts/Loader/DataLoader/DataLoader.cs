using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using TinaX.Core.Helper.String;
using TinaX.Exceptions;
using TinaX.Services.ConfigAssets;
using TinaX.VFS.ConfigAssets;
using TinaX.VFS.Consts;
using TinaX.VFS.SerializableModels;
using TinaX.VFS.Utils;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace TinaX.VFS.Loader.DataLoader
{
    /// <summary>
    /// 数据加载器
    /// </summary>
    public class DataLoader
    {
        private readonly string m_PlatformName;
        private readonly string m_VSpaceInStreamingAssetsPath;
        private readonly string m_VSpaceInLocalStoragePath;
        private readonly bool m_LoadAssetsViaEditor;

        public DataLoader(string platformName,
            string vspaceInStreamingAssetsPath,
            string vspaceInLocalStoragePath,
            bool loadAssetsViaEditor)
        {
            this.m_PlatformName = platformName;
            this.m_VSpaceInStreamingAssetsPath = vspaceInStreamingAssetsPath;
            this.m_VSpaceInLocalStoragePath = vspaceInLocalStoragePath;
            this.m_LoadAssetsViaEditor = loadAssetsViaEditor;
        }

        public async UniTask<VFSConfigModel> LoadVFSConfigAsync()
        {
            if (m_LoadAssetsViaEditor)
            {
                return LoadVFSConfigFromAssetInEditor();
            }
            //先尝试从localStorage加载
            var vspaceDataFolderInLocalStorage = VFSUtils.GetMainPackageDataFolder(m_VSpaceInLocalStoragePath, m_PlatformName); //这个文件存在于MainPackage
            var configFilePathInLocalStorage = VFSUtils.GetVFSConfigModelFilePath(vspaceDataFolderInLocalStorage);
            if(File.Exists(configFilePathInLocalStorage)) //这个目录应该是可读写的，所以用System.IO判断
            {
                try
                {
                    var json = File.ReadAllText(configFilePathInLocalStorage, Encoding.UTF8);
                    var config = JsonUtility.FromJson<VFSConfigModel>(json);
                    return config;
                }
                catch(Exception ex)
                {
                    Debug.LogErrorFormat("Failed to load VFS config file from localStorage path {0} , exception message: {1}", configFilePathInLocalStorage, ex.Message);
                }
            }

            //从StreamingAssets中加载
            var vspaceDataFolderInStreamingAssets = VFSUtils.GetMainPackageDataFolder(m_VSpaceInStreamingAssetsPath, m_PlatformName); //这个文件存在于MainPackage
            var configFilePathInStreamingAssets = VFSUtils.GetVFSConfigModelFilePath(vspaceDataFolderInStreamingAssets);
            var uri = new System.Uri(configFilePathInStreamingAssets);
            try
            {
                using(var req = UnityWebRequest.Get(uri))
                {
                    var result = await req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.ProtocolError)
                    {
                        if (req.responseCode == 404)
                        {
                            throw new NotFoundException("Failed to load VFS config file from localStorage path {0}, file not found.", uri.ToString());
                        }
                    }
                    var json = StringHelper.RemoveUTF8BOM(req.downloadHandler.data);
                    return JsonUtility.FromJson<VFSConfigModel>(json);
                }
            }
            catch(UnityWebRequestException ex)
            {
                if(ex.Result == UnityWebRequest.Result.ProtocolError)
                {
                    if (ex.UnityWebRequest.responseCode == 404)
                    {
                        throw new NotFoundException("Failed to load VFS config file from localStorage path {0}, file not found.", uri.ToString());
                    }
                }
                throw;
            }

        }


        /// <summary>
        /// 编辑器下直接加载资产
        /// </summary>
        /// <returns></returns>
        private VFSConfigModel LoadVFSConfigFromAssetInEditor()
        {
#if UNITY_EDITOR
            string loadPath = GetVFSConfigAssetEditorPath();
            var configAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VFSConfigAsset>(loadPath);
            return VFSConfigUtils.MapToVFSConfigModel(configAsset);
#else
            throw new TinaX.Exceptions.XException("Configuration files should not be loaded using the unity editor API");
#endif
        }

        private string GetVFSConfigAssetEditorPath()
        {
#if TINAX_CONFIG_NO_RESOURCES
            return $"{ConfigAssetService.ConfigAssetsDirectoryForAssetServiceNoResources}/{VFSConsts.DefaultConfigAssetName}.asset";
#else
            return $"{ConfigAssetService.ConfigAssetsDirectoryForAssetServiceInResources}/{VFSConsts.DefaultConfigAssetName}.asset";
#endif
        }
    }
}
