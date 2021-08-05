namespace ShopLinhKien.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("leaseItem")]
    public partial class leaseItem
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
        public double Price { get; set; }

        public int CateId { get; set; }

        public string Description { get; set; }

        public string Content { get; set; }

        public int Status { get; set; }

        public int Order { get; set; }
    }
}
