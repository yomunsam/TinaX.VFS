using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit
{
    public class PackerOptions
    {
        internal List<AssetsManagerRule> AssetManagerRules = new List<AssetsManagerRule>();

        public PackerOptions AddAssetManagerRule(AssetsManagerRule rule)
        {
            AssetManagerRules.Add(rule);
            return this;
        }

        public PackerOptions AddAssetManagerRules(AssetsManagerRule[] rules)
        {
            AssetManagerRules.AddRange(rules);
            return this;
        }

    }
}
