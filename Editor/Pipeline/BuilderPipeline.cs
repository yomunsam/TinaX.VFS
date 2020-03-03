using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaXEditor.VFSKit.Pipeline.Builtin;

namespace TinaXEditor.VFSKit.Pipeline
{
    /// <summary>
    /// Pipeline管理类
    /// </summary>
    public class BuilderPipeline
    {
        private readonly BuilderPipelineContext _head;
        private readonly BuilderPipelineContext _last;

        public BuilderPipelineContext First => _head;

        public BuilderPipeline()
        {
            _head = new BuilderPipelineContext(new BuilderPipelineHead());
            _last = new BuilderPipelineContext(new BuilderPipelineLast());

            _head.Next = _last;
            _last.Prev = _head;
        }

        public BuilderPipelineContext AddFirst(IBuildHandler handler)
        {
            var context = new BuilderPipelineContext(handler);
            var _origin_first = _head.Next;
            context.Next = _origin_first;
            _head.Next = context;
            context.Prev = _head;
            if(_origin_first != null)
            {
                _origin_first.Prev = context;
            }
            return context;
        }


        public BuilderPipelineContext AddLast(IBuildHandler handler)
        {
            var context = new BuilderPipelineContext(handler);
            var origin_prev = _last.Prev;
            origin_prev.Next = context;
            context.Prev = origin_prev;
            _last.Prev = context;
            return context;
        }


    }
}
