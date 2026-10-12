namespace EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;
public sealed record AuditViewerQuery(DateOnly? From,DateOnly? To,int? Actor,string? Action,string? Entity,string? Reference,string? Result,int Page,int Size)
{
    public static AuditViewerQuery Parse(IQueryCollection query,bool operation=false)
    {
        var q=new AcademicQuery(query,operation?["page","pageSize"]:["fromDate","toDate","actorId","action","entityType","entityId","result","page","pageSize"]);
        var from=q.Date("fromDate");var to=q.Date("toDate");if(from>to)throw ApiErrorException.Validation("toDate");
        var action=q.Get("action");var entity=q.Get("entityType");var reference=q.Get("entityId");var result=q.Get("result");
        if(action is not null&&!AuditViewerPolicy.Action(action))throw ApiErrorException.Validation("action");
        if(entity is not null&&!AuditViewerPolicy.Entity(entity))throw ApiErrorException.Validation("entityType");
        if(action is not null&&entity is not null&&!AuditViewerPolicy.Accepts(action,entity))throw ApiErrorException.Validation("entityType");
        if(!AuditViewerPolicy.Reference(reference))throw ApiErrorException.Validation("entityId");
        if(result is not null&&result is not ("SUCCESS" or "FAILURE"))throw ApiErrorException.Validation("result");
        return new(from,to,q.Id("actorId"),action,entity,reference,result,q.Number("page"),q.Number("pageSize",20,100));
    }
}
