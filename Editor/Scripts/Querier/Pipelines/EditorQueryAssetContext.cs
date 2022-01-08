namespace TinaXEditor.VFS.Querier.Pipelines
{
    public class EditorQueryAssetContext
    {
        /// <summary>
        /// 是否终断Pipeline的标记
        /// </summary>
        public bool BreakPipeline { get; set; } = false;

        /// <summary>
        /// 终断Pipeline流程
        /// </summary>
        public void Break() => BreakPipeline = true;


        /// <summary>
        /// 在主包中查询
        /// </summary>
        public bool QueryMainPack { get; set; } = true;

        /// <summary>
        /// 在扩展包中查询
        /// </summary>
        public bool QueryExpansionPack { get; set; } = true;

    }
}
