using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ternary.HyRest;

namespace HyRest.Relay;

public static class EndPointLoaders
{
    public static WebApplication AddEndpoints(this WebApplication web)
    {        
        var app = web.Services.GetService<HylandApp>();
        web.MapItemTypeEndPoints(app);
        web.MapDocumentEndPoints(app);
        web.MapArchiveEndpoints(app);
        web.MapQueryEndpoints(app);
        return web;
    }

    internal static WebApplication MapItemTypeEndPoints(this WebApplication web, HylandApp app)
    {
        web.MapGet("/documenttypes", [Authorize] ([FromQuery] string? query) =>
        {
            if (query == null)
                return app.Core.DocumentTypes.ToArray();
            else
                return [app.Core.DocumentTypes[query]];
        }).WithName("GetDocumentTypes");
        web.MapGet("/documenttypes/{id}", [Authorize] ([FromRoute] string id) =>
        {
            return app.Core.DocumentTypes[id];
        }).WithName("GetDocumentTypeById");
        web.MapGet("/documenttypes/{id}/keywordtypes", [Authorize] ([FromRoute] string id) =>
        {
            return app.Core.DocumentTypes[id]?.KeywordTypeCollection;
        }).WithName("GetKeywordTypesByDocumentTypeId");
        web.MapGet("/documenttypegroups", [Authorize] ([FromQuery] string? query) => {         
            if (query == null)
                return app.Core.DocumentTypeGroups.ToArray();
            else
                return [app.Core.DocumentTypeGroups[query]];
        }).WithName("GetDocumentTypeGroups");
        web.MapGet("/documenttypegroups/{id}", [Authorize] ([FromRoute] string id) => {            
            return app.Core.DocumentTypeGroups[id];         
        }).WithName("GetDocumentTypeGroupById");
        web.MapGet("/keywordtypes", [Authorize]([FromQuery] string? query) =>
        {            
            if (query == null)
                return app.Core.KeywordTypes.ToArray();
            else
                return [app.Core.KeywordTypes[query]];
        }).WithName("GetKeywordTypes");
        web.MapGet("/keywordtypes/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            return app.Core.KeywordTypes[id];
        }).WithName("GetKeywordTypeById");
        web.MapGet("/keywordtypegroups", [Authorize] ([FromQuery] string? query) =>
        {            
            if (query == null)
                return app.Core.KeywordTypeGroups.ToArray();
            else
                return [app.Core.KeywordTypeGroups[query]];
        }).WithName("GetKeywordTypeGroups");
        web.MapGet("/keywordtypegroups/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            return app.Core.KeywordTypeGroups[id];
        }).WithName("GetKeywordTypeGroupById");
        web.MapGet("/filetypes", [Authorize] ([FromQuery] string? query) =>
        {            
            if (query == null)
                return app.Core.FileTypes.ToArray();
            else
                return [app.Core.FileTypes[query]];
        }).WithName("GetFileTypes");
        web.MapGet("/filetypes/bestguess", [Authorize] ([FromQuery] string extension) =>
        {            
            return app.Core.FileTypes.BestGuess(extension);
        }).WithName("GetFileTypesBestGuess");
        web.MapGet("/filetypes/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            return app.Core.FileTypes[id];
        }).WithName("GetFileTypeById");
        web.MapGet("/customqueries", [Authorize] ([FromQuery] string? query) =>
        {
            if (query == null)
                return app.Core.CustomQueries.ToArray();
            else
                return [app.Core.CustomQueries[query]];
        }).WithName("GetCustomQueries");
        web.MapGet("/customqueries/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            return app.Core.CustomQueries[id];
        }).WithName("GetCustomQueryById");
        web.MapGet("/notetypes", [Authorize] ([FromQuery] string? query) =>
        {
            if (query == null)
                return app.Core.NoteTypes.ToArray();
            else
                return [app.Core.NoteTypes[query]];
        }).WithName("GetNoteTypes");
        web.MapGet("/notetypes/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            return app.Core.NoteTypes[id];
        }).WithName("GetNoteTypeById");
        return web;
    }

    internal static WebApplication MapDocumentEndPoints(this WebApplication web, HylandApp app)
    {
        web.MapGet("/document/{id}", [Authorize] ([FromRoute] string id) =>
        {            
            var doc = app.Core.GetDocumentById(id);
            return doc;
        }).WithName("GetDocumentById");
        web.MapGet("/document/{id}/keywords", [Authorize] ([FromRoute] string id) =>
        {           
            var doc = app.Core.GetDocumentById(id);
            return doc?.KeywordCollection;
        }).WithName("GetDocumentKeywords");
        web.MapGet("/document/{id}/content", [Authorize] ([FromRoute] string id, [FromQuery] string? revision, [FromQuery] string? rendition, [FromHeader] string? accept) =>
        {            
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
        web.MapGet("/document/{id}/notes", [Authorize] ([FromRoute] string id, [FromQuery] string? revision, [FromQuery] string? rendition, [FromHeader] string? accept) =>
        {            
            var doc = app.Core.GetDocumentById(id);
            return doc?.Notes;
        }).WithName("GetDocumentNotes");
        return web;
    }

    internal static WebApplication MapArchiveEndpoints(this WebApplication web, HylandApp app)
    {
        web.MapGet("/documenttypes/{id}/archive", [Authorize] async ([FromRoute] string id) =>
        {
            return await DocumentArchiveHelpers.CreateUploadModel(app, id);
        }).WithName("GetDocumentTypeArchive");

        web.MapPost("/document", [Authorize] async ([FromBody] DocumentUploadModel model) =>
        {
            return await DocumentArchiveHelpers.ArchiveDocument(app, model);
            
        }).WithName("ArchiveDocument");
        web.MapGet("/document/{id}/update", [Authorize] async ([FromRoute] string id) =>
        {
            return await DocumentArchiveHelpers.EditDocument(app, id);
        }).WithName("EditDocument");
        web.MapPut("/document", [Authorize] async ([FromBody] DocumentUpdateModel model) =>
        {
            return await DocumentArchiveHelpers.UpdateDocument(app, model);
        }).WithName("UpdateDocument");

        return web;
    }

    internal static WebApplication MapQueryEndpoints(this WebApplication web, HylandApp app)
    {
        web.MapPost("/query", [Authorize] (DocumentQueryRequest request) =>
        {
            var query = DocumentQueryHelpers.ConstructQuery(app, request);
            return DocumentQueryHelpers.ExecuteQuery(app, query);
        }).WithName("ExecuteQuery");
        
        return web;
    }

}
