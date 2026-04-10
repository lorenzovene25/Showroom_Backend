namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  ORDER
// ══════════════════════════════════════════════════════════════════

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int SouvenirId { get; set; }
    public string? SouvenirName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateOrderDto
{
    public int UserId { get; set; }
    public IEnumerable<CreateOrderItemDto> Items { get; set; } = [];
}

public class CreateOrderItemDto
{
    public int SouvenirId { get; set; }
    public int Quantity { get; set; }
}

public class PatchOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}