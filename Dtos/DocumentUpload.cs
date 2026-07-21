
using System.Security;
using Ternary.HyRest.DocumentManagement;

namespace HyRest.Relay;


public class DocumentUpdateResponse
{
    public Document? Document { get; set; }
    public List<string> KeywordExceptions { get; set; } = [];
}

public class DocumentUpdateModel
{
    public string DocumentId { get; set; }
    public SAKeywordUploadModel StandAloneKeywords { get; set; } = [];
    public List<KeywordGroupUploadModel> KeywordGroups { get; set; } = [];
}

public class DocumentUploadResponse
{
    public Document? Document { get; set; }
    public List<string> KeywordExceptions { get; set; } = [];
}

public class DocumentUploadModel
{
    public string DocumentType { get; set; }
    public string FileExtension { get; set; }
    public DateTime DocumentDate { get; set; }
    public bool StoreAsNew { get; set; }
    public string? RevisionComment { get; set; }
    public byte[] Bytes { get; set; } = [];
    public SAKeywordUploadModel StandAloneKeywords { get; set; } = [];
    public List<KeywordGroupUploadModel> KeywordGroups { get; set; } = [];
}


public class SAKeywordUploadModel : Dictionary<string, object[]>
{
    public SAKeywordUploadModel() : base(StringComparer.InvariantCultureIgnoreCase) { }
}

public class KGKeywordUploadModel : Dictionary<string, object>
{
    public KGKeywordUploadModel() : base(StringComparer.InvariantCultureIgnoreCase) { }
}

public class KeywordGroupUploadModel
{
    public required string KeywordGroup { get; set; }
    public KGKeywordUploadModel Keywords { get; set; } = [];
}