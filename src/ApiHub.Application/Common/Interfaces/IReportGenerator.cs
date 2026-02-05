using ApiHub.Domain.Enums;

namespace ApiHub.Application.Common.Interfaces;

public interface IReportGenerator
{
    Task<byte[]> GeneratePdfAsync(string templateType, object data, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateExcelAsync(string templateType, object data, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateAsync(string templateType, ReportFormat format, object data, CancellationToken cancellationToken = default);
}
