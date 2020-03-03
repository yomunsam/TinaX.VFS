using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaXEditor.VFSKit.Pipeline
{
    public static class BuilderPipelineHelper
    {
        public static BuilderPipelineContext AddStep(this BuilderPipelineContext ctx, IBuildHandler nextHandler)
        {
            var nextContext = new BuilderPipelineContext(nextHandler);
            var origin_next = ctx.Next;
            ctx.Next = nextContext;
            nextContext.Next = origin_next;
            nextContext.Prev = ctx;
            if(origin_next != null)
            {
                origin_next.Prev = nextContext;
            }
            return nextContext;
        }
    }
}
