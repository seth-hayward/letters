//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class edit
    {
        public int ID { get; set; }
        public int letterID { get; set; }
        public string previousLetter { get; set; }
        public string newLetter { get; set; }
        public System.DateTime editDate { get; set; }
        public string status { get; set; }
        public string editor { get; set; }
        public string editComment { get; set; }
    }
}
