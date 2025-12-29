using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiPanel.Client.Abstract.Services;

/// <summary>
///     可写配置文件工厂
/// </summary>
public interface IWritableConfigureFactory
{
    /// <summary>
    ///     获取可写的配置项
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <typeparam name="T">配置类型</typeparam>
    /// <returns>可写配置项</returns>
    IWritableConfigure<T> GetConfigure<T>(string path)
        where T : class, new();

    void SaveToFile(string path);
    TOption DeSerialize<TOption>(string path) where TOption : class, new();
}

internal class WritableConfigureFactory : IWritableConfigureFactory
{
    private static readonly Dictionary<string, Dictionary<Type, IWritableConfigure>> Table = new();
    private static readonly Dictionary<string, JObject> CacheData = new();

    public IWritableConfigure<T> GetConfigure<T>(string path) where T : class, new()
    {
        if (!Table.TryGetValue(path, out var dic) || !dic.TryGetValue(typeof(T), out var wc))
            return CreateNewConfigure();
        return (IWritableConfigure<T>)wc;

        IWritableConfigure<T> CreateNewConfigure()
        {
            lock (Table)
            {
                if (!Table.TryGetValue(path, out var d))
                    Table.Add(path, d = new Dictionary<Type, IWritableConfigure>());

                if (d.TryGetValue(typeof(T), out var w)) return (IWritableConfigure<T>)w;

                d.Add(typeof(T), w = new WritableConfigure<T>(this, path));
                return (IWritableConfigure<T>)w;
            }
        }
    }

    public void SaveToFile(string path)
    {
        var jo = CacheData[path];
        foreach (var (t, w) in Table[path])
            jo[t.Name] = JObject.FromObject(w.GetValue());
        File.WriteAllText(path, jo.ToString());
    }

    public TOption DeSerialize<TOption>(string path) where TOption : class, new()
    {
        if (!CacheData.TryGetValue(path, out var jo))
        {
            EnsureFileExists(path);
            using var sr = File.OpenText(path);
            using var jr = new JsonTextReader(sr);
            CacheData.Add(path, jo = JObject.Load(jr));
        }

        var tNode = jo[typeof(TOption).Name];
        return tNode?.ToObject<TOption>() ?? new TOption();
    }

    private void EnsureFileExists(string path)
    {
        path = Path.GetFullPath(path);
        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
            File.WriteAllText(path, new JObject().ToString());
    }
}

public interface IWritableConfigure
{
    string FilePath { get; }
    void SaveToFile();
    object GetValue();
}

public interface IWritableConfigure<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes
        .PublicParameterlessConstructor)]
    out TOptions> : IWritableConfigure, IOptions<TOptions> where TOptions : class, new()
{
}

internal class WritableConfigure<TOptions>(IWritableConfigureFactory factory, string path)
    : IWritableConfigure<TOptions>
    where TOptions : class, new()
{
    public TOptions Value { get; } = factory.DeSerialize<TOptions>(path);
    public string FilePath { get; } = path;

    public void SaveToFile()
    {
        factory.SaveToFile(FilePath);
    }

    public object GetValue()
    {
        return Value;
    }
}