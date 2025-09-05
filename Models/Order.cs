using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Order
    {
        [Key] 
        public int OID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

        [ForeignKey("user")]
        public int UID { get; set; }
        public virtual User user { get; set; }


        [JsonIgnore]
        public virtual ICollection <OrderProducts> OrderProducts { get; set; } // use virtual for lazy loading
    }

    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4

        //Pending, Paid, Shipped, Delivered, Cancelled
    }
}
