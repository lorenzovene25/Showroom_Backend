using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Exhibition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string MapsUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;


    public IEnumerable<ExhibitionTimeSlot> TimeSlots { get; set; }
    public IEnumerable<ArtworkExhibition> ArtworkExhibitions { get; set; }
}