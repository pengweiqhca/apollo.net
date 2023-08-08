﻿using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Apollo.Configuration.Demo;

internal class ConfigurationDemo
{
    private const string DefaultValue = "undefined";
    private readonly IConfiguration _config;
    private readonly IConfiguration _anotherConfig;

    public ConfigurationDemo()
    {
        var host = Host.CreateDefaultBuilder()
            .AddApollo()
            .ConfigureServices((context, services) =>
            {
                services.AddOptions()
                    .Configure<Value>(context.Configuration)
                    .Configure<Value>("other", context.Configuration.GetSection("a"));
            })
            .Build();

        _config = host.Services.GetRequiredService<IConfiguration>();
        _anotherConfig = _config.GetSection("a");

        host.Services.GetRequiredService<IOptionsMonitor<Value>>().OnChange(OnChanged);

        //new ConfigurationManagerDemo( host.Services.GetService<ApolloConfigurationManager>());
    }

    public string GetConfig(string key)
    {
        var result = _config.GetValue(key, DefaultValue);
        if (result.Equals(DefaultValue, StringComparison.Ordinal)) result = _anotherConfig.GetValue(key, DefaultValue);
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Loading key: {0} with value: {1}", key, result);
        Console.ForegroundColor = color;

        return result;
    }

    private static void OnChanged(Value value, string name) => Console.WriteLine(name + " has changed: " + JsonSerializer.Serialize(value));

    private class Value
    {
        public string Timeout { get; set; } = "";
    }
}
