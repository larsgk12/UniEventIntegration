namespace UniEventIntegration.Models;

[BizEntity("biz/custom-fields")]
[JsonConverter(typeof(BizEntityConverterFactory))]
public partial record CustomField : IBizEntity
{
    public int ModelID { get; init; }
    public string? Name { get; init; }
    public Guid? UpdatedBy { get; init; }
    public int? StatusCode { get; init; }
    public DateTime? CreatedAt { get; init; }
    public bool Deleted { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid DataType { get; init; }
    public Guid? CreatedBy { get; init; }
    public bool Nullable { get; init; }
    public int ID { get; init; }
}
