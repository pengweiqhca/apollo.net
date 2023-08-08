using Com.Ctrip.Framework.Apollo.Exceptions;

namespace Com.Ctrip.Framework.Apollo.Util.Http;

internal sealed class HttpUtil : IDisposable
{
    private readonly IApolloOptions _options;

    public HttpUtil(IApolloOptions options) => _options = options;

    public Task<HttpResponse<T>> DoGetAsync<T>(Uri url, CancellationToken cancellationToken) =>
        DoGetAsync<T>(url, _options.Timeout, cancellationToken);

    public async Task<HttpResponse<T>> DoGetAsync<T>(Uri url, int timeout, CancellationToken cancellationToken)
    {
        url.ThrowIfNull();

        var httpClient = new HttpClient(_options.HttpMessageHandler, false)
        {
            Timeout = TimeSpan.FromMilliseconds(timeout > 0 ? timeout : _options.Timeout)
        };

        if (!string.IsNullOrWhiteSpace(_options.Secret))
            foreach (var header in Signature.BuildHttpHeaders(url, _options.AppId, _options.Secret!))
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);

        using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        return response.StatusCode switch
        {
            HttpStatusCode.OK =>
                string.Equals(response.Content.Headers.ContentType?.MediaType?.Split('/')[^1], "json",
                    StringComparison.OrdinalIgnoreCase)
                    ? new(response.StatusCode, await response.Content
                        .ReadFromJsonAsync<T>(JsonUtil.JsonSettings, cancellationToken).ConfigureAwait(false))
                    : throw new ApolloConfigException(
                        $"Response content type is not json: {response.Content.Headers.ContentType?.MediaType ?? "`empty`"}"),
            HttpStatusCode.NotModified => new(response.StatusCode),
            _ => throw new ApolloConfigStatusCodeException(response.StatusCode, $"Get operation failed for {url}")
        };
    }

    public void Dispose()
    {
        if (_options is IDisposable disposable) disposable.Dispose();
    }
}
