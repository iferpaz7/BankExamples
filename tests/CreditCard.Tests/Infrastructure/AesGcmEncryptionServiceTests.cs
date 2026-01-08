namespace CreditCard.Tests.Infrastructure;

using System.Security.Cryptography;
using CreditCard.Infrastructure.Security;
using FluentAssertions;
using Xunit;

public class AesGcmEncryptionServiceTests
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hmacKey;
    private readonly AesGcmEncryptionService _encryptionService;

    public AesGcmEncryptionServiceTests()
    {
        // Generate random keys for testing
        _encryptionKey = new byte[32];
        _hmacKey = new byte[32];
        RandomNumberGenerator.Fill(_encryptionKey);
        RandomNumberGenerator.Fill(_hmacKey);
        
        _encryptionService = new AesGcmEncryptionService(_encryptionKey, _hmacKey);
    }

    #region Encrypt Tests

    [Fact]
    public void Encrypt_WithValidPlainText_ShouldReturnEncryptedString()
    {
        // Arrange
        var plainText = "4111111111111111";

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plainText);
    }

    [Fact]
    public void Encrypt_WithSamePlainTextTwice_ShouldReturnDifferentCipherTexts()
    {
        // Arrange
        var plainText = "4111111111111111";

        // Act
        var encrypted1 = _encryptionService.Encrypt(plainText);
        var encrypted2 = _encryptionService.Encrypt(plainText);

        // Assert - Different due to random nonce
        encrypted1.Should().NotBe(encrypted2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Encrypt_WithNullOrEmptyPlainText_ShouldReturnSameValue(string? plainText)
    {
        // Act
        var encrypted = _encryptionService.Encrypt(plainText!);

        // Assert
        encrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_WithSpecialCharacters_ShouldEncryptSuccessfully()
    {
        // Arrange
        var plainText = "Test with special chars: áéíóú ñ ¿? !@ #$%^&*()";

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plainText);
    }

    [Fact]
    public void Encrypt_WithLongText_ShouldEncryptSuccessfully()
    {
        // Arrange
        var plainText = new string('A', 10000);

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Decrypt Tests

    [Fact]
    public void Decrypt_WithValidCipherText_ShouldReturnOriginalPlainText()
    {
        // Arrange
        var plainText = "4111111111111111";
        var encrypted = _encryptionService.Encrypt(plainText);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Decrypt_WithNullOrEmptyCipherText_ShouldReturnSameValue(string? cipherText)
    {
        // Act
        var decrypted = _encryptionService.Decrypt(cipherText!);

        // Assert
        decrypted.Should().Be(cipherText);
    }

    [Fact]
    public void Decrypt_WithInvalidCipherText_ShouldThrowException()
    {
        // Arrange
        var invalidCipherText = Convert.ToBase64String(new byte[50]);

        // Act
        var act = () => _encryptionService.Decrypt(invalidCipherText);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithTamperedCipherText_ShouldThrowException()
    {
        // Arrange
        var plainText = "4111111111111111";
        var encrypted = _encryptionService.Encrypt(plainText);
        var bytes = Convert.FromBase64String(encrypted);
        bytes[20] ^= 0xFF; // Tamper with the ciphertext
        var tamperedCipherText = Convert.ToBase64String(bytes);

        // Act
        var act = () => _encryptionService.Decrypt(tamperedCipherText);

        // Assert - Should fail MAC verification
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithSpecialCharacters_ShouldDecryptSuccessfully()
    {
        // Arrange
        var plainText = "Test with special chars: áéíóú ñ ¿? !@ #$%^&*()";
        var encrypted = _encryptionService.Encrypt(plainText);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_WithLongText_ShouldDecryptSuccessfully()
    {
        // Arrange
        var plainText = new string('A', 10000);
        var encrypted = _encryptionService.Encrypt(plainText);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    #endregion

    #region ComputeHash Tests

    [Fact]
    public void ComputeHash_WithValidValue_ShouldReturnHash()
    {
        // Arrange
        var value = "4111111111111111";

        // Act
        var hash = _encryptionService.ComputeHash(value);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(value);
    }

    [Fact]
    public void ComputeHash_WithSameValueTwice_ShouldReturnSameHash()
    {
        // Arrange
        var value = "4111111111111111";

        // Act
        var hash1 = _encryptionService.ComputeHash(value);
        var hash2 = _encryptionService.ComputeHash(value);

        // Assert - Deterministic for searchability
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeHash_WithDifferentValues_ShouldReturnDifferentHashes()
    {
        // Arrange
        var value1 = "4111111111111111";
        var value2 = "5500000000000004";

        // Act
        var hash1 = _encryptionService.ComputeHash(value1);
        var hash2 = _encryptionService.ComputeHash(value2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ComputeHash_WithNullOrEmptyValue_ShouldReturnSameValue(string? value)
    {
        // Act
        var hash = _encryptionService.ComputeHash(value!);

        // Assert
        hash.Should().Be(value);
    }

    #endregion

    #region Roundtrip Tests

    [Theory]
    [InlineData("4111111111111111")]
    [InlineData("5500000000000004")]
    [InlineData("378282246310005")]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("12/25")]
    [InlineData("JUAN PEREZ GARCIA")]
    public void EncryptDecrypt_RoundTrip_ShouldReturnOriginalValue(string originalValue)
    {
        // Act
        var encrypted = _encryptionService.Encrypt(originalValue);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(originalValue);
    }

    [Fact]
    public void EncryptDecrypt_MultipleCreditCardFields_ShouldPreserveAllData()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var cvv = "123";
        var expirationDate = "12/25";

        // Act
        var encryptedCardNumber = _encryptionService.Encrypt(cardNumber);
        var encryptedCvv = _encryptionService.Encrypt(cvv);
        var encryptedExpiration = _encryptionService.Encrypt(expirationDate);

        var decryptedCardNumber = _encryptionService.Decrypt(encryptedCardNumber);
        var decryptedCvv = _encryptionService.Decrypt(encryptedCvv);
        var decryptedExpiration = _encryptionService.Decrypt(encryptedExpiration);

        // Assert
        decryptedCardNumber.Should().Be(cardNumber);
        decryptedCvv.Should().Be(cvv);
        decryptedExpiration.Should().Be(expirationDate);
    }

    #endregion

    #region Key Validation Tests

    [Fact]
    public void Constructor_WithInvalidEncryptionKeySize_ShouldThrowException()
    {
        // Arrange
        var invalidKey = new byte[16]; // Should be 32

        // Act
        var act = () => new AesGcmEncryptionService(invalidKey, _hmacKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Encryption key must be 32 bytes*");
    }

    [Fact]
    public void Constructor_WithInvalidHmacKeySize_ShouldThrowException()
    {
        // Arrange
        var invalidKey = new byte[16]; // Should be 32

        // Act
        var act = () => new AesGcmEncryptionService(_encryptionKey, invalidKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*HMAC key must be 32 bytes*");
    }

    #endregion

    #region Cross-Service Tests

    [Fact]
    public void DifferentServices_WithSameKeys_ShouldDecryptEachOther()
    {
        // Arrange
        var service1 = new AesGcmEncryptionService(_encryptionKey, _hmacKey);
        var service2 = new AesGcmEncryptionService(_encryptionKey, _hmacKey);
        var plainText = "4111111111111111";

        // Act
        var encrypted = service1.Encrypt(plainText);
        var decrypted = service2.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void DifferentServices_WithDifferentKeys_ShouldNotDecryptEachOther()
    {
        // Arrange
        var differentEncryptionKey = new byte[32];
        RandomNumberGenerator.Fill(differentEncryptionKey);
        
        var service1 = new AesGcmEncryptionService(_encryptionKey, _hmacKey);
        var service2 = new AesGcmEncryptionService(differentEncryptionKey, _hmacKey);
        var plainText = "4111111111111111";

        // Act
        var encrypted = service1.Encrypt(plainText);
        var act = () => service2.Decrypt(encrypted);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void DifferentServices_WithSameHmacKey_ShouldProduceSameHash()
    {
        // Arrange
        var differentEncryptionKey = new byte[32];
        RandomNumberGenerator.Fill(differentEncryptionKey);
        
        var service1 = new AesGcmEncryptionService(_encryptionKey, _hmacKey);
        var service2 = new AesGcmEncryptionService(differentEncryptionKey, _hmacKey);
        var value = "4111111111111111";

        // Act
        var hash1 = service1.ComputeHash(value);
        var hash2 = service2.ComputeHash(value);

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion
}
