using TinaXEditor.VFS.Groups;
using TinaXEditor.VFS.Querier;
using UnityEngine;

namespace TinaXEditor.VFS.AssetBuilder.Builder
{
    /// <summary>
    /// VFS 资产组 资产构建器
    /// </summary>
    public class GroupAssetBuilder
    {
        private readonly IEditorAssetQuerier m_AssetQuerier;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetQuerier">资产查询器</param>
        public GroupAssetBuilder(IEditorAssetQuerier assetQuerier, 
            EditorVFSGroup group)
        {
            this.m_AssetQuerier = assetQuerier;
        }


        /*
         * 一个完整的资产构建流程：
         * 1. 整理所有要打包的资产
         *      1. 整理要打包的范围内（比如一个组、一个包、或者全局）所有的AssetBundle信息
         * 2. 【可选】显式标记
         * 3. 打包
         * 
         */

    }
}
