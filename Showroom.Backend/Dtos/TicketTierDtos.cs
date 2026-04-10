namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  TICKET TIER
// ══════════════════════════════════════════════════════════════════

public record TicketTierDto(
    int Id, string Type, decimal Price,
    // from ticket_tier_translations (culture-aware)
    string? Name, string? Description);

public record CreateTicketTierDto(
    string Type, decimal Price,
    string Culture = "en",
    string? Name = null, string? Description = null);

public record UpdateTicketTierDto(
    string Type, decimal Price,
    string? Name = null, string? Description = null);

public record PatchTicketTierDto(
    string? Type = null, decimal? Price = null,
    string? Name = null, string? Description = null);
