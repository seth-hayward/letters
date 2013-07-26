using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryBlocks : IQueryBlocks
    {

        private db_mysql _db_mysql;
        public db_mysql db_mysql
        {
            get
            {
                if (_db_mysql == null)
                {
                    _db_mysql = new db_mysql();
                }

                return _db_mysql;
            }
            set
            {
                _db_mysql = value;
            }
        }

        public EfQueryBlocks()
        {
            Mapper.CreateMap<Block, block>();
            Mapper.CreateMap<block, Block>();
        }

        public void Add(int type, string value, int what)
        {
            block b = new block();
            b.Type = type;
            b.Value = value;
            b.What = what;

            db_mysql.blocks.Add(b);
            db_mysql.SaveChanges();

        }

        public List<Core.Model.Block> getBlocks(blockType type, blockWhat what)
        {

            int block_type = (int)type;
            int block_what = (int)what;

            List<block> blocks = (from m in db_mysql.blocks where m.Type == block_type && m.What == block_what select m).ToList();
            return Mapper.Map<List<block>, List<Block>>(blocks);
        }
    }
}
