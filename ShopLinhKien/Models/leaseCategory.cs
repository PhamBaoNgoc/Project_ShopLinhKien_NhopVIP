namespace ShopLinhKien.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("leaseCategory")]
    public partial class leaseCategory
    {
        public int ID { get; set; }

        public string Name { get; set; }
        public DateTime created_at { get; set; }

        public int Status { get; set; }
    }
}
