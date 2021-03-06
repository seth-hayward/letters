﻿using System;
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

        public void Add(int type, string value, int what, string notes)
        {
            block b = new block();
            b.Type = type;
            b.Value = value;
            b.What = what;
            b.Notes = notes;
            b.Date = DateTime.UtcNow;

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

        public Core.Model.Block getBlock(int id)
        {
            block chosen_one = (from m in db_mysql.blocks where m.Id == id select m).FirstOrDefault();
            return Mapper.Map<block, Block>(chosen_one);
        }

        public void Remove(int id)
        {

            block fated_block = (from m in db_mysql.blocks where m.Id == id select m).FirstOrDefault();

            if(fated_block != null) {
                db_mysql.blocks.Remove(fated_block);
                db_mysql.SaveChanges();
            }

        }
    }
}
