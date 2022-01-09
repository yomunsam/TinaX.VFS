using TinaX.VFS.Assets;
using TinaX.VFS.Querier;

#nullable enable
namespace TinaX.VFS.Pipelines.LoadAsset
{
    public class LoadAssetPayload //TinaX中有的管线分了Context和Payload, 用意是后期计划把Context部分弄成对象池减少GC
    {
        public LoadAssetPayload(string loadPath, string variant)
        {
            this.LoadPath = loadPath;
            this.Variant = variant;
        }

        public string LoadPath { get; set; }
        public string Variant { get; set; }

        public AssetQueryResult? QueryResult { get; set; }



        public VFSAsset? LoadedAsset { get; set; }
    }
}
