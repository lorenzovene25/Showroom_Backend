namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  CART
// ══════════════════════════════════════════════════════════════════

public class CartDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<CartItemDto> Items { get; set; } = new List<CartItemDto>();
}

public class CartItemDto
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int SouvenirId { get; set; }
    public string? SouvenirName { get; set; }
    public decimal SouvenirPrice { get; set; }
    public int Quantity { get; set; }
    public string SouvenirImageUrl { get; set; } = string.Empty;
}

public class AddCartItemDto
{
    public int SouvenirId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class PatchCartItemDto
{
    public int Quantity { get; set; }
}

public class CheckoutItemDto
{
    public int CartId { get; set; }
    public int SouvenirId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}