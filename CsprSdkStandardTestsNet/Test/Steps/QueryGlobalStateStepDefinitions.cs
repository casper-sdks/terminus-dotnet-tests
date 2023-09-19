using System;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.SSE;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using CsprSdkStandardTestsNet.Test.Tasks;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * Query global state step definitions
 */

[Binding]
public class QueryGlobalStateStepDefinitions {
    
    private static readonly TestProperties TestProperties = new();
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private readonly Nctl _nctl = new(TestProperties.DockerName);
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a valid block hash is known")]
    public async Task GivenThatAValidBlockHashIsKnown() {
        WriteLine("that a valid block hash is known");

        await CreateTransfer();
        await WaitForBlockAdded();
        
        Assert.That(_contextMap.Get<BlockAdded>(StepConstants.LAST_BLOCK_ADDED), Is.Not.Null);

    }

    [When(@"the query_global_state RCP method is invoked with the block hash as the query identifier")]
    public async Task WhenTheQueryGlobalStateRcpMethodIsInvokedWithTheBlockHashAsTheQueryIdentifier() {
        WriteLine("that a valid block hash is known");

        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        var blockHash = _contextMap.Get<BlockAdded>(StepConstants.LAST_BLOCK_ADDED).BlockHash;

        var key = GlobalStateKey.FromString("deploy-" + deployResult.Parse().DeployHash);

        var globalStateData = 
            await GetCasperService().QueryGlobalStateWithBlockHash(key, blockHash);

        Assert.That(globalStateData, Is.Not.Null);
        
        _contextMap.Add(StepConstants.GLOBAL_STATE_DATA, globalStateData);
        
    }

    [Then(@"a valid query_global_state_result is returned")]
    public void ThenAValidQueryGlobalStateResultIsReturned() {
        WriteLine("a valid query_global_state_result is returned");

        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        
        Assert.That(globalStateData, Is.Not.Null);
        Assert.That(globalStateData.Parse().ApiVersion, Is.EqualTo("1.0.0"));
        Assert.That(globalStateData.Parse().MerkleProof, Is.Not.Null);
        Assert.That(globalStateData.Parse().BlockHeader.Timestamp, Is.Not.Null);
        Assert.That(globalStateData.Parse().BlockHeader.EraId, Is.GreaterThan(0));
        Assert.That(globalStateData.Parse().BlockHeader.AccumulatedSeed, Is.Not.Null);
        Assert.That(globalStateData.Parse().BlockHeader.BodyHash, Is.Not.Null);
        Assert.That(globalStateData.Parse().BlockHeader.ParentHash, Is.Not.Null);

    }

    [Then(@"the query_global_state_result contains a valid deploy info stored value")]
    public void ThenTheQueryGlobalStateResultContainsAValidDeployInfoStoredValue() {
        WriteLine("the query_global_state_result contains a valid deploy info stored value");
        
        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        
        Assert.That(globalStateData.Parse().StoredValue, Is.Not.Null);
        
    }

    [Then(@"the query_global_state_result's stored value from is the user-(.*) account hash")]
    public void ThenTheQueryGlobalStateResultsStoredValueFromIsTheUserAccountHash(int user) {
        WriteLine("the query_global_state_result's stored value from is the user-{0} account hash", user);
        
        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        var deployInfo = globalStateData.Parse().StoredValue.DeployInfo;
        var accountHash = _nctl.GetAccountHash(user);
        
        Assert.That(deployInfo.From.ToString()!.ToUpper(), 
            Is.EqualTo(accountHash.ToUpper()));

    }

    [Then(@"the query_global_state_result's stored value contains a gas price of (.*)")]
    public void ThenTheQueryGlobalStateResultsStoredValueContainsAGasPriceOf(string gasPrice) {
        WriteLine("the query_global_state_result's stored value contains a gas price of {0}", gasPrice);
        
        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        var deployInfo = globalStateData.Parse().StoredValue.DeployInfo;

        Assert.That(deployInfo.Gas, Is.EqualTo(BigInteger.Parse(gasPrice)));
        
    }

    [Then(@"the query_global_state_result stored value contains the transfer hash")]
    public void ThenTheQueryGlobalStateResultStoredValueContainsTheTransferHash() {
        WriteLine("the query_global_state_result stored value contains the transfer hash");
        
        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        var deployInfo = globalStateData.Parse().StoredValue.DeployInfo;

        Assert.That(deployInfo.Transfers[0].ToString()!.StartsWith("transfer-"), Is.True);
        
    }

