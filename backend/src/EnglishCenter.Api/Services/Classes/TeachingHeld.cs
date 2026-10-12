namespace EnglishCenter.Api.Services.Classes;
using System.Text.Json.Nodes;

public static class TeachingHeld
{
    public static bool CanConfirm(ClassState s,EnglishCenter.Api.Domain.Training.Schedule row,DateTime now)=>
        s.Class.Status=="ONGOING"&&row.IsCurrent&&row.OccurrenceState=="PLANNED"&&now>=ClassWire.Window(row).EndAt;
    public static JsonObject Manifest(ClassState s,EnglishCenter.Api.Domain.Training.Schedule row)
    {var m=ClassWire.BaseManifest(s);m["expectedVersions"]!["schedule"]=ClassWire.Rv(row.RowVersion);m["expectedSet"]!["targetScheduleId"]=row.ScheduleId;m["expectedSet"]!["obligationKey"]=row.ObligationKey;return m;}
}
