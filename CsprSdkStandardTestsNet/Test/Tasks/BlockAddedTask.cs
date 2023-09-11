using System;
using System.Linq;
using System.Threading;
using Casper.Network.SDK.SSE;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Tasks;

/**
 * Starts a listener task for BlockAdded events
 * Uses a CancellationToken to set a timeout  
 */
public class BlockAddedTask {

    private static readonly TestProperties TestProperties = new();
    private readonly ContextMap _contextMap = ContextMap.Instance;    

    public void HasTransferHashWithin(string blockHash, int timeout) {
        
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));
        
        Listen(blockHash, cts.Token);

        if (cts.IsCancellationRequested){
            Assert.Fail("Timeout occured when waiting for BlockAdded event with specified hash.");
        }
    }

    private void Listen(string blockHash, CancellationToken ct) {

        var sse = new ServerEventsClient(TestProperties.Hostname, TestProperties.SsePort);
        var matched = false;
        
        sse.AddEventCallback(EventType.BlockAdded, "blocks-added",  (evt) => {
                try{
                   
                    var block = evt.Parse<BlockAdded>();
                    Assert.IsNotNull(block.BlockHash);
                    
                    if (block.Block.Body.TransferHashes.Contains(blockHash, StringComparer.OrdinalIgnoreCase)){
                        matched = true;
                        _contextMap.Add(StepConstants.LAST_BLOCK_ADDED, block);
                    }
                    
                }
                catch (Exception e){
                    WriteLine(e);
                }
            },
            startFrom: 1);

        sse.StartListening();

        while (!ct.IsCancellationRequested && !matched){
            Thread.Sleep(1000);
        }

        sse.StopListening();
    }
}
