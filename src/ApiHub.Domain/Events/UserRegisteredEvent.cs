namespace ApiHub.Domain.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName) : DomainEvent;
