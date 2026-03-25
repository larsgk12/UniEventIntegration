namespace UniEventIntegration.Models;

[BizEntity("biz/valuelists")]
[JsonConverter(typeof(BizEntityConverterFactory))]
public partial record ValueList : IBizEntity
{
    public string? Code { get; init; }
    public int ID { get; init; }
    public bool Deleted { get; init; }
    public Guid? UpdatedBy { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }

    public IList<ValueItem>? Items { get; init; }
}
