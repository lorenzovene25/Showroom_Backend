using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Ticket
{
    public int Id { get; set; }
    public int ExhibitionId { get; set; }
    public int TierId { get; set; }
    public int UserId { get; set; }
    public DateOnly VisitDate { get; set; }
    public int TimeSlotId { get; set; }
    public DateTime PurchasedAt { get; set; }


    // Navigation
    public Exhibition Exhibition { get; set; }
    public TicketTier Tier { get; set; }
    public User User { get; set; }
    public ExhibitionTimeSlot TimeSlot { get; set; }
}