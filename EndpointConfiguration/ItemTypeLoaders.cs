using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ternary.HyRest;


namespace HyRest.Relay;

public static class EndPointLoaders
{
    public static WebApplication AddEndpoints(this WebApplication web)
    {        
       
        web.MapItemTypeEndPoints();
        web.MapDocumentEndPoints();
        web.MapArchiveEndpoints();
        web.MapQueryEndpoints();
        return web;
    }

    internal static WebApplication MapItemTypeEndPoints(this WebApplication web)
    {
        web.MapGet("/api/documenttypes", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();           
            if (query == null)
                return app.Core.DocumentTypes.ToArray();
            else
                return [app.Core.DocumentTypes[query]];
        }).WithName("GetDocumentTypes");
        web.MapGet("/api/documenttypes/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.DocumentTypes[id];
        }).WithName("GetDocumentTypeById");
        web.MapGet("/api/documenttypes/{id}/keywordtypes", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.DocumentTypes[id]?.KeywordTypeCollection;
        }).WithName("GetKeywordTypesByDocumentTypeId");
        web.MapGet("/api/documenttypegroups", [Authorize] async ([FromQuery] string? query, HttpContext context) => {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.DocumentTypeGroups.ToArray();
            else
                return [app.Core.DocumentTypeGroups[query]];
        }).WithName("GetDocumentTypeGroups");
        web.MapGet("/api/documenttypegroups/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) => {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.DocumentTypeGroups[id];         
        }).WithName("GetDocumentTypeGroupById");
        web.MapGet("/api/keywordtypes", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.KeywordTypes.ToArray();
            else
                return [app.Core.KeywordTypes[query]];
        }).WithName("GetKeywordTypes");
        web.MapGet("/api/keywordtypes/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.KeywordTypes[id];
        }).WithName("GetKeywordTypeById");
        web.MapGet("/api/keywordtypegroups", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.KeywordTypeGroups.ToArray();
            else
                return [app.Core.KeywordTypeGroups[query]];
        }).WithName("GetKeywordTypeGroups");
        web.MapGet("/api/keywordtypegroups/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.KeywordTypeGroups[id];
        }).WithName("GetKeywordTypeGroupById");
        web.MapGet("/api/filetypes", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.FileTypes.ToArray();
            else
                return [app.Core.FileTypes[query]];
        }).WithName("GetFileTypes");
        web.MapGet("/api/filetypes/bestguess", [Authorize] async ([FromQuery] string extension, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.FileTypes.BestGuess(extension);
        }).WithName("GetFileTypesBestGuess");
        web.MapGet("/api/filetypes/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.FileTypes[id];
        }).WithName("GetFileTypeById");
        web.MapGet("/api/customqueries", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.CustomQueries.ToArray();
            else
                return [app.Core.CustomQueries[query]];
        }).WithName("GetCustomQueries");
        web.MapGet("/api/customqueries/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.CustomQueries[id];
        }).WithName("GetCustomQueryById");
        web.MapGet("/api/notetypes", [Authorize] async ([FromQuery] string? query, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            if (query == null)
                return app.Core.NoteTypes.ToArray();
            else
                return [app.Core.NoteTypes[query]];
        }).WithName("GetNoteTypes");
        web.MapGet("/api/notetypes/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return app.Core.NoteTypes[id];
        }).WithName("GetNoteTypeById");
        return web;
    }

    internal static WebApplication MapDocumentEndPoints(this WebApplication web)
    {
        web.MapGet("/api/document/{id}", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            var doc = app.Core.GetDocumentById(id);
            return doc;
        }).WithName("GetDocumentById");
        web.MapGet("/api/document/{id}/keywords", [Authorize] async ([FromRoute] string id, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            var doc = app.Core.GetDocumentById(id);
            return doc?.KeywordCollection;
        }).WithName("GetDocumentKeywords");
        web.MapGet("/api/document/{id}/content", [Authorize] async ([FromRoute] string id, [FromQuery] string? revision, 
            [FromQuery] string? rendition, [FromHeader] string? accept, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            var doc = app.Core.GetDocumentById(id);
            var content = doc?.GetContent(revisionId: revision ?? "latest", fileTypeId: rendition ?? "default", accept: accept ?? "*/*");

            if (content != null && content.IsSuccessful && content.Content != null)
                return TypedResults.File(
                    fileStream: content.Content,
                    contentType: content.MimeType,
                    fileDownloadName: $"download-file.{content.Extension}"
                    );
            else return null;

        }).WithName("GetDocumentContent");
        web.MapGet("/api/document/{id}/notes", [Authorize] async ([FromRoute] string id, [FromQuery] string? revision, 
            [FromQuery] string? rendition, [FromHeader] string? accept, HttpContext context) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>(); 
            var doc = app.Core.GetDocumentById(id);
            return doc?.Notes;
        }).WithName("GetDocumentNotes");
        return web;
    }

    internal static WebApplication MapArchiveEndpoints(this WebApplication web)
    {
        web.MapGet("/api/documenttypes/{id}/archive", [Authorize] async ([FromRoute] string id) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return await DocumentArchiveHelpers.CreateUploadModel(app, id);
        }).WithName("GetDocumentTypeArchive");

        web.MapPost("/api/document", [Authorize] async ([FromBody] DocumentUploadModel model) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return await DocumentArchiveHelpers.ArchiveDocument(app, model);
            
        }).WithName("ArchiveDocument");
        web.MapGet("/api/document/{id}/update", [Authorize] async ([FromRoute] string id) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return await DocumentArchiveHelpers.EditDocument(app, id);
        }).WithName("EditDocument");
        web.MapPut("/api/document", [Authorize] async ([FromBody] DocumentUpdateModel model) =>
        {
            var app = web.Services.GetRequiredService<OnBaseApp>();
            return await DocumentArchiveHelpers.UpdateDocument(app, model);
        }).WithName("UpdateDocument");

        return web;
    }

    internal static WebApplication MapQueryEndpoints(this WebApplication web)
    {
        var app = web.Services.GetRequiredService<OnBaseApp>();
        web.MapPost("/api/query", [Authorize] (DocumentQueryRequest request) =>
        {
            var query = DocumentQueryHelpers.ConstructQuery(app, request);
            return DocumentQueryHelpers.ExecuteQuery(app, query);
        }).WithName("ExecuteQuery");

        return web;
    }
    
}

