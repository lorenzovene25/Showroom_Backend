namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  TICKET
// ══════════════════════════════════════════════════════════════════

public class TicketDto
{
    public int Id { get; set; }
    public int ExhibitionId { get; set; }
    public string? ExhibitionTitle { get; set; }
    public int TierId { get; set; }
    public string? TierName { get; set; }
    public decimal TierPrice { get; set; }
    public int UserId { get; set; }
    public DateOnly VisitDate { get; set; }
    public int TimeSlotId { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public DateTime PurchasedAt { get; set; }
}

public class CreateTicketDto
{
    public int ExhibitionId { get; set; }
    public int TierId { get; set; }
    public int UserId { get; set; }
    public DateOnly VisitDate { get; set; }
    public int TimeSlotId { get; set; }
}