using UniEventIntegration.Models;

namespace UniEventIntegration.UnimicroPlatform;


[JsonSerializable(typeof(CustomField))]
[JsonSerializable(typeof(IDPayload))]
[JsonSerializable(typeof(ICollection<IDPayload>))]
[JsonSerializable(typeof(ValueList))]
[JsonSerializable(typeof(ValueItem))]
[JsonSerializable(typeof(ICollection<ValueList>))]
internal sealed partial class ApiJsonCtx : JsonSerializerContext { }

