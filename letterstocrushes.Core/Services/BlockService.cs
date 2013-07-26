using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{
    public class BlockService
    {

        private readonly IQueryBlocks _queryBlocks;
        public BlockService(IQueryBlocks queryBlocks)
        {
            _queryBlocks = queryBlocks;
        }

        public void Add(int type, int what, string value)
        {
            _queryBlocks.Add(type, value, what);
        }

        public List<Block> getBlocks(blockType type, blockWhat what)
        {
            return _queryBlocks.getBlocks(type, what);
        }

    }
}
