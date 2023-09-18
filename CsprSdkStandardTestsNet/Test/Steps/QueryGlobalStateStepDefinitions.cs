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

[Binding]
public class QueryGlobalStateStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
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

        var blockHash = _contextMap.Get<BlockAdded>(StepConstants.LAST_BLOCK_ADDED).BlockHash;
        var globalStateData = await GetCasperService().QueryGlobalState(blockHash);

        Assert.That(globalStateData, Is.Not.Null);
        
        _contextMap.Add(StepConstants.GLOBAL_STATE_DATA, globalStateData);
        
        // final Digest blockHash = ((BlockAdded) contextMap.get(LAST_BLOCK_ADDED)).getBlockHash();
        // final BlockHashIdentifier globalStateIdentifier = new BlockHashIdentifier(blockHash.toString());
        //
        // final DeployResult deployResult = contextMap.get(DEPLOY_RESULT);
        // final String key = "deploy-" + deployResult.getDeployHash();
        //
        // final GlobalStateData globalStateData = casperService.queryGlobalState(globalStateIdentifier, key, new String[0]);
        // assertThat(globalStateData, is(notNullValue()));
        //
        // contextMap.put(GLOBAL_STATE_DATA, globalStateData);


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

        // final GlobalStateData globalStateData = contextMap.get(GLOBAL_STATE_DATA);
        // assertThat(globalStateData, is(notNullValue()));
        // assertThat(globalStateData.getApiVersion(), is("1.0.0"));
        // assertThat(globalStateData.getMerkleProof(), is(notNullValue()));
        // assertThat(globalStateData.getHeader().getTimeStamp(), is(notNullValue()));
        // assertThat(globalStateData.getHeader().getEraId(), is(greaterThan(0L)));
        // assertThat(globalStateData.getHeader().getAccumulatedSeed().isValid(), is(true));
        // assertThat(globalStateData.getHeader().getBodyHash().isValid(), is(true));
        // assertThat(globalStateData.getHeader().getParentHash().isValid(), is(true));
        //

    }

    [Then(@"the query_global_state_result contains a valid deploy info stored value")]
    public void ThenTheQueryGlobalStateResultContainsAValidDeployInfoStoredValue() {
        WriteLine("the query_global_state_result contains a valid deploy info stored value");
        
    }

    [Then(@"the query_global_state_result's stored value from is the user-(.*) account hash")]
    public void ThenTheQueryGlobalStateResultsStoredValueFromIsTheUserAccountHash(int user) {
        WriteLine("the query_global_state_result's stored value from is the user-{0} account hash", user);
        
    }

    [Then(@"the query_global_state_result's stored value contains a gas price of (.*)")]
    public void ThenTheQueryGlobalStateResultsStoredValueContainsAGasPriceOf(int gasPrice) {
        WriteLine("the query_global_state_result's stored value contains a gas price of {0}", gasPrice);
        
    }

    [Then(@"the query_global_state_result stored value contains the transfer hash")]
    public void ThenTheQueryGlobalStateResultStoredValueContainsTheTransferHash() {
        WriteLine("the query_global_state_result stored value contains the transfer hash");
        
    }

    [Then(@"the query_global_state_result stored value contains the transfer source uref")]
    public void ThenTheQueryGlobalStateResultStoredValueContainsTheTransferSourceUref() {
        WriteLine("the query_global_state_result stored value contains the transfer source uref");
        
    }

    [Given(@"that the state root hash is known")]
    public void GivenThatTheStateRootHashIsKnown() {
        WriteLine("that the state root hash is known");
        
    }

    [When(@"the query_global_state RCP method is invoked with the state root hash as the query identifier and an invalid key")]
    public void WhenTheQueryGlobalStateRcpMethodIsInvokedWithTheStateRootHashAsTheQueryIdentifierAndAnInvalidKey() {
        WriteLine("the query_global_state RCP method is invoked with the state root hash as the query identifier and an invalid key");
        
    }

    [Then(@"an error code of (.*) is returned")]
    public void ThenAnErrorCodeOfIsReturned(int code) {
        WriteLine("an error code of {0} is returned", code);
        
    }

    [Then(@"an error message of ""(.*)"" is returned")]
    public void ThenAnErrorMessageOfIsReturned(string msg) {
        WriteLine("an error message of {0} is returned", msg);
        
    }

    [Given(@"the query_global_state RCP method is invoked with an invalid block hash as the query identifier")]
    public void GivenTheQueryGlobalStateRcpMethodIsInvokedWithAnInvalidBlockHashAsTheQueryIdentifier() {
        WriteLine("the query_global_state RCP method is invoked with an invalid block hash as the query identifier");
        
    }

    private async Task CreateTransfer(){

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
        
        Assert.That(transferHashes, Contains.Value(deployResult.Parse().DeployHash));
        
    }
    
    
}
