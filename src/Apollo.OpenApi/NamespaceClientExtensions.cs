using Com.Ctrip.Framework.Apollo.OpenApi.Model;

namespace Com.Ctrip.Framework.Apollo.OpenApi;

public static class NamespaceClientExtensions
{
    /// <summary>3.2.6 获取某个Namespace信息接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_326-%e8%8e%b7%e5%8f%96%e6%9f%90%e4%b8%aanamespace%e4%bf%a1%e6%81%af%e6%8e%a5%e5%8f%a3" />
    public static Task<Namespace?> GetNamespaceInfo(this INamespaceClient client,
        CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Get<Namespace>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}", cancellationToken);

    /// <summary>3.2.8 获取某个Namespace当前编辑人接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_328-%e8%8e%b7%e5%8f%96%e6%9f%90%e4%b8%aanamespace%e5%bd%93%e5%89%8d%e7%bc%96%e8%be%91%e4%ba%ba%e6%8e%a5%e5%8f%a3" />
    public static Task<NamespaceLock?> GetNamespaceLock(this INamespaceClient client,
        CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Get<NamespaceLock>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/lock", cancellationToken);

    /// <summary>3.2.9 读取配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_329-%e8%af%bb%e5%8f%96%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<Item?> GetItem(this INamespaceClient client,
        string key, CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Get<Item>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items/{key.ThrowIfNull()}", cancellationToken);

    /// <summary>3.2.10 新增配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3210-%e6%96%b0%e5%a2%9e%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<Item?> CreateItem(this INamespaceClient client,
        Item item, CancellationToken cancellationToken = default)
    {
        item.ThrowIfNull();
        item.Key.ThrowIfNullOrEmpty();
        item.DataChangeCreatedBy.ThrowIfNullOrEmpty();

        return client.ThrowIfNull().Post<Item>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items", item.ThrowIfNull(), cancellationToken);
    }

    /// <summary>3.2.11 修改配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3211-%e4%bf%ae%e6%94%b9%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task UpdateItem(this INamespaceClient client,
        Item item, CancellationToken cancellationToken = default)
    {
        item.ThrowIfNull();
        item.Key.ThrowIfNullOrEmpty();
        item.DataChangeCreatedBy.ThrowIfNullOrEmpty();

        return client.ThrowIfNull().Put<Item>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items/{item.ThrowIfNull().Key}", item, cancellationToken);
    }

    /// <summary>3.2.11 修改配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3211-%e4%bf%ae%e6%94%b9%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<Item?> CreateOrUpdateItem(this INamespaceClient client,
        Item item, CancellationToken cancellationToken = default)
    {
        item.ThrowIfNull();
        item.Key.ThrowIfNullOrEmpty();
        item.DataChangeCreatedBy.ThrowIfNullOrEmpty();

        if (string.IsNullOrEmpty(item.DataChangeLastModifiedBy))
            item.DataChangeLastModifiedBy = item.DataChangeCreatedBy;

        return client.ThrowIfNull().Put<Item>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items/{item.ThrowIfNull().Key}?createIfNotExists=true", item, cancellationToken);
    }

    /// <summary>3.2.12 删除配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3212-%e5%88%a0%e9%99%a4%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    /// <returns>存在时删除后返回true，或者返回false</returns>
    public static Task<bool> RemoveItem(this INamespaceClient client, string key,
        string @operator, CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Delete($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items/{key.ThrowIfNull()}?operator={WebUtility.UrlEncode(@operator.ThrowIfNull())}", cancellationToken);

    /// <summary>3.2.13 发布配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3213-%e5%8f%91%e5%b8%83%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<Release?> Publish(this INamespaceClient client,
        NamespaceRelease release, CancellationToken cancellationToken = default)
    {
        release.ThrowIfNull();
        release.ReleaseTitle.ThrowIfNullOrEmpty();
        release.ReleasedBy.ThrowIfNullOrEmpty();

        return client.ThrowIfNull().Post<Release>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/releases", release.ThrowIfNull(), cancellationToken);
    }

    /// <summary>3.2.14 获取某个Namespace当前生效的已发布配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3214-%e8%8e%b7%e5%8f%96%e6%9f%90%e4%b8%aanamespace%e5%bd%93%e5%89%8d%e7%94%9f%e6%95%88%e7%9a%84%e5%b7%b2%e5%8f%91%e5%b8%83%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<Release?> GetLatestActiveRelease(this INamespaceClient client,
        CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Get<Release>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/releases/latest", cancellationToken);

    /// <summary>3.2.15 回滚已发布配置接口</summary>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3215-%e5%9b%9e%e6%bb%9a%e5%b7%b2%e5%8f%91%e5%b8%83%e9%85%8d%e7%bd%ae%e6%8e%a5%e5%8f%a3" />
    public static Task<bool> Rollback(this INamespaceClient client, string @operator, int releaseId,
        CancellationToken cancellationToken = default) =>
        client.ThrowIfNull().Put($"envs/{client.Env}/releases/{releaseId}/rollback?operator={WebUtility.UrlEncode(@operator)}", cancellationToken);

    /// <summary>3.2.16 分页获取配置项接口</summary>
    /// <param name="client"></param>
    /// <param name="page">从 0 开始</param>
    /// <param name="size"></param>
    /// <param name="cancellationToken"></param>
    /// <see href="https://www.apolloconfig.com/#/zh/usage/apollo-open-api-platform?id=_3216-%e5%88%86%e9%a1%b5%e8%8e%b7%e5%8f%96%e9%85%8d%e7%bd%ae%e9%a1%b9%e6%8e%a5%e5%8f%a3" />
    public static async Task<PageModel<Item>> GetItems(this INamespaceClient client, int page = 0, int size = 50, CancellationToken cancellationToken = default)
    {
        var result = await client.ThrowIfNull().Get<PageModel<Item>>($"envs/{client.Env}/apps/{client.AppId}/clusters/{client.Cluster}/namespaces/{client.Namespace}/items?page={page}&size={size}", cancellationToken);

        return result ?? new PageModel<Item>();
    }
}
