using UniEventIntegration.Models;

namespace UniEventIntegration.UnimicroPlatform;

public static class HttpClientBizExtensions
{
    public static async Task<TEntity?> BizGetOne<TEntity>(this HttpClient httpClient, int entityId, params string[] expands)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>() 
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var bizRoute = new StringBuilder();
        bizRoute.Append(bizRouteAttr.Route);
        bizRoute.Append('/');
        bizRoute.Append(entityId);
        if (expands.Length > 0)
        {
            bizRoute.Append("?expand=");
            bizRoute.Append(string.Join(',', expands));
        }

        var resp = await httpClient.GetAsync(bizRoute.ToString());

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank
                ? default
                : JsonSerializer.Deserialize<TEntity?>(json)
            : ErrorHandler<TEntity?>(resp.StatusCode, json, isBlank);
    }

    public static async Task<IList<TEntity>> BizGetMany<TEntity>(this HttpClient httpClient, string filter, params string[] expands)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var bizRoute = new StringBuilder();
        bizRoute.Append(bizRouteAttr.Route);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            bizRoute.Append("?filter=");
            bizRoute.Append(HttpUtility.UrlEncode(filter));
        }
        if (expands.Length > 0)
        {
            bizRoute.Append(string.IsNullOrWhiteSpace(filter) ? '?' : '&');
            bizRoute.Append("expand=");
            bizRoute.Append(string.Join(',', expands));
        }
        var fullUrl = bizRoute.ToString();
        var resp = await httpClient.GetAsync(fullUrl);

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank
                ? []
                : JsonSerializer.Deserialize<IList<TEntity>>(json) ?? []
            : ErrorHandler<IList<TEntity>>(resp.StatusCode, json, isBlank);
    }

    public static async Task<TEntity?> BizPut<TEntity>(this HttpClient httpClient, int entityId, TEntity entity, bool deserialize = true)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = await httpClient.PutAsJsonAsync($"{bizRouteAttr.Route}/{entityId}", entity);

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank || !deserialize
                ? default
                : JsonSerializer.Deserialize<TEntity?>(json)
            : ErrorHandler<TEntity?>(resp.StatusCode, json, isBlank);
    }

    public static async Task<IReadOnlyCollection<TEntity>?> BizPutMany<TEntity>(
        this HttpClient httpClient, 
        IReadOnlyCollection<TEntity> entities, 
        bool deserialize = true)
        where TEntity : IBizEntity
    {
        if (entities is null || entities.Count == 0) return entities;

        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = await httpClient.PutAsJsonAsync(bizRouteAttr.Route, entities);

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank || !deserialize
                ? default
                : JsonSerializer.Deserialize<IReadOnlyCollection<TEntity>?>(json)
            : ErrorHandler<IReadOnlyCollection<TEntity>?>(resp.StatusCode, json, isBlank);
    }

    public static async Task<TEntity?> BizPost<TEntity>(this HttpClient httpClient, TEntity entity, bool deserialize = true)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = await httpClient.PostAsJsonAsync(bizRouteAttr.Route, entity);

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank || !deserialize
                ? default
                : JsonSerializer.Deserialize<TEntity?>(json)
            : ErrorHandler<TEntity?>(resp.StatusCode, json, isBlank);
    }

    public static async Task<IReadOnlyCollection<TEntity>?> BizPostMany<TEntity>(
        this HttpClient httpClient, 
        IReadOnlyCollection<TEntity> entities, 
        bool deserialize = true)
        where TEntity : IBizEntity
    {
        if (entities is null || entities.Count == 0) return entities;

        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = await httpClient.PostAsJsonAsync(bizRouteAttr.Route, entities);

        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank || !deserialize
                ? default
                : JsonSerializer.Deserialize<IReadOnlyCollection<TEntity>?>(json)
            : ErrorHandler<IReadOnlyCollection<TEntity>?>(resp.StatusCode, json, isBlank);
    }

    public static async Task BizDelete<TEntity>(this HttpClient httpClient, int entityId)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = await httpClient.DeleteAsync($"{bizRouteAttr.Route}/{entityId}");
        if (resp.IsSuccessStatusCode) return;

        ErrorHandler<None>(resp.StatusCode, await resp.Content.ReadAsStringAsync());
    }

    public static async Task BizTransition<TEntity>(this HttpClient httpClient, string transition, int entityId, HttpMethod httpMethod)
    where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var resp = httpMethod == HttpMethod.Put
            ? await httpClient.PutAsync($"{bizRouteAttr.Route}/{entityId}?action={transition}", null)
            : await httpClient.PostAsync($"{bizRouteAttr.Route}/{entityId}?action={transition}", null);

        if (resp.IsSuccessStatusCode) return;

        ErrorHandler<None>(resp.StatusCode, await resp.Content.ReadAsStringAsync());
    }

    public static async Task<TResult?> BizAction<TEntity, TInput, TResult>(
        this HttpClient httpClient, 
        string action, 
        int entityId, 
        TInput? input,
        HttpMethod httpMethod)
        where TEntity : IBizEntity
    {
        var bizRouteAttr = typeof(TEntity).GetCustomAttribute<BizEntityAttribute>()
            ?? throw new InvalidOperationException("Did not find the 'BizEntity' attribute on provided generic type argument.");
        if (string.IsNullOrWhiteSpace(bizRouteAttr.Route))
            throw new InvalidOperationException("The provided generic type argument does not have a route specified.");

        var requestUri = entityId < 0
            ? $"{bizRouteAttr.Route}?action={action}"
            : $"{bizRouteAttr.Route}/{entityId}?action={action}";

        var req = httpMethod switch
        {
            var m when m == HttpMethod.Get => httpClient.GetAsync(requestUri),
            var m when m == HttpMethod.Put => httpClient.PutAsJsonAsync(requestUri, input),
            var m when m == HttpMethod.Post => httpClient.PostAsJsonAsync(requestUri, input),
            _ => throw new NotImplementedException()
        };
        var resp = await req;
        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank
                ? default
                : JsonSerializer.Deserialize<TResult?>(json)
            : ErrorHandler<TResult?>(resp.StatusCode, json, isBlank);
    }

    public static async Task<IReadOnlyCollection<TEntity>> GetFromStatistics<TEntity>(this HttpClient client, string? filter = null)
    where TEntity : IBizEntity
    {
        var modelName = typeof(TEntity).Name.ToLowerInvariant();
        var propertyNames = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();
        var requestUri = new StringBuilder("statistics?model=");
        requestUri.Append(Uri.EscapeDataString(modelName));
        requestUri.Append("&wrap=false");
        if (!string.IsNullOrWhiteSpace(filter))
            requestUri.Append(CultureInfo.InvariantCulture, $"&filter={Uri.EscapeDataString(filter)}");
        requestUri.Append("&select=");
        requestUri.Append(Uri.EscapeDataString(string.Join(",", propertyNames)));
        var resp = await client.GetAsync(requestUri.ToString());
        var json = await resp.Content.ReadAsStringAsync();
        var isBlank = string.IsNullOrWhiteSpace(json);
        return resp.IsSuccessStatusCode
            ? isBlank ? [] : JsonSerializer.Deserialize<IReadOnlyCollection<TEntity>>(json) ?? []
            : ErrorHandler<IReadOnlyCollection<TEntity>?>(resp.StatusCode, json, isBlank);
    }

    [DoesNotReturn]
    private static T ErrorHandler<T>(HttpStatusCode statusCode, string? json, bool? isBlank = null)
    {
        try
        {
            if (!isBlank.HasValue)
                isBlank = string.IsNullOrWhiteSpace(json);

            if (!isBlank.Value && !(json![0] is '[' or '{'))
                throw new ApiException(statusCode, json, null);

            var error1 = isBlank.Value ? null : JsonSerializer.Deserialize<ErrorPayload1>(json!);
            if (!string.IsNullOrWhiteSpace(error1?.ErrorReference))
                throw new ApiException(statusCode, error1?.Message, null);

            var error2 = isBlank.Value ? null : JsonSerializer.Deserialize<ErrorPayload2>(json!);
            if (error2 is not null && (error2.ErrorsCount > 0 || error2.WarningsCount > 0 || error2.InfosCount > 0))
                throw new ApiException(statusCode, string.Join("\r\n", error2.Messages.Select(x => x.Message)), null);

            var error3 = isBlank.Value ? null : JsonSerializer.Deserialize<ValidationResult>(json!);
            if (error3 is not null && error3.Data is not null && error3.Data.Count > 0)
            {
                var cnt = 1;
                throw new ApiException(statusCode, string.Join(", ", error3.Data.Values.SelectMany(v => v.Select(w => $"{cnt++:D2}: {w.Message}"))), null);
            }
        }
        catch (ApiException)
        {
            // If we reach this point, we have already thrown an ApiException and should re-throw it
            throw;
        }
        catch { }

        // If we reach this point, we have no idea what the error is
        throw new ApiException(statusCode, "Unknown error", json);
    }

    private sealed record ErrorPayload1(string? Message, string? StackTrace, string? Source, string? ErrorReference);

    private sealed record ErrorMessage(int ID, int EntityID, string? PropertyName, string? EntityType, string Message, int Level);

    private sealed record ErrorPayload2(ErrorMessage[] Messages, int ErrorsCount, int WarningsCount, int InfosCount);

    public record ValidationError(string? PropertyName, string? EntityType, string? Message);

    public record ValidationResult([property: JsonPropertyName("_validationResults")] IDictionary<string, ValidationError[]> Data);
}
