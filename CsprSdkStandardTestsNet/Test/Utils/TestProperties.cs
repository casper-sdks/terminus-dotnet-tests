using System;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class TestProperties
{
    private string _hostname;
    private string _dockerName;
    private int _rcpPort;
    private int _restPort;
    private int _ssePort;
    
    public TestProperties()
    {
        _hostname = GetProperty("cspr.hostname", "localhost");
        _dockerName = GetProperty("cspr.docker.name", "cspr-nctl");
        _rcpPort = GetIntProperty("cspr.port.rcp", 11101);
        _restPort = GetIntProperty("cspr.port.rest", 14101);
        _ssePort = GetIntProperty("cspr.port.sse", 18101);
    }

    private static int GetIntProperty(string name, int defaultValue)
    {
        string property = GetProperty(name, null);
        return property != null ? int.Parse(property) : defaultValue;
    }

    private static string GetProperty(string name, string defaultValue)
    {
        var property = Environment.GetEnvironmentVariable(name);
        return property ?? defaultValue;
    }

    public string Hostname
    {
        get => _hostname;
        set => _hostname = value;
    }

    public string DockerName
    {
        get => _dockerName;
        set => _dockerName = value;
    }

    public int RcpPort
    {
        get => _rcpPort;
        set => _rcpPort = value;
    }

    public int RestPort
    {
        get => _restPort;
        set => _restPort = value;
    }

    public int SsePort
    {
        get => _ssePort;
        set => _ssePort = value;
    }
}