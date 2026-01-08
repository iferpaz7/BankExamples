namespace CreditCard.Application.Interfaces;

/// <summary>
/// Interface for computing hashes of sensitive data.
/// Used for searchable encrypted fields.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes a deterministic hash for the given value.
    /// </summary>
    string ComputeHash(string value);
}
