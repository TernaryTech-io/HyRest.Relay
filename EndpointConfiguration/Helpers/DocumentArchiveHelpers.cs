using Ternary.HyRest;
using Ternary.HyRest.DocumentManagement;

namespace HyRest.Relay;

public static class DocumentArchiveHelpers
{
    public static async Task<DocumentUpdateResponse> UpdateDocument(HylandApp app, DocumentUpdateModel model)
    {
        var doc = await app.Core.GetDocumentByIdAsync(model.DocumentId);
        if (doc == null)
            return null;        
        List<Exception> exceptions = [];
        foreach (var sak in model.StandAloneKeywords)
        {

            var keyword = doc.KeywordCollection.CreateEditableKeyword(sak.Key);
            if (sak.Value != null && sak.Value.Count() > 0)
            {
                foreach (var value in sak.Value)
                {
                    if (value != null)
                    {
                        keyword.TryAdd(value.ToString(), out Exception? ex);
                        if (ex != null)
                            exceptions.Add(ex);
                    }
                }
            }
        }
        foreach (var group in model.KeywordGroups)
        {
            var groupType = app.Core.KeywordTypeGroups.Find(group.KeywordGroup);
            if (groupType.StorageType == KeywordTypeGroupType.SingleInstance)
            {
                var record = doc.KeywordCollection.CreateEditableSingleInstanceRecord(group.KeywordGroup);
                foreach (var keyword in group.Keywords)
                {
                    if (keyword.Value != null)
                    {
                        record.CreateEditableKeyword(keyword.Key)
                            .TryAdd(keyword.Value, out Exception? ex);
                        if (ex != null)
                            exceptions.Add(ex);
                    }
                }
            }
            else
            {
                var record = doc.KeywordCollection.CreateEditableMultiInstanceRecord(group.KeywordGroup);
                foreach (var keyword in group.Keywords)
                {
                    record.CreateEditableKeyword(keyword.Key)
                            .TryAdd(keyword.Value, out Exception? ex);
                    if (ex != null)
                        exceptions.Add(ex);
                }
            }
        }
        await doc.UpdateKeywords();
        doc = await app.Core.GetDocumentByIdAsync(model.DocumentId);
        return new DocumentUpdateResponse
        {
            Document = doc,
            KeywordExceptions = exceptions.Select(e => e.Message).ToList()
        };
    }
    public static async Task<DocumentUpdateModel?> EditDocument(HylandApp app, string id)
    {
        var doc = await app.Core.GetDocumentByIdAsync(id);
        if (doc == null)
            return null;
        var collection = doc.KeywordCollection;
        var update = new DocumentUpdateModel
        {
            DocumentId = doc.Id.ToString()
        };
        foreach (var key in collection.StandAloneKeywords.ToList())
        {
            List<object?> valArray = [];
            key.Values
                .ToList()
                .ForEach(v => valArray.Add(v.Value));
            update.StandAloneKeywords.Add(key.Name, valArray.ToArray());
        }
        foreach (var kwg in collection.SingleInstanceGroups.ToList())
        {
            var model = new KeywordGroupUploadModel
            {
                KeywordGroup = kwg.Name ?? kwg.Id.ToString()
            };
            foreach (var key in kwg.Keywords)
            {
                model.Keywords.Add(key.Name, key.Value());
            }
            update.KeywordGroups.Add(model);
        }
        foreach (var sorted in collection.MultiInstanceGroups.GroupCollection)
        {
            foreach (var kwg in sorted.ToList())
            {
                var model = new KeywordGroupUploadModel
                {
                    KeywordGroup = kwg.Name ?? kwg.Id.ToString()
                };
                foreach (var key in kwg.Keywords)
                {
                    model.Keywords.Add(key.Name, key.Value());
                }
                update.KeywordGroups.Add(model);
            }
        }
        return update;
    }
    public static async Task<DocumentUploadResponse> ArchiveDocument(HylandApp app, DocumentUploadModel model)
    {
        var docType = app.Core.DocumentTypes.Find(model.DocumentType);
        if (docType == null)
            throw new Exception("Could not find document type.");
        var props = docType
        .CreateNewDocumentArchiveProperties()
        .WithBytes(model.Bytes, model.FileExtension);
        props.DocumentDate = model.DocumentDate;
        props.StoreAsNew = model.StoreAsNew;
        props.Comment = model.RevisionComment ?? string.Empty;
        List<Exception> exceptions = [];
        foreach (var sak in model.StandAloneKeywords)
        {

            var keyword = props.KeywordCollection.CreateEditableKeyword(sak.Key);
            if (sak.Value != null && sak.Value.Count() > 0)
            {
                foreach (var value in sak.Value)
                {
                    if (value != null)
                    {
                        keyword.TryAdd(value, out Exception? ex);
                        if (ex != null)
                            exceptions.Add(ex);
                    }
                }
            }
        }
        foreach (var group in model.KeywordGroups)
        {
            var groupType = app.Core.KeywordTypeGroups.Find(group.KeywordGroup);
            if (groupType.StorageType == KeywordTypeGroupType.SingleInstance)
            {
                var record = props.KeywordCollection.CreateEditableSingleInstanceRecord(group.KeywordGroup);
                foreach (var keyword in group.Keywords)
                {
                    if (keyword.Value != null)
                    {
                        record.CreateEditableKeyword(keyword.Key)
                            .TryAdd(keyword.Value, out Exception? ex);
                        if (ex != null)
                            exceptions.Add(ex);
                    }
                }
            }
            else
            {
                var record = props.KeywordCollection.CreateEditableMultiInstanceRecord(group.KeywordGroup);
                foreach (var keyword in group.Keywords)
                {
                    record.CreateEditableKeyword(keyword.Key)
                            .TryAdd(keyword.Value, out Exception? ex);
                    if (ex != null)
                        exceptions.Add(ex);
                }
            }
        }
        var document = await props.ArchiveDocument();

        return new DocumentUploadResponse
        {
            Document = document,
            KeywordExceptions = exceptions.Select(e => e.Message).ToList()
        };
    }    
    public static async Task<DocumentUploadModel?> CreateUploadModel(HylandApp app, string id)
    {
        var doctType = app.Core.DocumentTypes[id];
        if (doctType == null)
            return null;

        var defaultKeys = await doctType.GetDefaultKeywords();
        var archive = new DocumentUploadModel
        {
            DocumentType = doctType.Name,
            FileExtension = "???"
        };
        foreach (var key in defaultKeys.StandAloneKeywords.ToList())
        {
            List<object?> valArray = [];
            key.Values
                .ToList()
                .ForEach(v => valArray.Add(v.Value));
            archive.StandAloneKeywords.Add(key.Name, valArray.ToArray());
        }
        foreach (var kwg in defaultKeys.SingleInstanceGroups.ToList())
        {
            var model = new KeywordGroupUploadModel
            {
                KeywordGroup = kwg.Name ?? kwg.Id.ToString()
            };
            foreach (var key in kwg.Keywords)
            {
                model.Keywords.Add(key.Name, key.Value());
            }
            archive.KeywordGroups.Add(model);
        }
        foreach (var sorted in defaultKeys.MultiInstanceGroups.GroupCollection)
        {
            foreach (var kwg in sorted.ToList())
            {
                var model = new KeywordGroupUploadModel
                {
                    KeywordGroup = kwg.Name ?? kwg.Id.ToString()
                };
                foreach (var key in kwg.Keywords)
                {
                    model.Keywords.Add(key.Name, key.Value());
                }
                archive.KeywordGroups.Add(model);
            }
        }

        return archive;
    }
}
