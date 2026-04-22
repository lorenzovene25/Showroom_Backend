using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class ArtworkExhibition
{
    public int ArtworkId { get; set; }
    public int ExhibitionId { get; set; }

    // Navigation
    public Artwork Artwork { get; set; }
    public Exhibition Exhibition { get; set; }
}