//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DubiSouqWebsite.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class product_picture
    {
        public int ID { get; set; }
        public int Product_ID { get; set; }
        public string Picture { get; set; }
    
        public virtual product product { get; set; }
    }
}
