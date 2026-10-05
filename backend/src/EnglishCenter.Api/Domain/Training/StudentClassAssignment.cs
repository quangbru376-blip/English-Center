namespace EnglishCenter.Api.Domain.Training;

using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;

public sealed class StudentClassAssignment
{
    public int ClassAssignmentId { get; set; }
    public int ClassId { get; set; }
    public int RegistrationId { get; set; }
    public int AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }

    public Class? Class { get; set; }
    public Registration? Registration { get; set; }
    public Account? AssignedByAccount { get; set; }
}
