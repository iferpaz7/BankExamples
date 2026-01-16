namespace CreditCard.Web.Models;

using System.ComponentModel.DataAnnotations;

public class CreditCardViewModel
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public decimal UsedCredit => CreditLimit - AvailableCredit;
    public decimal UsagePercentage => CreditLimit > 0 ? Math.Round((UsedCredit / CreditLimit) * 100, 2) : 0;
    public string MaskedCardNumber => CardNumber.Length >= 4
        ? $"**** **** **** {CardNumber[^4..]}"
        : CardNumber;
}

public class CreateCreditCardModel
{
    [Required(ErrorMessage = "El nNúmero de tarjeta es requerido")]
    [StringLength(19, MinimumLength = 13, ErrorMessage = "El nNúmero debe tener entre 13 y 19 ddígitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo se permiten ddígitos")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del titular es requerido")]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de expiración es requerida")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Formato: MM/YY")]
    public string ExpirationDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CVV es requerido")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "El CVV debe tener 3 o 4 ddígitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Solo se permiten ddígitos")]
    public string CVV { get; set; } = string.Empty;

    [Required(ErrorMessage = "El límite de crédito es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El límite debe ser mayor a 0")]
    public decimal CreditLimit { get; set; } = 1000m;

    [Required(ErrorMessage = "El tipo de tarjeta es requerido")]
    public string CardType { get; set; } = "Visa";
}

public class UpdateCreditCardModel
{
    [Required(ErrorMessage = "El nombre del titular es requerido")]
    [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El límite de crédito es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El límite debe ser mayor a 0")]
    public decimal CreditLimit { get; set; }
}

public class TransactionModel
{
    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
