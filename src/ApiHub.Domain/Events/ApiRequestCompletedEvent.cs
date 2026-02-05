namespace ApiHub.Domain.Events;

public record ApiRequestCompletedEvent(
    Guid ApiRecordId,
    Guid UserId,
    Guid ConnectorId,
    string Url,
    int StatusCode,
    long DurationMs,
    bool IsSuccess) : DomainEvent;
