using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaXEditor.VFSKit;
using TinaX.VFSKit;
using System.IO;

namespace TinaXEditor.VFSKit.Pipeline
{
    public interface IBuildHandler
    {
        /// <summary>
        /// Called before builder sets the AssetBundle name and AssetBundleVarient of an asset | 在Builder设置资产的AssetBundle Name 和 AssetBundleVarient之前被调用
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns>If return false , Pipeline will break | 如果返回False， Pipline将中断</returns>
        bool BeforeSetAssetBundleSign(ref string assetbundleName, ref AssetsStatusQueryResult assetQueryResult);

        /// <summary>
        /// Before the AssetBundle files is saved by groups | 在AssetBundle文件被以组为单位处理保存时
        /// </summary>
        /// <param name="group">Group by which the current file is processed | 处理当前文件依据的组</param>
        /// <param name="assetBundleFileName">The name of the AssetBundle file currently being processed | 当前处理的AssetBundle文件名。</param>
        /// <param name="assetName">The assetbundle file currently processed is queried based on which asset | 当前处理的AssetBundle文件是依据哪个asset资产查询出来的</param>
        /// <param name="fileStream">Current AssetBundle fileStream | 当前AssetBundle文件的FileStream</param>
        /// <returns></returns>
        bool BeforeAssetBundleFileSavedByGroup(ref VFSGroup group, string assetBundleFileName, string assetName, ref FileStream fileStream);
    }
}
