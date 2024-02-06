using System.Collections.Generic;

namespace TerminusDotNet.Test.Utils; 

/**
 * Singleton to share the context map between different step classes
 */

public sealed class ContextMap {
    
    private readonly Dictionary<string, object> _map = new();
    
    static ContextMap() { }
    private ContextMap() { }
    
    public static ContextMap Instance { get; } = new();

    public void Add<KV>(string key, KV value) {
        _map[key] = value;
    }
    
    public KV Get<KV>(string key) {
        return (KV)_map[key];
    }

    public void Clear() {
        _map.Clear();
    }

    public bool HasKey(string key) {
        return _map.ContainsKey(key);
    }
    
}
