namespace EnglishCenter.SqlTests;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

internal sealed record SqlResult(string[] Columns, string[] Types, List<object?[]> Rows)
{
    internal object? Value(int row, string column) => Rows[row][Array.IndexOf(Columns, column)];
}

internal static class SqlRows
{
    internal static async Task<List<SqlResult>> ReadAsync(SqlFixture fixture, string statement, params (string Name, object? Value)[] parameters)
    {
        using var command = ((SqlConnection)fixture.Context.Database.GetDbConnection()).CreateCommand();
        command.Transaction = (SqlTransaction?)fixture.Context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = statement;
        foreach (var (name, value) in parameters)
        {
            if (value is DateOnly date) command.Parameters.Add(new SqlParameter(name, SqlDbType.Date) { Value = date.ToDateTime(TimeOnly.MinValue) });
            else command.Parameters.Add(new SqlParameter(name, value ?? DBNull.Value));
        }
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var sets = new List<SqlResult>();
        do
        {
            if (reader.FieldCount == 0) continue;
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
            var types = Enumerable.Range(0, reader.FieldCount).Select(reader.GetDataTypeName).ToArray();
            var rows = new List<object?[]>();
            while (await reader.ReadAsync(TestContext.Current.CancellationToken))
            {
                var row = new object?[reader.FieldCount];
                for (var i = 0; i < row.Length; i++) row[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            sets.Add(new(columns, types, rows));
        } while (await reader.NextResultAsync(TestContext.Current.CancellationToken));
        return sets;
    }
}
