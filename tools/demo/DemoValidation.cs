using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Operator;
using Microsoft.Data.SqlClient;

internal static class DemoValidation
{
    public static async Task<int> Run()
    {
        await using var connection=new SqlConnection(LocalRuntimeReferences.SqlConnection);await connection.OpenAsync();
        await using var command=connection.CreateCommand();command.CommandTimeout=10;
        command.CommandText=await File.ReadAllTextAsync(@"C:\Study\SOEN\main_workplace\tools\demo\Validate-Demo.sql");
        await using var reader=await command.ExecuteReaderAsync();await reader.ReadAsync();
        var values=Enumerable.Range(0,reader.FieldCount).ToDictionary(reader.GetName,reader.GetValue);
        Console.WriteLine(JsonSerializer.Serialize(values));return 0;
    }
}
