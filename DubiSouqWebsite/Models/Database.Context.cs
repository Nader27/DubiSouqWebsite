﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<address> addresses { get; set; }
        public virtual DbSet<cart_item> cart_item { get; set; }
        public virtual DbSet<category> categories { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<order> orders { get; set; }
        public virtual DbSet<order_item> order_item { get; set; }
        public virtual DbSet<order_status> order_status { get; set; }
        public virtual DbSet<payment_method> payment_method { get; set; }
        public virtual DbSet<product> products { get; set; }
        public virtual DbSet<product_picture> product_picture { get; set; }
        public virtual DbSet<product_type> product_type { get; set; }
        public virtual DbSet<report> reports { get; set; }
        public virtual DbSet<report_Type> report_Type { get; set; }
        public virtual DbSet<review> reviews { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<user_type> user_type { get; set; }
        public virtual DbSet<whish_list> whish_list { get; set; }
    }
}
