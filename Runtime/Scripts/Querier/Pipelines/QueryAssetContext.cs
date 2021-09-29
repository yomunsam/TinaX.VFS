using TinaX.Container;

namespace TinaX.VFS.Querier.Pipelines
{
    public class QueryAssetContext
    {
        /// <summary>
        /// 是否终断Pipeline的标记
        /// </summary>
        public bool BreakPipeline { get; set; } = false;

        /// <summary>
        /// 终断Pipeline流程
        /// </summary>
        public void Break() => BreakPipeline = true;

        public IServiceContainer Services { get; set; }

        //public Dictionary<string, object> Items = new Dictionary<string, object>(); //先注释掉，等啥时候给它搞个对象池的再放出来，当下的目标是尽可能减少gc
    }
}
