using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Souvenir
{
    public int Id { get; set; }
    public string ArchiveId { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public int QuantityAvailable { get; set; }
    public string ImageUrl { get; set; }
    public string Specifications { get; set; }  // JSONB stored as raw JSON string
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
   

    // Navigation
    public Category Category { get; set; }
}

