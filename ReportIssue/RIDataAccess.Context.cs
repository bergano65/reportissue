﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReportIssue
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class RIDataModelContainer : DbContext
    {
        public RIDataModelContainer()
            : base("name=RIDataModelContainer")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Filter> Filters { get; set; }
        public virtual DbSet<Issue> Issues { get; set; }
        public virtual DbSet<Picture> Pictures { get; set; }
    }
}
