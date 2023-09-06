using System.Collections.Generic;

namespace CsprSdkStandardTestsNet.Test.Utils; 

public sealed class ContextMap {
    
    private readonly Dictionary<string, object> _map = new();
    
    static ContextMap() { }
    private ContextMap() { }
    
    public static ContextMap Instance { get; } = new ContextMap();

    public void Add<TV>(string key, TV value) {
        _map[key] = value;
    }
    
    public TV Get<TV>(string key) {
        return (TV)_map[key];
    }

    public void Clear() {
        _map.Clear();
    }

    public bool HasKey(string key) {
        return _map.ContainsKey(key);
    }
    
}