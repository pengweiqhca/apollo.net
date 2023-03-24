using Com.Ctrip.Framework.Apollo.Exceptions;

namespace Com.Ctrip.Framework.Apollo.Util.Http;

public class HttpUtil : IDisposable
{
    private readonly IApolloOptions _options;

    public HttpUtil(IApolloOptions options) => _options = options;

    public Task<HttpResponse<T>> DoGetAsync<T>(Uri url) => DoGetAsync<T>(url, _options.Timeout);

    public async Task<HttpResponse<T>> DoGetAsync<T>(Uri url, int timeout)
    {
        url.ThrowIfNull();

        Exception e;
        try
        {
            using var cts = new CancellationTokenSource(timeout);

            var httpClient = new HttpClient(_options.HttpMessageHandler, false)
            {
                Timeout = TimeSpan.FromMilliseconds(timeout > 0 ? timeout : _options.Timeout)
            };

            if (!string.IsNullOrWhiteSpace(_options.Secret))
                foreach (var header in Signature.BuildHttpHeaders(url, _options.AppId, _options.Secret!))
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);

            using var response = await httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new(response.StatusCode, await response.Content.ReadFromJsonAsync<T>(
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                        }, cts.Token).ConfigureAwait(false));
                case HttpStatusCode.NotModified:
                    return new(response.StatusCode);
            }

            e = new ApolloConfigStatusCodeException(response.StatusCode, $"Get operation failed for {url}");
        }
        catch (Exception ex)
        {
            e = new ApolloConfigException("Could not complete get operation", ex);
        }

        throw e;
    }

    public void Dispose()
    {
        if (_options is IDisposable disposable) disposable.Dispose();

        GC.SuppressFinalize(this);
    }
}
