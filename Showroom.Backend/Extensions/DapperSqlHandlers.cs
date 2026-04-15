using Dapper;
using System.Data;

namespace Showroom.Backend.Extensions;

/// <summary>
/// Custom Dapper type handlers for DateOnly and TimeOnly types.
/// These handlers convert between .NET types and SQL Server/PostgreSQL types.
/// </summary>

/// <summary>
/// Handles conversion between DateOnly (.NET type) and DATE (SQL type).
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s),
            _ => throw new ArgumentException($"Cannot convert value of type {value?.GetType()} to DateOnly", nameof(value))
        };
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

/// <summary>
/// Handles conversion between TimeOnly (.NET type) and TIME (SQL type).
/// </summary>
public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
    {
        return value switch
        {
            TimeOnly t => t,
            DateTime dt => TimeOnly.FromDateTime(dt),
            TimeSpan ts => TimeOnly.FromTimeSpan(ts),
            string s => TimeOnly.Parse(s),
            _ => throw new ArgumentException($"Cannot convert value of type {value?.GetType()} to TimeOnly", nameof(value))
        };
    }

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = value.ToTimeSpan();
    }
}

/// <summary>
/// Extension methods for registering Dapper type handlers.
/// </summary>
public static class DapperSqlHandlersExtensions
{
    /// <summary>
    /// Registers custom type handlers for DateOnly and TimeOnly with Dapper.
    /// Call this at application startup before any database operations.
    /// </summary>
    public static void AddDapperTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }
}
