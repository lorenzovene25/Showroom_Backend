namespace Showroom.Backend.Dtos;
public class ExhibitionTimeSlotDto
{
    public int Id { get; set; }
    public int ExhibitionId { get; set; }
    public int[] DaysOfWeek { get; set; } = [];
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxCapacity { get; set; }
}

public class CreateExhibitionTimeSlotDto
{
    public int ExhibitionId { get; set; }
    public int[] DaysOfWeek { get; set; } = [];
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxCapacity { get; set; } = 50;
}

public class UpdateExhibitionTimeSlotDto
{
    public int[] DaysOfWeek { get; set; } = [];
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxCapacity { get; set; }
}

public class PatchExhibitionTimeSlotDto
{
    public int[]? DaysOfWeek { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int? MaxCapacity { get; set; }
}