    [Then(@"the query_global_state_result stored value contains the transfer source uref")]
    public void ThenTheQueryGlobalStateResultStoredValueContainsTheTransferSourceUref() {
        WriteLine("the query_global_state_result stored value contains the transfer source uref");
     
        var globalStateData = _contextMap.Get<RpcResponse<QueryGlobalStateResult>>(StepConstants.GLOBAL_STATE_DATA);
        var deployInfo = globalStateData.Parse().StoredValue.DeployInfo;
        var accountMainPurse = _nctl.GetAccountMainPurse(1);
        
        Assert.That(deployInfo.Source.ToString().ToUpper(), Is.EqualTo(accountMainPurse.ToUpper()));
        
    }

    [Given(@"that the state root hash is known")]
    public async Task GivenThatTheStateRootHashIsKnown() {
        WriteLine("that the state root hash is known");

        var stateRootHash = await GetCasperService().GetStateRootHash();
        
        Assert.That(stateRootHash, Is.Not.Null);

        _contextMap.Add(StepConstants.STATE_ROOT_HASH, stateRootHash);

    }

    [When(@"the query_global_state RCP method is invoked with the state root hash as the query identifier and an invalid key")]
    public async Task WhenTheQueryGlobalStateRcpMethodIsInvokedWithTheStateRootHashAsTheQueryIdentifierAndAnInvalidKey() {
        WriteLine("the query_global_state RCP method is invoked with the state root hash as the query identifier and an invalid key");

        var stateRootHash = _contextMap.Get<string>(StepConstants.STATE_ROOT_HASH);
        var key = "uref-d0343bb766946f9f850a67765aae267044fa79a6cd50235ffff248a37534-007";

        try { 
            await GetCasperService().QueryGlobalState(key, stateRootHash);
        } catch (RpcClientException e) {
            _contextMap.Add(StepConstants.CLIENT_EXCEPTION, e);            
        }
    
    }

    [Then(@"an error code of (.*) is returned")]
    public void ThenAnErrorCodeOfIsReturned(int code) {
        WriteLine("an error code of {0} is returned", code);

        var clientException = _contextMap.Get<RpcClientException>(StepConstants.CLIENT_EXCEPTION);
        Assert.That(clientException.RpcError.Code, Is.EqualTo(code));

    }

    [Then(@"an error message of ""(.*)"" is returned")]
    public void ThenAnErrorMessageOfIsReturned(string msg) {
        WriteLine("an error message of {0} is returned", msg);
        
        var clientException = _contextMap.Get<RpcClientException>(StepConstants.CLIENT_EXCEPTION);
        Assert.That(clientException.RpcError.Message, Contains.Substring(msg));
        
    }

    [Given(@"the query_global_state RCP method is invoked with an invalid block hash as the query identifier")]
    public async Task GivenTheQueryGlobalStateRcpMethodIsInvokedWithAnInvalidBlockHashAsTheQueryIdentifier() {
        WriteLine("the query_global_state RCP method is invoked with an invalid block hash as the query identifier");
        
        var block = "00112233441343670f71afb96018ab193855a85adc412f81571570dea34f2ca6500";
        var key = "deploy-80fbb9c25eebda88e5d2eb9a0f7053ad6098d487aff841dc719e1526e0f59728";

        try {
            await GetCasperService().QueryGlobalStateWithBlockHash(key, block);
        } catch (RpcClientException e) {
            _contextMap.Add(StepConstants.CLIENT_EXCEPTION, e);
        }
        
    }

    private async Task CreateTransfer() {

        var senderPem = AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem");
        var senderKey = KeyPair.FromPem(senderPem);

        Assert.IsNotNull(senderKey);
        
        var receiverPem = AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem");
        var receiverKey = PublicKey.FromPem(receiverPem);

        Assert.IsNotNull(receiverKey);  

        _contextMap.Add(StepConstants.DEPLOY_TIMESTAMP, DateUtils.ToEpochTime(DateTime.UtcNow));
        
        var deploy = DeployTemplates.StandardTransfer(
            senderKey.PublicKey,
            receiverKey,
            new BigInteger(2500000000),
            100_000_000,
            "casper-net-1",
            null,
            1,
            (ulong)TimeSpan.FromMinutes(30).TotalMilliseconds);
        
        deploy.Sign(senderKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        _contextMap.Add(StepConstants.DEPLOY_RESULT, putResponse);
        
    }
    
    
    private async Task WaitForBlockAdded() {

        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        
        var sseBlockAdded = new BlockAddedTask();
        sseBlockAdded.HasTransferHashWithin(deployResult.Parse().DeployHash, 180);
            
        var matchingBlockHash = _contextMap.Get<BlockAdded>(StepConstants.LAST_BLOCK_ADDED).BlockHash;
        
        Assert.That(matchingBlockHash, Is.Not.Null);

        var block = await GetCasperService().GetBlock(matchingBlockHash);

        Assert.That(block, Is.Not.Null);

        var transferHashes = block.Parse().Block.Body.TransferHashes;
        
        Assert.That(transferHashes, Contains.Item(deployResult.Parse().DeployHash));
        
    }
    
}
