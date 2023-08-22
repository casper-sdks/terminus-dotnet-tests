using System;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class TestProperties {
    public TestProperties(){
        Hostname = GetProperty("cspr.hostname", "localhost");
        DockerName = GetProperty("cspr.docker.name", "cspr-nctl");
        RcpPort = GetIntProperty("cspr.port.rcp", 11101);
        GetIntProperty("cspr.port.rest", 14101);
        SsePort = GetIntProperty("cspr.port.sse", 18101);
    }

    public string Hostname { get; }

    public string DockerName { get; }

    public int RcpPort { get; }

    public int SsePort { get; }

    private static int GetIntProperty(string name, int defaultValue){
        var property = GetProperty(name, null);
        return property != null ? int.Parse(property) : defaultValue;
    }

    private static string GetProperty(string name, string defaultValue){
        var property = Environment.GetEnvironmentVariable(name);
        return property ?? defaultValue;
    }
}
