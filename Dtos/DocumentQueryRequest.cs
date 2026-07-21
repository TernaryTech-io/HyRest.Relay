using System.Text.Json.Serialization;
using Ternary.HyRest;
using Ternary.HyRest.DocumentManagement;

namespace HyRest.Relay;

public class DocumentQueryResponse
{
    public List<DocumentQueryResponseItem> Items { get; set; } = [];
}
public class DocumentQueryResponseItem
{
    public string Id { get; set; }
    public List<QueryDisplayColumn> DisplayColumns { get; set; } = [];

}

public class DocumentQueryRequest
{
    [HyRestConverter<JsonStringEnumConverter>]
    public QueryType Type { get; set; }
    /// <summary>
    /// The document type name/id, document type group name/id, custom query name/id
    /// </summary>
    public string Item { get; set; }
    public List<QueryKeywordModel> QueryKeywords { get; set; } = [];
}

public class QueryKeywordModel
{
    public string Keyword { get; set; }
    public string Value { get; set; }
    /// <summary>
    /// Represents the operator for the keyword value of
    /// <br/>this query keyword. Defaults to Equal if not present.
    /// </summary>
    [HyRestConverter<JsonStringEnumConverter>]
    public QueryKeywordOperator Operator { get; set; }

    /// <summary>
    /// Represents the relation of this query keyword to
    /// <br/>other query keywords. Defaults to And if not present.
    /// </summary>
    [HyRestConverter<JsonStringEnumConverter>]
    public QueryKeywordRelation Relation { get; set; }
}