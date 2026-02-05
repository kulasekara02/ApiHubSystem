using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Enums;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiHub.Infrastructure.Services;

public class ReportGenerator : IReportGenerator
{
    static ReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePdfAsync(string templateType, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data);
        var reportData = JsonSerializer.Deserialize<JsonElement>(json);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text(reportData.TryGetProperty("Title", out var title) ? title.GetString() : "Report")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        if (reportData.TryGetProperty("GeneratedAt", out var generatedAt))
                        {
                            column.Item().Text($"Generated: {generatedAt.GetDateTime():yyyy-MM-dd HH:mm:ss UTC}");
                        }

                        if (reportData.TryGetProperty("GeneratedBy", out var generatedBy))
                        {
                            column.Item().Text($"By: {generatedBy.GetString()}");
                        }

                        if (reportData.TryGetProperty("TotalRecords", out var totalRecords))
                        {
                            column.Item().Text($"Total Records: {totalRecords.GetInt32()}");
                        }

                        if (reportData.TryGetProperty("SuccessRate", out var successRate))
                        {
                            column.Item().Text($"Success Rate: {successRate.GetDouble()}%");
                        }

                        column.Item().PaddingTop(20);

                        if (reportData.TryGetProperty("Records", out var records) &&
                            records.ValueKind == JsonValueKind.Array)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Connector").Bold();
                                    header.Cell().Element(CellStyle).Text("Method").Bold();
                                    header.Cell().Element(CellStyle).Text("URL").Bold();
                                    header.Cell().Element(CellStyle).Text("Status").Bold();
                                    header.Cell().Element(CellStyle).Text("Duration").Bold();
                                    header.Cell().Element(CellStyle).Text("Time").Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                    }
                                });

                                foreach (var record in records.EnumerateArray().Take(100))
                                {
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("ConnectorName", out var cn) ? cn.GetString() : "");
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("Method", out var m) ? m.ToString() : "");
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("RequestUrl", out var url) ? TruncateUrl(url.GetString()) : "");
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("StatusCode", out var sc) ? sc.GetInt32().ToString() : "");
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("DurationMs", out var d) ? $"{d.GetInt64()}ms" : "");
                                    table.Cell().Element(CellStyle).Text(
                                        record.TryGetProperty("CreatedAt", out var ca) ? ca.GetDateTime().ToString("g") : "");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(3);
                                    }
                                }
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> GenerateExcelAsync(string templateType, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data);
        var reportData = JsonSerializer.Deserialize<JsonElement>(json);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        var row = 1;

        if (reportData.TryGetProperty("Title", out var title))
        {
            worksheet.Cell(row, 1).Value = title.GetString();
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 16;
            row += 2;
        }

        if (reportData.TryGetProperty("GeneratedAt", out var generatedAt))
        {
            worksheet.Cell(row, 1).Value = "Generated:";
            worksheet.Cell(row, 2).Value = generatedAt.GetDateTime();
            row++;
        }

        if (reportData.TryGetProperty("TotalRecords", out var totalRecords))
        {
            worksheet.Cell(row, 1).Value = "Total Records:";
            worksheet.Cell(row, 2).Value = totalRecords.GetInt32();
            row++;
        }

        if (reportData.TryGetProperty("SuccessRate", out var successRate))
        {
            worksheet.Cell(row, 1).Value = "Success Rate:";
            worksheet.Cell(row, 2).Value = $"{successRate.GetDouble()}%";
            row++;
        }

        row += 2;

        if (reportData.TryGetProperty("Records", out var records) &&
            records.ValueKind == JsonValueKind.Array)
        {
            var headers = new[] { "Connector", "Method", "URL", "Status Code", "Duration (ms)", "Success", "Time" };
            for (var col = 0; col < headers.Length; col++)
            {
                worksheet.Cell(row, col + 1).Value = headers[col];
                worksheet.Cell(row, col + 1).Style.Font.Bold = true;
                worksheet.Cell(row, col + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            row++;

            foreach (var record in records.EnumerateArray())
            {
                worksheet.Cell(row, 1).Value = record.TryGetProperty("ConnectorName", out var cn) ? cn.GetString() : "";
                worksheet.Cell(row, 2).Value = record.TryGetProperty("Method", out var m) ? m.ToString() : "";
                worksheet.Cell(row, 3).Value = record.TryGetProperty("RequestUrl", out var url) ? url.GetString() : "";
                worksheet.Cell(row, 4).Value = record.TryGetProperty("StatusCode", out var sc) ? sc.GetInt32() : 0;
                worksheet.Cell(row, 5).Value = record.TryGetProperty("DurationMs", out var d) ? d.GetInt64() : 0;
                worksheet.Cell(row, 6).Value = record.TryGetProperty("IsSuccess", out var s) && s.GetBoolean() ? "Yes" : "No";
                worksheet.Cell(row, 7).Value = record.TryGetProperty("CreatedAt", out var ca) ? ca.GetDateTime() : DateTime.MinValue;
                row++;
            }

            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> GenerateAsync(string templateType, ReportFormat format, object data, CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ReportFormat.PDF => GeneratePdfAsync(templateType, data, cancellationToken),
            ReportFormat.Excel => GenerateExcelAsync(templateType, data, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static string? TruncateUrl(string? url, int maxLength = 40)
    {
        if (string.IsNullOrEmpty(url) || url.Length <= maxLength)
            return url;
        return url[..(maxLength - 3)] + "...";
    }
}
