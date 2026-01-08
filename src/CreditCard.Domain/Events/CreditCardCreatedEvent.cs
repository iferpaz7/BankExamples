namespace CreditCard.Domain.Events;

public class CreditCardCreatedEvent : IDomainEvent
{
    public Guid CardId { get; }
    public string CardNumber { get; }
    public string CardHolderName { get; }
    public DateTime OccurredOn { get; }

    public CreditCardCreatedEvent(Guid cardId, string cardNumber, string cardHolderName)
    {
        CardId = cardId;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        OccurredOn = DateTime.UtcNow;
    }
}
