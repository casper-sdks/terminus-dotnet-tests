using System;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class TestProperties{
    public TestProperties()
    {
        Hostname = GetProperty("cspr.hostname", "localhost");
        DockerName = GetProperty("cspr.docker.name", "cspr-nctl");
        RcpPort = GetIntProperty("cspr.port.rcp", 11101);
        RestPort = GetIntProperty("cspr.port.rest", 14101);
        SsePort = GetIntProperty("cspr.port.sse", 18101);
    }

    private static int GetIntProperty(string name, int defaultValue){
        string property = GetProperty(name, null);
        return property != null ? int.Parse(property) : defaultValue;
    }

    private static string GetProperty(string name, string defaultValue){
        var property = Environment.GetEnvironmentVariable(name);
        return property ?? defaultValue;
    }

    public string Hostname { get; set; }

    public string DockerName { get; set; }

    public int RcpPort { get; set; }

    public int RestPort { get; set; }

    public int SsePort { get; set; }
}
