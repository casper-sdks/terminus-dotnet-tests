using System;
using System.Linq;
using System.Threading;
using Casper.Network.SDK.SSE;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Tasks;

public class BlockAddedTask{

    private static readonly TestProperties TestProperties = new();

    public void HasTransferHashWithin(string blockHash, int timeout){
        
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeout);
        
        Listen(blockHash, cts.Token);

        if (cts.IsCancellationRequested){
            Assert.Fail("failed to find the required blockhash");
        }
        
    }

    private void Listen(string blockHash, CancellationToken ct){

        var sse = new ServerEventsClient(TestProperties.Hostname, TestProperties.SsePort);
        var matched = false;
        
        sse.AddEventCallback(EventType.BlockAdded, "blocks-added", async (SSEvent evt) => {
                try{

                    var block = evt.Parse<BlockAdded>();
                    Assert.IsNotNull(block.BlockHash);
                    
                    WriteLine(blockHash + "," + block.BlockHash);

                    if (block.Block.Body.TransferHashes.Contains(blockHash, StringComparer.OrdinalIgnoreCase)){
                        matched = true;
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