namespace ShopLinhKien.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LeaseOrder")]
    public partial class LeaseOrder
    {
        [Key]
        public int ID { get; set; }

        public int LeaseItemID { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Emal { get; set; }

        public string Phone { get; set; }

        public double TotalPrice { get; set; }

        public int StatusPayment { get; set; }
        public string PaymentMethod { get; set; }


        public int RentalPeriod { get; set; }
    }
}
