using System.Globalization;
using UniEventIntegration.Models;

namespace UniEventIntegration.UnimicroPlatform;

public static class HttpClientExtensions
{
    public static void SetCompanyKey(this HttpClient httpClient, Guid companyKey)
    {
        if (httpClient is null || companyKey == Guid.Empty) return;
        httpClient.DefaultRequestHeaders.Add("companykey", companyKey.ToString());
    }

    public static void SetCompanyKey(this HttpClient httpClient, string companyKey)
    {
        if (httpClient is null || string.IsNullOrWhiteSpace(companyKey)) return;
        httpClient.DefaultRequestHeaders.Add("companykey", companyKey);
    }

    public static async Task<ValueList?> GetValueListByCodeAsync(this HttpClient client, string code)
    { 
        var lists = await client.GetFromJsonAsync(
            $"biz/valuelists?filter=code eq '{code}'&expand=items&select=code,name,items.name,items.code,items.value",
            ApiJsonCtx.Default.ICollectionValueList);
        return lists?.FirstOrDefault();
    }

    public static async Task<int> GetModelIDbyNameAsync(this HttpClient client, string modelName)
    {
        var models = await client.GetFromJsonAsync(
            $"biz/models?filter=name eq '{modelName}'&select='ID'", 
            ApiJsonCtx.Default.ICollectionIDPayload);
        return models is null ? -1 : models.Select(x => x.ID).First();
    }

    public static async Task<int> AddCustomFieldAsync(this HttpClient client, CustomField customField)
    {
        var resp = await client.PostAsJsonAsync(
            "biz/custom-fields", 
            customField, 
            ApiJsonCtx.Default.CustomField);
        resp.EnsureSuccessStatusCode();
        var id = (await resp.Content.ReadFromJsonAsync(ApiJsonCtx.Default.IDPayload))?.ID ?? -1;
        if (id < 0) return id;
        resp = await client.PostAsJsonAsync<object>($"biz/custom-fields/{id}?action=activate", new { });
        resp.EnsureSuccessStatusCode();
        return id;
    }


    public static async Task<bool> SetProductPurchaseToActive(this HttpClient client, int purchaseProductId)
    {
        var resp = await client.PutAsync($"elsa/purchases/{purchaseProductId}?action=set-as-active", new StringContent(""));
        return resp.IsSuccessStatusCode;
    }

}
