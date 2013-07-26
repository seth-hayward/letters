﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace letterstocrushes.Infrastructure
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class db_mysql : DbContext
    {
        public db_mysql()
            : base("name=db_mysql")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<block> blocks { get; set; }
        public DbSet<chat_visitors> chat_visitors { get; set; }
        public DbSet<chat> chats { get; set; }
        public DbSet<comment> comments { get; set; }
        public DbSet<letter> letters { get; set; }
        public DbSet<song> songs { get; set; }
    
        public virtual ObjectResult<searchLetters_Result> searchLetters(string search_terms)
        {
            var search_termsParameter = search_terms != null ?
                new ObjectParameter("search_terms", search_terms) :
                new ObjectParameter("search_terms", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<searchLetters_Result>("searchLetters", search_termsParameter);
        }
    
        public virtual int quickSearch(string search_terms)
        {
            var search_termsParameter = search_terms != null ?
                new ObjectParameter("search_terms", search_terms) :
                new ObjectParameter("search_terms", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("quickSearch", search_termsParameter);
        }
    }
}
