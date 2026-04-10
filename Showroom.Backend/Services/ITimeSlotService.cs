using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public interface ITimeSlotService
{
    Task<IEnumerable<ExhibitionTimeSlotDto>> GetTimeSlotsAsync(int exhibitionId);
    Task<ExhibitionTimeSlotDto?> GetTimeSlotByIdAsync(int id);
    Task<ExhibitionTimeSlotDto> CreateTimeSlotAsync(CreateExhibitionTimeSlotDto dto);
    Task<ExhibitionTimeSlotDto?> UpdateTimeSlotAsync(int id, UpdateExhibitionTimeSlotDto dto);
    Task<ExhibitionTimeSlotDto?> PatchTimeSlotAsync(int id, PatchExhibitionTimeSlotDto dto);
    Task<bool> DeleteTimeSlotAsync(int id);
}