using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════
//  TIME SLOT SERVICE
// ══════════════════════════════════════════════════════════════
public class TimeSlotService : ITimeSlotService
{
    protected readonly string _connectionString;

    public TimeSlotService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("db")!;
    }

    private NpgsqlConnection Conn() => new(_connectionString);

    // GET all slots for an exhibition
    public async Task<IEnumerable<ExhibitionTimeSlotDto>> GetTimeSlotsAsync(int exhibitionId)
    {
        using var conn = Conn();
        return await conn.QueryAsync<ExhibitionTimeSlotDto>("""
            SELECT 
                id            AS Id, 
                exhibition_id AS ExhibitionId, 
                days_of_week  AS DaysOfWeek, 
                start_time    AS StartTime, 
                end_time      AS EndTime, 
                max_capacity  AS MaxCapacity
            FROM exhibition_time_slots
            WHERE exhibition_id = @ExhibitionId
            ORDER BY start_time;
            """, new { ExhibitionId = exhibitionId });
    }

    // GET one slot by id
    public async Task<ExhibitionTimeSlotDto?> GetTimeSlotByIdAsync(int id)
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<ExhibitionTimeSlotDto>("""
            SELECT 
                id            AS Id, 
                exhibition_id AS ExhibitionId, 
                days_of_week  AS DaysOfWeek, 
                start_time    AS StartTime, 
                end_time      AS EndTime, 
                max_capacity  AS MaxCapacity
            FROM exhibition_time_slots
            WHERE id = @Id;
            """, new { Id = id });
    }

    // POST
    public async Task<ExhibitionTimeSlotDto> CreateTimeSlotAsync(CreateExhibitionTimeSlotDto dto)
    {
        using var conn = Conn();
        int id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO exhibition_time_slots
                (exhibition_id, days_of_week, start_time, end_time, max_capacity)
            VALUES
                (@ExhibitionId, @DaysOfWeek, @StartTime, @EndTime, @MaxCapacity)
            RETURNING id AS Id;
            """, dto);
        return (await GetTimeSlotByIdAsync(id))!;
    }

    // PUT
    public async Task<ExhibitionTimeSlotDto?> UpdateTimeSlotAsync(int id, UpdateExhibitionTimeSlotDto dto)
    {
        using var conn = Conn();
        int rows = await conn.ExecuteAsync("""
            UPDATE exhibition_time_slots
            SET days_of_week  = @DaysOfWeek,
                start_time    = @StartTime,
                end_time      = @EndTime,
                max_capacity  = @MaxCapacity
            WHERE id = @Id;
            """, new { Id = id, dto.DaysOfWeek, dto.StartTime, dto.EndTime, dto.MaxCapacity });

        return rows == 0 ? null : await GetTimeSlotByIdAsync(id);
    }

    // PATCH
    public async Task<ExhibitionTimeSlotDto?> PatchTimeSlotAsync(int id, PatchExhibitionTimeSlotDto dto)
    {
        using var conn = Conn();
        var sets = new List<string>();
        var p = new DynamicParameters(new { Id = id });

        if (dto.DaysOfWeek != null) { sets.Add("days_of_week = @DaysOfWeek"); p.Add("DaysOfWeek", dto.DaysOfWeek); }
        if (dto.StartTime != null) { sets.Add("start_time = @StartTime"); p.Add("StartTime", dto.StartTime); }
        if (dto.EndTime != null) { sets.Add("end_time = @EndTime"); p.Add("EndTime", dto.EndTime); }
        if (dto.MaxCapacity != null) { sets.Add("max_capacity = @MaxCapacity"); p.Add("MaxCapacity", dto.MaxCapacity); }

        if (sets.Count == 0) return await GetTimeSlotByIdAsync(id);

        int rows = await conn.ExecuteAsync(
            $"UPDATE exhibition_time_slots SET {string.Join(", ", sets)} WHERE id = @Id", p);
        return rows == 0 ? null : await GetTimeSlotByIdAsync(id);
    }

    // DELETE
    public async Task<bool> DeleteTimeSlotAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM exhibition_time_slots WHERE id = @Id", new { Id = id }) > 0;
    }
}