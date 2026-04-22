namespace Showroom.Backend.Dtos;

public class TicketTierDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class CreateTicketTierDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Culture { get; set; } = "en";
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateTicketTierDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class PatchTicketTierDto
{
    public string? Type { get; set; }
    public decimal? Price { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}