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
    
    public partial class survey
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public Nullable<System.DateTime> SubmitDate { get; set; }
        public string Email { get; set; }
        public Nullable<int> Age { get; set; }
        public Nullable<byte> Gender { get; set; }
        public Nullable<byte> ReaderTime { get; set; }
        public Nullable<int> Language_English { get; set; }
        public Nullable<int> Language_Spanish { get; set; }
        public Nullable<int> Language_Mandarin { get; set; }
        public Nullable<int> Language_Hindi { get; set; }
        public Nullable<int> Language_Arabic { get; set; }
        public Nullable<int> Language_Bengali { get; set; }
        public Nullable<int> Language_Portuguese { get; set; }
        public Nullable<int> Language_Japanese { get; set; }
        public Nullable<int> Language_German { get; set; }
        public Nullable<int> Language_Tagalog { get; set; }
        public Nullable<int> Language_French { get; set; }
        public string Language_Other { get; set; }
        public Nullable<int> Timezone { get; set; }
        public Nullable<byte> Swearing { get; set; }
        public string Feedback { get; set; }
        public Nullable<byte> BuyBook { get; set; }
        public Nullable<byte> ShippingLocation { get; set; }
        public Nullable<int> CookieCounter { get; set; }
        public Nullable<int> FrontPageFrequency { get; set; }
        public Nullable<int> BestRename { get; set; }
    }
}
