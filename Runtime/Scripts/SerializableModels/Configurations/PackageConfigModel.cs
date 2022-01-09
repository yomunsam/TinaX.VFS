using System;
using System.Collections.Generic;

namespace TinaX.VFS.SerializableModels.Configurations
{
    /// <summary>
    /// 可序列化模型 - 资产包配置
    /// </summary>
    [Serializable]
    public class PackageConfigModel
    {
        public List<GroupConfigModel> Groups;
    }
}
