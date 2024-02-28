using System;

namespace TerminusDotNet.Test.Utils;

public class TestProperties {
    public TestProperties() {
        ChainName = GetProperty("cspr.chain.name", "cspr-dev-cctl");;
        Hostname = GetProperty("cspr.hostname", "localhost");
        DockerName = GetProperty("cspr.docker.name", "cspr-cctl");
        RcpPort = GetIntProperty("cspr.port.rcp", 11101);
        GetIntProperty("cspr.port.rest", 14101);
        SsePort = GetIntProperty("cspr.port.sse", 18101);
        SpecPort = GetIntProperty("cspr.port.spec", 25101);
    }

    public int SpecPort { get; }

    public string Hostname { get; }

    public string DockerName { get; }

    public int RcpPort { get; }

    public int SsePort { get; }

    public string ChainName { get; }
    
    private static int GetIntProperty(string name, int defaultValue) {
        var property = GetProperty(name, null);
        return property != null ? int.Parse(property) : defaultValue;
    }

    private static string GetProperty(string name, string defaultValue) {
        var property = Environment.GetEnvironmentVariable(name);
        return property ?? defaultValue;
    }
}
