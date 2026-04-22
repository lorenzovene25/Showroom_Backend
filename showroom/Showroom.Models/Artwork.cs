using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Artwork
{
    public int Id { get; set; }
    public string ArchiveId { get; set; }
    public string Name { get; set; }
    public int? Year { get; set; }
    public string Dimensions { get; set; }
    public string ImageUrl { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string HistoricalPeriod { get; set; }
    public string Support { get; set; }
    public string Camera { get; set; }

    // Navigation

    public IEnumerable<ArtworkExhibition> ArtworkExhibitions { get; set; }
}