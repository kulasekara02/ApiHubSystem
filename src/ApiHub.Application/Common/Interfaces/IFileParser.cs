namespace ApiHub.Application.Common.Interfaces;

public record ParsedFileData(
    List<string> Headers,
    List<Dictionary<string, object>> Rows,
    int RowCount,
    int ColumnCount);

public interface IFileParser
{
    Task<ParsedFileData> ParseCsvAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<ParsedFileData> ParseJsonAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
