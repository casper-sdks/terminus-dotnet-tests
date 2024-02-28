using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * Info Get Chain Spec steps
 */
[Binding]
public class InfoGetChainSpecStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private static readonly TestProperties TestProperties = new();
    
    private readonly SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [Given(@"that info_get_chainspec is invoked against the SDK")]
    public async Task GivenThatInfoGetChainspecIsInvokedAgainstTheSDK() {
        WriteLine("that info_get_chainspec is invoked against the SDK");
        
        var chainSpecSdk = await GetCasperService().GetChainspec();

        Assert.That(chainSpecSdk, Is.Not.Null);

        _contextMap.Add(StepConstants.INFO_GET_CHAINSPEC_SDK, chainSpecSdk);

    }

    [Given(@"that the info_get_chainspec is invoked using a simple RPC json request")]
    public async Task GivenThatTheInfoGetChainspecIsInvokedUsingASimpleRpcJsonRequest() {
        WriteLine("that the info_get_chainspec is invoked using a simple RPC json request");
        
        var chainSpecNode = await _simpleRcpClient.GetInfoGetChainspec();

        Assert.That(chainSpecNode, Is.Not.Null);

        _contextMap.Add(StepConstants.INFO_GET_CHAINSPEC_NODE, chainSpecNode);
    }

    [Then(@"the SDK chain bytes equals the RPC json request chain bytes")]
    public void ThenTheSdkChainBytesEqualsTheRpcJsonRequestChainBytes() {
        WriteLine("the SDK chain bytes equals the RPC json request chain bytes");

        var chainSpecNode = _contextMap.Get<JsonNode>(StepConstants.INFO_GET_CHAINSPEC_NODE);
        var chainSpecSdk = _contextMap.Get<RpcResponse<GetChainspecResult>>(StepConstants.INFO_GET_CHAINSPEC_SDK);

        Assert.That(chainSpecSdk.Parse().ChainspecBytes.ChainspecBytes.ToUpper(),
            Is.EqualTo(chainSpecNode["result"]!["chainspec_bytes"]!["chainspec_bytes"]!.ToString().ToUpper()));

    }

    [Then(@"the SDK genesis bytes equals the RPC json request genesis bytes")]
    public void ThenTheSdkGenesisBytesEqualsTheRpcJsonRequestGenesisBytes() {
        WriteLine("the SDK genesis bytes equals the RPC json request genesis bytes");

        var chainSpecNode = _contextMap.Get<JsonNode>(StepConstants.INFO_GET_CHAINSPEC_NODE);
        var chainSpecSdk = _contextMap.Get<RpcResponse<GetChainspecResult>>(StepConstants.INFO_GET_CHAINSPEC_SDK);

        if (chainSpecSdk.Parse().ChainspecBytes.MaybeGenesisAccountsBytes == null) {
            Assert.That(chainSpecNode["result"]!["chainspec_bytes"]!["maybe_genesis_accounts_bytes"], Is.EqualTo(null));
        }
        else {
            Assert.That(chainSpecSdk.Parse().ChainspecBytes.MaybeGenesisAccountsBytes.ToUpper(),
                Is.EqualTo(chainSpecNode["result"]!["chainspec_bytes"]!["maybe_genesis_accounts_bytes"]!.ToString().ToUpper()));
        }
        
    }

    [Then(@"the SDK global state bytes equals the RPC json request global state bytes")]
    public void ThenTheSdkGlobalStateBytesEqualsTheRpcJsonRequestGlobalStateBytes() {
        WriteLine("the SDK global state bytes equals the RPC json request global state bytes");
        
        var chainSpecNode = _contextMap.Get<JsonNode>(StepConstants.INFO_GET_CHAINSPEC_NODE);
        var chainSpecSdk = _contextMap.Get<RpcResponse<GetChainspecResult>>(StepConstants.INFO_GET_CHAINSPEC_SDK);

        if (chainSpecSdk.Parse().ChainspecBytes.MaybeGlobalStateBytes == null) {
            Assert.That(chainSpecNode["result"]!["chainspec_bytes"]!["maybe_global_state_bytes"], Is.EqualTo(null));
        }
        else {
            Assert.That(chainSpecSdk.Parse().ChainspecBytes.MaybeGlobalStateBytes.ToUpper(),
                Is.EqualTo(chainSpecNode["result"]!["chainspec_bytes"]!["maybe_global_state_bytes"]!.ToString().ToUpper()));
        }
        
    }
}
