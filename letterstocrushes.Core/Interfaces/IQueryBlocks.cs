﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryBlocks
    {
        void Add(int type, string value, int what, string notes);
        List<Block> getBlocks(blockType type, blockWhat what);
        void Remove(int id);
        Block getBlock(int id);
    }
}
