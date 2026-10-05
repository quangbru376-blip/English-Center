namespace EnglishCenter.Api.Domain.Academics;

public sealed class Course
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];
}
