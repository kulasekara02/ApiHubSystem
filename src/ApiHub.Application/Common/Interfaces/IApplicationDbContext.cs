using ApiHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Connector> Connectors { get; }
    DbSet<ConnectorEndpoint> ConnectorEndpoints { get; }
    DbSet<UserConnectorSecret> UserConnectorSecrets { get; }
    DbSet<ApiRecord> ApiRecords { get; }
    DbSet<Dataset> Datasets { get; }
    DbSet<Report> Reports { get; }
    DbSet<UploadedFile> UploadedFiles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RequestTemplate> RequestTemplates { get; }
    DbSet<ScheduledJob> ScheduledJobs { get; }
    DbSet<ScheduledJobExecution> ScheduledJobExecutions { get; }
    DbSet<Webhook> Webhooks { get; }
    DbSet<WebhookDelivery> WebhookDeliveries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
