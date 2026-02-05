using System.Globalization;
using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace ApiHub.Infrastructure.Services;

public class FileParser : IFileParser
{
    public async Task<ParsedFileData> ParseCsvAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        });

        var records = new List<Dictionary<string, object>>();
        List<string>? headers = null;

        await csv.ReadAsync();
        csv.ReadHeader();
        headers = csv.HeaderRecord?.ToList() ?? new List<string>();

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            foreach (var header in headers)
            {
                var value = csv.GetField(header);
                row[header] = ParseValue(value);
            }
            records.Add(row);
        }

        return new ParsedFileData(
            headers,
            records,
            records.Count,
            headers.Count);
    }

    public async Task<ParsedFileData> ParseJsonAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        var json = await reader.ReadToEndAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        var records = new List<Dictionary<string, object>>();
        var headers = new HashSet<string>();

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var row = new Dictionary<string, object>();
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        headers.Add(prop.Name);
                        row[prop.Name] = GetJsonValue(prop.Value);
                    }
                }
                records.Add(row);
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            var row = new Dictionary<string, object>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                headers.Add(prop.Name);
                row[prop.Name] = GetJsonValue(prop.Value);
            }
            records.Add(row);
        }

        return new ParsedFileData(
            headers.ToList(),
            records,
            records.Count,
            headers.Count);
    }

    private static object ParseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (int.TryParse(value, out var intVal))
            return intVal;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleVal))
            return doubleVal;

        if (bool.TryParse(value, out var boolVal))
            return boolVal;

        if (DateTime.TryParse(value, out var dateVal))
            return dateVal;

        return value;
    }

    private static object GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number when element.TryGetInt32(out var i) => i,
            JsonValueKind.Number when element.TryGetDouble(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }
}
