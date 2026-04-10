namespace Showroom.Backend.Dtos;




// ══════════════════════════════════════════════════════════════════
//  ORDER
// ══════════════════════════════════════════════════════════════════

public record OrderDto(
    int Id, int UserId, string Status, decimal TotalAmount,
    DateTime CreatedAt, IEnumerable<OrderItemDto> Items);

public record OrderItemDto(
    int Id, int OrderId, int SouvenirId, string? SouvenirName,
    int Quantity, decimal UnitPrice);

public record CreateOrderDto(int UserId, IEnumerable<CreateOrderItemDto> Items);
public record CreateOrderItemDto(int SouvenirId, int Quantity);
public record PatchOrderStatusDto(string Status);
