namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  CART
// ══════════════════════════════════════════════════════════════════

public record CartDto(
    int Id, DateTime CreatedAt, IEnumerable<CartItemDto> Items);

public record CartItemDto(
    int Id, int CartId, int SouvenirId, string? SouvenirName,
    decimal SouvenirPrice, int Quantity);

public record AddCartItemDto(int SouvenirId, int Quantity = 1);
public record PatchCartItemDto(int Quantity);
