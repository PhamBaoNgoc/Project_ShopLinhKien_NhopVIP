using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ShopLinhKien.Models
{
    public class ShopLinhKienDbContext : DbContext
    {
        public ShopLinhKienDbContext() : base("name=ChuoiKN")
        { }
        public virtual DbSet<Mcategory> Categorys { get; set; }
        public virtual DbSet<Mcontact> Contacts { get; set; }
        public virtual DbSet<link> Link { get; set; }
        public virtual DbSet<Mmenu> Menus { get; set; }
        public virtual DbSet<Morder> Orders { get; set; }
        public virtual DbSet<leaseCategory> leaseCategories { get; set; }
        public virtual DbSet<leaseItem> leaseItems { get; set; }
        public virtual DbSet<LeaseOrder> LeaseOrders { get; set; }
        public virtual DbSet<Mordersdetail> Orderdetails { get; set; }
        public virtual DbSet<Mpost> posts { get; set; }
        public virtual DbSet<Mproduct> Products { get; set; }
        public virtual DbSet<Mslider> Sliders { get; set; }
        public virtual DbSet<Mtopic> topics { get; set; }
        public virtual DbSet<Muser> users { get; set; }
        public virtual DbSet<role> roles { get; set; }
        internal int ExecuteScalar()
        {
            throw new NotImplementedException();
        }
    }
}
