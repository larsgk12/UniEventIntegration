namespace UniEventIntegration.Models;

[BizEntity("biz/valueitems")]
[JsonConverter(typeof(BizEntityConverterFactory))]
public partial record ValueItem : IBizEntity
{
    public Guid Code { get; init; }
    public int ID { get; init; }
    public bool Deleted { get; init; }
    public Guid? UpdatedBy { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? Value { get; init; }
    public int ValueListID { get; init; }
    public int Index { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}
