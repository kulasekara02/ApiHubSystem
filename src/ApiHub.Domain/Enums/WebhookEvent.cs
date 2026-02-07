namespace ApiHub.Domain.Enums;

public enum WebhookEvent
{
    ApiRequestCompleted = 0,
    ApiRequestFailed = 1,
    ConnectorCreated = 10,
    ConnectorUpdated = 11,
    ConnectorDeleted = 12,
    UserRegistered = 20,
    ReportGenerated = 30,
    FileUploaded = 40
}
