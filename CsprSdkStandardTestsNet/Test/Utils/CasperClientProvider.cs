using System;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class CasperClientProvider
{
    private static CasperClientProvider _instance;
    private NetCasperClient _casperService;
    private ServerEventsClient _eventService;
    
    public static  CasperClientProvider GetInstance()
    {
        return _instance ??= new CasperClientProvider();
    }
    
    private CasperClientProvider() {
        try {
            var properties = new TestProperties();
            _casperService = new NetCasperClient("http://" + properties.Hostname + ":" + properties.RcpPort + "/rpc");
        } catch (Exception exception) {
            throw new Exception(exception.ToString());
        }
    }

    public NetCasperClient CasperService
    {
        get => _casperService;
        set => _casperService = value;
    }

    public ServerEventsClient EventService
    {
        get => _eventService;
        set => _eventService = value;
    }
}