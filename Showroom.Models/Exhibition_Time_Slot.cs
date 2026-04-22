using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class ExhibitionTimeSlot
{
    public int Id { get; set; }
    public int ExhibitionId { get; set; }
    public int[] DaysOfWeek { get; set; }       // e.g. new[]{3,4,5} = Wed,Thu,Fri
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxCapacity { get; set; }

    // Navigation
    public Exhibition Exhibition { get; set; }
}

