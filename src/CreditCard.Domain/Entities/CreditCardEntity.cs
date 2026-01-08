namespace CreditCard.Domain.Entities;

public class CreditCardEntity
{
    public Guid Id { get; private set; }
    public string CardNumber { get; private set; } = string.Empty;
    /// <summary>
    /// HMAC hash of the card number for searchable uniqueness constraint.
    /// The actual CardNumber is encrypted, so we use this hash for lookups.
    /// </summary>
    public string CardNumberHash { get; private set; } = string.Empty;
    public string CardHolderName { get; private set; } = string.Empty;
    public string ExpirationDate { get; private set; } = string.Empty;
    public string CVV { get; private set; } = string.Empty;
    public decimal CreditLimit { get; private set; }
    public decimal AvailableCredit { get; private set; }
    public string CardType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CreditCardEntity() { }

    public static CreditCardEntity Create(
        string cardNumber,
        string cardHolderName,
        string expirationDate,
        string cvv,
        decimal creditLimit,
        string cardType,
        Func<string, string>? hashFunction = null)
    {
        ValidateCardNumber(cardNumber);
        ValidateCardHolderName(cardHolderName);
        ValidateCVV(cvv);

        return new CreditCardEntity
        {
            Id = Guid.NewGuid(),
            CardNumber = cardNumber,
            CardNumberHash = hashFunction?.Invoke(cardNumber) ?? string.Empty,
            CardHolderName = cardHolderName.ToUpper(),
            ExpirationDate = expirationDate,
            CVV = cvv,
            CreditLimit = creditLimit,
            AvailableCredit = creditLimit,
            CardType = cardType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sets the card number hash. Used when the hash function is not available at creation time.
    /// </summary>
    public void SetCardNumberHash(string hash)
    {
        if (!string.IsNullOrEmpty(hash))
        {
            CardNumberHash = hash;
        }
    }

    public void UpdateCardHolder(string newCardHolderName)
    {
        ValidateCardHolderName(newCardHolderName);
        CardHolderName = newCardHolderName.ToUpper();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCreditLimit(decimal newLimit)
    {
        if (newLimit <= 0)
            throw new ArgumentException("El límite de crédito debe ser mayor a 0");

        var difference = newLimit - CreditLimit;
        CreditLimit = newLimit;
        AvailableCredit += difference;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakeCharge(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0");

        if (!IsActive)
            throw new InvalidOperationException("La tarjeta está inactiva");

        if (amount > AvailableCredit)
            throw new InvalidOperationException("Crédito insuficiente");

        AvailableCredit -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakePayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0");

        var newAvailable = AvailableCredit + amount;
        if (newAvailable > CreditLimit)
            throw new InvalidOperationException("El pago excede el límite de crédito");

        AvailableCredit = newAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("El número de tarjeta es requerido");

        if (cardNumber.Length < 13 || cardNumber.Length > 19)
            throw new ArgumentException("El número de tarjeta debe tener entre 13 y 19 dígitos");
    }

    private static void ValidateCardHolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del titular es requerido");

        if (name.Length < 3)
            throw new ArgumentException("El nombre debe tener al menos 3 caracteres");
    }

    private static void ValidateCVV(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            throw new ArgumentException("El CVV es requerido");

        if (cvv.Length < 3 || cvv.Length > 4)
            throw new ArgumentException("El CVV debe tener 3 o 4 dígitos");
    }
}
