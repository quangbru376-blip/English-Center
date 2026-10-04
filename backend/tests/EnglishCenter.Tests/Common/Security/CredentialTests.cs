namespace EnglishCenter.Tests.Common.Security;

using System.Buffers.Binary;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Xunit;

public class CredentialTests
{
    private static PasswordPolicy Policy()=>new(OfflinePasswordBlocklist.Load(OfflinePasswordBlocklistTests.SourceDirectory));
    [Fact]
    public void Hash_UsesSaltedIdentityV3Sha512WithExplicitWorkFactor()
    {
        var hasher=new CredentialHasher(Policy());
        const string raw="A distinctive password phrase 82735";
        var first=hasher.Hash(raw);var second=hasher.Hash(raw);
        Assert.NotEqual(first,second);
        Assert.Equal(84,first.Length);
        Assert.True(first.Length<=512);
        var bytes=Convert.FromBase64String(first);
        Assert.Equal(61,bytes.Length);
        Assert.Equal(1,bytes[0]);
        Assert.Equal(2u,BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(1,4)));
        Assert.Equal(220000u,BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(5,4)));
        Assert.Equal(16u,BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(9,4)));
        Assert.Equal(PasswordVerificationResult.Success,hasher.Verify(first,raw));
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(first,raw+"x"));
    }
    [Theory]
    [InlineData("  A distinctive phrase 82735  ","A distinctive phrase 82735")]
    [InlineData("A distinctive phrase 82735","a distinctive phrase 82735")]
    [InlineData("é distinctive phrase 82735","é distinctive phrase 82735")]
    public void HashVerification_PreservesRawSpacesCaseAndUnicode(string raw,string altered)
    {
        var hasher=new CredentialHasher(Policy());
        var hash=hasher.Hash(raw);
        const string alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var unusedBits=hash[..^3]+alphabet[alphabet.IndexOf(hash[^3])+1]+"==";
        Assert.Equal(Convert.FromBase64String(hash),Convert.FromBase64String(unusedBits));
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(unusedBits,raw));
        Assert.Equal(PasswordVerificationResult.Success,hasher.Verify(hash,raw));
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(hash,altered));
    }
    [Fact]
    public void OldLowerV3Factor_RehashNeededWithoutApplyingNewPolicyToExistingPassword()
    {
        var raw=File.ReadLines(Path.Combine(OfflinePasswordBlocklistTests.SourceDirectory,"10k-most-common.txt")).First(s=>s.Length>=15);
        Assert.Throws<ApiErrorException>(()=>Policy().Validate(raw));
        var old=new PasswordHasher<object>(Options.Create(new PasswordHasherOptions{CompatibilityMode=PasswordHasherCompatibilityMode.IdentityV3,IterationCount=120000}));
        var oldHash=old.HashPassword(new object(),raw);
        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded,new CredentialHasher(Policy()).Verify(oldHash,raw));
    }
    [Theory]
    [InlineData(null),InlineData(""),InlineData("not base64"),InlineData("AAAA"),InlineData("AA==")]
    public void MalformedStoredHashes_FailSafely(string? hash) => Assert.Equal(PasswordVerificationResult.Failed,new CredentialHasher(Policy()).Verify(hash,"A distinctive phrase 82735"));
    [Fact]
    public void NoncanonicalAndExpensiveHeaders_AreRejected()
    {
        var hasher=new CredentialHasher(Policy());
        const string raw="A distinctive phrase 82735";
        var hash=hasher.Hash(raw);
        foreach(var value in new[]{" "+hash,hash+"\n",hash.TrimEnd('='),hash[..^2]+"B="})
            Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(value,raw));
        foreach(var part in new[]{0,1,5,9})
        {
            var bytes=Convert.FromBase64String(hash);
            if(part==0)bytes[0]=0;
            else BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(part,4),part==5?uint.MaxValue:99u);
            Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(Convert.ToBase64String(bytes),raw));
        }
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(hash,null));
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(hash,new string('x',129)));
        Assert.Equal(PasswordVerificationResult.Failed,hasher.Verify(hash,"invalid phrase "+(char)0xd800));
        Assert.Throws<ApiErrorException>(()=>hasher.Hash("short"));
        Assert.Throws<ApiErrorException>(()=>hasher.Hash("invalid phrase "+(char)0xd800));
    }
    [Fact]
    public void GeneratedTemporaryPasswords_Are32AsciiAndPolicyValid()
    {
        var policy=Policy();var generator=new TemporaryPasswordGenerator(policy);
        var samples=new HashSet<string>(StringComparer.Ordinal);
        for(var i=0;i<100;i++)
        {
            var value=generator.Generate();
            Assert.Equal(32,value.Length);
            Assert.All(value,c=>Assert.True(char.IsAsciiLetterOrDigit(c)||c is '-' or '_'));
            Assert.Same(value,policy.Validate(value));
            Assert.True(samples.Add(value));
        }
    }
    [Fact]
    public void NullDependencies_AreProgrammerErrors()
    {
        Assert.Throws<ArgumentNullException>(()=>new CredentialHasher(null!));
        Assert.Throws<ArgumentNullException>(()=>new TemporaryPasswordGenerator(null!));
    }

    [Fact]
    public void HashAndVerify_Allow128SupplementaryCodePoints()
    {
        var raw=string.Concat(Enumerable.Repeat("😀",128));
        var hasher=new CredentialHasher(Policy());
        Assert.Equal(PasswordVerificationResult.Success,hasher.Verify(hasher.Hash(raw),raw));
    }
}
