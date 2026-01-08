namespace CreditCard.Infrastructure.Security;

/// <summary>
/// Interface for encryption/decryption services with authenticated encryption (AEAD).
/// Uses AES-GCM which provides both confidentiality and integrity (MAC).
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the plaintext using AES-GCM authenticated encryption.
    /// The output includes the nonce and authentication tag.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64 encoded encrypted data with nonce and tag</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts the ciphertext using AES-GCM authenticated encryption.
    /// Verifies the authentication tag (MAC) before returning the plaintext.
    /// </summary>
    /// <param name="cipherText">Base64 encoded encrypted data with nonce and tag</param>
    /// <returns>The decrypted plaintext</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Computes a deterministic hash for the given value.
    /// Used for searchable encrypted fields (like card number for uniqueness checks).
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <returns>Base64 encoded HMAC hash</returns>
    string ComputeHash(string value);
}
