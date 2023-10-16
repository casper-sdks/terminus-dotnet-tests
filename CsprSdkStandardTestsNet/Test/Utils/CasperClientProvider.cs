using System;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class CasperClientProvider {
    private static CasperClientProvider _instance;

    private CasperClientProvider() {
        try{
            var properties = new TestProperties();
            CasperService = new NetCasperClient("http://" + properties.Hostname + ":" + properties.RcpPort + "/rpc");
        }
        catch (Exception exception) {
            throw new Exception(exception.ToString());
        }
    }

    public NetCasperClient CasperService { get; set; }

    public ServerEventsClient EventService { get; set; }

    public static CasperClientProvider GetInstance() {
        return _instance ??= new CasperClientProvider();
    }
}

public class CasperSpeculativeClientProvider {
    private static CasperSpeculativeClientProvider _instance;

    private CasperSpeculativeClientProvider() {
        try{
            var properties = new TestProperties();
            CasperService = new NetCasperClient("http://" + properties.Hostname + ":" + properties.SpecPort + "/rpc");
        }
        catch (Exception exception) {
            throw new Exception(exception.ToString());
        }
    }

    public NetCasperClient CasperService { get; set; }

    public ServerEventsClient EventService { get; set; }

    public static CasperSpeculativeClientProvider GetInstance() {
        return _instance ??= new CasperSpeculativeClientProvider();
    }
}
