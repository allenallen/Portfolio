﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PortfolioController
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class StockXDataBaseEntities : DbContext
    {
        public StockXDataBaseEntities()
            : base("name=StockXDataBaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<ClientAverageCost> ClientAverageCosts { get; set; }
        public DbSet<MatchedOrder> MatchedOrders { get; set; }
        public DbSet<Summary> Summaries { get; set; }
        public DbSet<PosCost> PosCosts { get; set; }
    }
}