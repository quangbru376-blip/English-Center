using EnglishCenter.Api.Common.Validation;
using Xunit;

namespace EnglishCenter.Tests.Common.Validation;

public class ContactNormalizerTests
{
    [Fact]
    public void NormalizeEmail_ValidEmail_ReturnsLowercasedAscii()
    {
        string input = "  User.Name+Tag@Example.COM  ";
        string expected = "user.name+tag@example.com";

        Assert.Equal(expected, ContactNormalizer.NormalizeEmail(input));
    }

    [Fact]
    public void NormalizeEmail_PreservesDotAndTagDistinctions()
    {
        string baseEmail = ContactNormalizer.NormalizeEmail("user@example.com");
        string dotEmail = ContactNormalizer.NormalizeEmail("u.ser@example.com");
        string tagEmail = ContactNormalizer.NormalizeEmail("user+tag@example.com");

        Assert.NotEqual(baseEmail, dotEmail);
        Assert.NotEqual(baseEmail, tagEmail);
        Assert.NotEqual(dotEmail, tagEmail);
    }

    [Fact]
    public void NormalizeEmail_CaseInsensitiveEquivalence()
    {
        string lower = ContactNormalizer.NormalizeEmail("student@example.com");
        string upper = ContactNormalizer.NormalizeEmail("STUDENT@EXAMPLE.COM");
        string mixed = ContactNormalizer.NormalizeEmail("Student@Example.Com");

        Assert.Equal(lower, upper);
        Assert.Equal(lower, mixed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("user.@example.com")]
    [InlineData("user@-example.com")]
    [InlineData("user@example-.com")]
    [InlineData("user@example.com(comment)")]
    [InlineData("user\0@example.com")]
    [InlineData("one@example.com,two@example.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@example@another.com")]
    [InlineData("John Doe <john@example.com>")]
    [InlineData("user1@example.com, user2@example.com")]
    [InlineData("user@example..com")]
    [InlineData("user name@example.com")]
    [InlineData("user\t@example.com")]
    [InlineData("user\n@example.com")]
    [InlineData("uśer@example.com")]
    [InlineData("user@víệt.com")]
    public void NormalizeEmail_InvalidInputs_ThrowsArgumentException(string? invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizeEmail(invalidEmail));
    }

    [Fact]
    public void NormalizeEmail_BoundaryLength254And255()
    {
        string local248 = new('a', 248);
        string email254 = local248 + "@b.com";
        Assert.Equal(254, email254.Length);
        Assert.Equal(email254, ContactNormalizer.NormalizeEmail(email254));

        string local249 = new('a', 249);
        string email255 = local249 + "@b.com";
        Assert.Equal(255, email255.Length);
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizeEmail(email255));
    }

    [Theory]
    [InlineData("0901234567", "0901234567")]
    [InlineData("0000000000", "0000000000")]
    [InlineData("+84901234567", "0901234567")]
    [InlineData("  090 123 4567  ", "0901234567")]
    [InlineData("  090-123-4567  ", "0901234567")]
    [InlineData("+84 901 234 567", "0901234567")]
    [InlineData("+84-901-234-567", "0901234567")]
    [InlineData("090 - 123 - 4567", "0901234567")]
    [InlineData("090\u00A0123\u00A04567", "0901234567")]
    public void NormalizePhone_Equivalence_ReturnsCanonicalZeroPrefix(string input, string expected)
    {
        Assert.Equal(expected, ContactNormalizer.NormalizePhone(input));
        Assert.Equal(expected, ContactNormalizer.NormalizeOptionalPhone(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizePhone_NullOrBlank_RequiredThrows_OptionalReturnsNull(string? blank)
    {
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizePhone(blank));
        Assert.Null(ContactNormalizer.NormalizeOptionalPhone(blank));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("--")]
    [InlineData(" - ")]
    [InlineData("---")]
    public void NormalizePhone_SeparatorsOnly_ThrowsArgumentExceptionInBoth(string separatorsOnly)
    {
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizePhone(separatorsOnly));
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizeOptionalPhone(separatorsOnly));
    }

    [Theory]
    [InlineData("090123456")]
    [InlineData("09012345678")]
    [InlineData("1901234567")]
    [InlineData("+85901234567")]
    [InlineData("+840901234567")]
    [InlineData("84901234567")]
    [InlineData("090.123.4567")]
    [InlineData("(090) 123 4567")]
    [InlineData("090123456a")]
    [InlineData("090\t1234567")]
    [InlineData("090\n1234567")]
    [InlineData("０９０１２３４５６７")]
    public void NormalizePhone_InvalidInputs_ThrowsArgumentException(string invalidPhone)
    {
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizePhone(invalidPhone));
        Assert.Throws<ArgumentException>(() => ContactNormalizer.NormalizeOptionalPhone(invalidPhone));
    }
}
