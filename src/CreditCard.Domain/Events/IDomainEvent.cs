namespace CreditCard.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
