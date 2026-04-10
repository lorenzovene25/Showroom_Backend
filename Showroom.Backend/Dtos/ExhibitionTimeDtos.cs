namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  EXHIBITION TIME SLOT
// ══════════════════════════════════════════════════════════════════

public record ExhibitionTimeSlotDto(
    int Id, int ExhibitionId, int[] DaysOfWeek,
    TimeOnly StartTime, TimeOnly EndTime, int MaxCapacity);

public record CreateExhibitionTimeSlotDto(
    int ExhibitionId, int[] DaysOfWeek,
    TimeOnly StartTime, TimeOnly EndTime, int MaxCapacity = 50);

public record UpdateExhibitionTimeSlotDto(
    int[] DaysOfWeek, TimeOnly StartTime,
    TimeOnly EndTime, int MaxCapacity);

public record PatchExhibitionTimeSlotDto(
    int[]? DaysOfWeek = null, TimeOnly? StartTime = null,
    TimeOnly? EndTime = null, int? MaxCapacity = null);