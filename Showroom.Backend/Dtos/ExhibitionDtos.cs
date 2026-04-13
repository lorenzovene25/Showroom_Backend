namespace Showroom.Backend.Dtos;

public class ExhibitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? MapsUrl { get; set; } = null;
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ImageUrl { get; set; } = null;
    public string? Title { get; set; } = null;
    public string? Description { get; set; } = null;
}
public class CreateExhibitionDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? MapsUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public string? Title { get; set; } = null;
    public string Culture { get; set; } = "en";
    public string? Description { get; set; } = null;
}

public class UpdateExhibitionDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? MapsUrl { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public string? Title { get; set; } = null;
    public string? Description { get; set; } = null;
}

public class PatchExhibitionDto
{
    public string? Name { get; set; } = null;
    public string? Location { get; set; } = null;
    public string? MapsUrl { get; set; } = null;
    public string? Status { get; set; } = null;
    public DateOnly? StartDate { get; set; } = null;
    public DateOnly? EndDate { get; set; } = null;
    public string? ImageUrl { get; set; } = null;
    public string? Title { get; set; } = null;
    public string? Description { get; set; } = null;
}