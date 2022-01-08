using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinaX.Core.Helper.Platform;
using TinaX.VFS.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable
namespace TinaXEditor.VFS.AssetBuilder.Pipelines.Handlers
{
    /// <summary>
    /// 保存数据文件
    /// </summary>
    public class SaveDataFileAsyncHandler : IBuildAssetsAsyncHandler
    {
        //------------固定字段----------------------------------------------------------------------------------------------------------------------
        private readonly string m_ProjectVirtualSpacePath = VFSUtils.GetProjectVirtualSpacePath();



        public string HandlerName => HandlerNameConsts.SaveDataFiles;

        public Task BuildAssetAsync(BuildAssetsContext context, CancellationToken cancellationToken)
        {
            if (context.HansLog)
                Debug.Log("保存数据文件.");
            else
                Debug.Log("Save data files.");

            var platformName = PlatformHelper.GetName(context.BuildArgs.BuildPlatform);

            //------保存配置文件--------------------------------------
            //主包配置和全局配置（VFSConfigTpl）
            var mainPackDataFolderInProjectVirtualSpace = VFSUtils.GetMainPackageDataFolder(m_ProjectVirtualSpacePath, platformName);
            if (!Directory.Exists(mainPackDataFolderInProjectVirtualSpace))
                Directory.CreateDirectory(mainPackDataFolderInProjectVirtualSpace);
            string vfsConfigTplFilePath = VFSUtils.GetVFSConfigTplFilePath(mainPackDataFolderInProjectVirtualSpace);
            string vfsConfigTplJson = JsonUtility.ToJson(context.VFSConfigTpl!);
            File.WriteAllText(vfsConfigTplFilePath, vfsConfigTplJson, Encoding.UTF8);

            //Todo: 扩展包的配置文件


            //------AssetBundle索引和依赖--------------------------------------
            //主包

            return Task.CompletedTask;
        }
    }
}
