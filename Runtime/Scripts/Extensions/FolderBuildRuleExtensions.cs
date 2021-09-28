using TinaX.VFS.BuildRules;
using TinaX.VFS.Const;

namespace TinaX.VFS.Extensions
{
    public static class FolderBuildRuleExtensions
    {
        /// <summary>
        /// 是否是一个仅编辑器下可用的规则
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static bool IsEditorOnly(this FolderBuildRule rule)
        {
            if (rule.BuildTags == null)
                return false;
            return rule.BuildTags.Contains(VFSConsts.BuildTag_EditorOnly);
        }
    }
}
