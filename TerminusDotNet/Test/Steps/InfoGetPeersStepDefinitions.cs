using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * info_get_peers step definitions
 */
[Binding]
public class InfoGetPeersStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;

    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [Given(@"that the info_get_peers RPC method is invoked against a node")]
    public async Task GivenThatTheInfoGetPeersRpcMethodIsInvokedAgainstANode() {
        WriteLine("that the info_get_peers RPC method is invoked against a node");

        var peerData = await GetCasperService().GetNodePeers();

        _contextMap.Add(StepConstants.PEER_DATA, peerData);
        
    }

    [Then(@"the node returns an info_get_peers_result")]
    public void ThenTheNodeReturnsAnInfoGetPeersResult() {
        WriteLine("the node returns an info_get_peers_result");
        
        Assert.That(GetPeerData(), Is.Not.Null);
        
    }

    [Then(@"the info_get_peers_result has an API version of ""(.*)""")]
    public void ThenTheInfoGetPeersResultHasAnApiVersionOf(string apiVersion) {
        WriteLine("the info_get_peers_result has an API version of {0}", apiVersion);
        
        Assert.That(GetPeerData().ApiVersion, Is.EqualTo(apiVersion));
        
    }

    [Then(@"the info_get_peers_result contains (.*) peers")]
    public void ThenTheInfoGetPeersResultContainsPeers(int peers) {
        WriteLine("the info_get_peers_result contains {0} peers", peers);

        Assert.That(GetPeerData().Peers.Count, Is.EqualTo(peers));

    }

    [Then(@"the info_get_peers_result contains a valid peer with a port number of (.*)")]
    public void ThenTheInfoGetPeersResultContainsAValidPeerWithAPortNumberOf(int port) {
        WriteLine("the info_get_peers_result contains a valid peer with a port number of {0}", port);

        var peer = GetPeerData().Peers.Find(p => p.Address.Contains(port.ToString()));
        Assert.That(peer, Is.Not.Null);
        
        Assert.That(IsValidPeer(port, peer), Is.True);

    }
    
    private GetNodePeersResult GetPeerData() {
        return _contextMap.Get<RpcResponse<GetNodePeersResult>>(StepConstants.PEER_DATA).Parse();
    }
    
    private bool IsValidPeer(int port,  Peer peerEntry) {
        return peerEntry.Address.EndsWith(":" + port) && peerEntry.NodeId.StartsWith("tls:") ;
    }
    
}
