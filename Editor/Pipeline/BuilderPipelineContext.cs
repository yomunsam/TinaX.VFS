using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.Pipeline
{
    public class BuilderPipelineContext
    {
        private IBuildHandler _handler;
        public virtual IBuildHandler Handler { get { return _handler; } }
        internal BuilderPipelineContext Next { get; set; }
        internal BuilderPipelineContext Prev { get; set; }

        public BuilderPipelineContext(IBuildHandler handler)
        {
            _handler = handler;
            Next = null;
            Prev = null;
        }


    }
}
