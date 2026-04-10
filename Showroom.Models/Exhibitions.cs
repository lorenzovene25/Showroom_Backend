using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Exhibition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string MapsUrl { get; set; }
    public string Status { get; set; }          // "ongoing" | "past" | "upcoming"
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ImageUrl { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }

    // Navigation

    public IEnumerable<ExhibitionTimeSlot> TimeSlots { get; set; }
    public IEnumerable<ArtworkExhibition> ArtworkExhibitions { get; set; }
}