namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  TICKET
// ══════════════════════════════════════════════════════════════════

public record TicketDto(
    int Id, int ExhibitionId, string? ExhibitionTitle,
    int TierId, string? TierName, decimal TierPrice,
    int UserId, DateOnly VisitDate,
    int TimeSlotId, TimeOnly SlotStartTime, TimeOnly SlotEndTime,
    DateTime PurchasedAt);

public record CreateTicketDto(
    int ExhibitionId, int TierId, int UserId,
    DateOnly VisitDate, int TimeSlotId);
