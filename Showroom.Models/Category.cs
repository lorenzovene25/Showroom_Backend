using System;
using System.Collections.Generic;
using System.Text;

namespace Showroom.Models;

public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Navigation
    public ICollection<Souvenir> Souvenirs { get; set; }
}