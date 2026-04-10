using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; }          // "pending" | "paid" | "shipped" | "delivered" | "cancelled"
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}
