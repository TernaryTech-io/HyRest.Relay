using Ternary.HyRest;
using Ternary.HyRest.DocumentManagement;

namespace HyRest.Relay;

public static class DocumentQueryHelpers
{
    public static DocumentQueryResponse ExecuteQuery(HylandApp app, DocumentQuery query)
    {
        var response = new DocumentQueryResponse();
        var results = query.GetResults();
        foreach(var result in results)
        {
            var item = new DocumentQueryResponseItem()
            {
                Id = result.DocumentId.ToString(),
                DisplayColumns = result.DisplayColumns.ToList()
            };
            response.Items.Add(item);
        }
        return response;
    }
    public static DocumentQuery ConstructQuery(HylandApp app, DocumentQueryRequest request)
    {
        if (request.Type == QueryType.CustomQuery)
            return ConstructCustomQuery(app, request);
        else if (request.Type == QueryType.DocumentType)
            return ConstructDocumentTypeQuery(app, request);
        else return ConstructDocumentTypeGroupQuery(app, request);
    }
    private static DocumentQuery ConstructCustomQuery(HylandApp app, DocumentQueryRequest request)
    {
        var cq = app.Core.CustomQueries[request.Item];
        var builder = app.Core.CreateDocumentQueryBuilder<CustomQueryBuilder>()
            .AddItem(cq);
        foreach(var queryKey in request.QueryKeywords)
        {
            builder.AddQueryKeyword(key =>
            {
                key.Id = queryKey.Keyword;
                key.Value = queryKey.Value;
                key.Operator = queryKey.Operator;
                key.Relation = queryKey.Relation;
            });
        }
        return builder.CreateQuery();
    }
    private static DocumentQuery ConstructDocumentTypeQuery(HylandApp app, DocumentQueryRequest request)
    {
        var dt = app.Core.DocumentTypes[request.Item];
        var builder = app.Core.CreateDocumentQueryBuilder<DocumentTypeQueryBuilder>()
            .AddItem(dt);
        foreach (var queryKey in request.QueryKeywords)
        {
            builder.AddQueryKeyword(key =>
            {
                key.Id = queryKey.Keyword;
                key.Value = queryKey.Value;
                key.Operator = queryKey.Operator;
                key.Relation = queryKey.Relation;
            });
        }
        return builder.CreateQuery();
    }
    private static DocumentQuery ConstructDocumentTypeGroupQuery(HylandApp app, DocumentQueryRequest request)
    {
        var dt = app.Core.DocumentTypeGroups[request.Item];
        var builder = app.Core.CreateDocumentQueryBuilder<DocumentTypeGroupQueryBuilder>()
            .AddItem(dt);
        foreach (var queryKey in request.QueryKeywords)
        {
            builder.AddQueryKeyword(key =>
            {
                key.Id = queryKey.Keyword;
                key.Value = queryKey.Value;
                key.Operator = queryKey.Operator;
                key.Relation = queryKey.Relation;
            });
        }
        return builder.CreateQuery();
    }
}
