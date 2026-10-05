namespace EnglishCenter.Api.Domain.Academics;

public sealed class Band
{
    public int BandId { get; set; }

    public string BandCode { get; set; } = string.Empty;

    public decimal MinScore { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
