using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CodeMajestyTech.Performance.Post02.DataAccess;

/// <summary>
///     Counts the number of SQL commands executed against the database.
///     Used during benchmark calibration to surface the SQL-query-count
///     dimension of the N+1 problem alongside time/allocation measurements.
///     Not registered during BenchmarkDotNet measurement runs to keep the
///     hot path free of interceptor overhead.
/// </summary>
public sealed class QueryCounterInterceptor : DbCommandInterceptor
{
    private long _count;

    public long Count => Interlocked.Read(ref _count);

    public void Reset()
    {
        Interlocked.Exchange(ref _count, 0);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Interlocked.Increment(ref _count);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _count);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}