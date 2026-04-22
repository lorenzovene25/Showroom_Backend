using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int SouvenirId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Cart Cart { get; set; }
    public Souvenir Souvenir { get; set; }
}