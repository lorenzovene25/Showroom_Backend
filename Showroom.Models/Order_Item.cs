using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int SouvenirId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation
    public Order Order { get; set; }
    public Souvenir Souvenir { get; set; }
}
