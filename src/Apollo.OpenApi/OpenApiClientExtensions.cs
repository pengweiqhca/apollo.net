using System.Net.Http.Json;
using System.Text.Json;

namespace Com.Ctrip.Framework.Apollo.OpenApi;

internal static class OpenApiClientExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
    };

    public static async Task<TResponse?> Get<TResponse>(this IOpenApiClient client, string url, CancellationToken cancellationToken) where TResponse : class
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        using var httpClient = client.CreateHttpClient();
        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        await AssertResponse(response).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<TResponse>(Options, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<bool> Delete(this IOpenApiClient client, string url, CancellationToken cancellationToken)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        using var httpClient = client.CreateHttpClient();
        using var response = await httpClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return false;

        await AssertResponse(response).ConfigureAwait(false);

        return true;
    }

    public static async Task<TResponse?> Post<TResponse>(this IOpenApiClient client, string url, object data, CancellationToken cancellationToken)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        using var httpClient = client.CreateHttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(data, options: Options)
        };
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        await AssertResponse(response).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<TResponse>(Options, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<bool> Put(this IOpenApiClient client, string url, CancellationToken cancellationToken)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        using var httpClient = client.CreateHttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return false;

        await AssertResponse(response).ConfigureAwait(false);

        return true;
    }

    public static async Task<TResponse?> Put<TResponse>(this IOpenApiClient client, string url, object data, CancellationToken cancellationToken)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));

        using var httpClient = client.CreateHttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(data, options: Options)
        };
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        await AssertResponse(response).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<TResponse>(Options, cancellationToken).ConfigureAwait(false);
    }

    private static async Task AssertResponse(HttpResponseMessage response)
    {
        if (response.StatusCode < HttpStatusCode.BadRequest) return;

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Exception ex;
        try
        {
            var msg = JsonSerializer.Deserialize<ExceptionMessage>(body, Options);

            ex = msg == null
                ? new(response.StatusCode, response.ReasonPhrase, body)
                : new ApolloOpenApiException(response.StatusCode, string.IsNullOrEmpty(response.ReasonPhrase) ? msg.Exception : response.ReasonPhrase, msg.Message);
        }
        catch (Exception e)
        {
            ex = new ApolloOpenApiException(response.StatusCode, response.ReasonPhrase, body, e);
        }

        throw ex;
    }

    private sealed record ExceptionMessage(string Message, string Exception);
}
