using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using CsprSdkStandardTestsNet.Test.Tasks;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * Block steps
 */
[Binding]
public class Blocks {
    
    private const string InvalidBlockHash = "2fe9630b7790852e4409d815b04ca98f37effcdf9097d317b9b9b8ad658f47c8";
    private const long InvalidHeight = 9999999999;
    private const string BlockErrorMsg = "No such block";
    private const string BlockErrorCode = "-32001";

    private static readonly TestProperties TestProperties = new();
    private readonly Dictionary<string, object> _contextMap = new();
    private readonly Nctl _nctl = new(TestProperties.DockerName);

    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [Given(@"that the latest block is requested via the sdk")]
    public async Task GivenThatTheLatestBlockIsRequestedViaTheSdk() {
        WriteLine("that the latest block is requested via the sdk");

        var rpcResponse = await GetCasperService().GetBlock();

        _contextMap.Add("blockDataSdk", rpcResponse.Parse().Block);
        _contextMap.Add("blockHashSdk", rpcResponse.Parse().Block.Hash);
    }

    [Then(@"request the latest block via the test node")]
    public void ThenRequestTheLatestBlockViaTheTestNode() {
        WriteLine("request the latest block via the test node");

        _contextMap.Add("blockDataNode", _nctl.GetChainBlock(""));
    }

    [Then(@"the body of the returned block is equal to the body of the returned test node block")]
    public async Task ThenTheBodyOfTheReturnedBlockIsEqualToTheBodyOfTheReturnedTestNodeBlock() {
        WriteLine("the body of the returned block is equal to the body of the returned test node block");

        var latestBlockSdk = (Block)_contextMap["blockDataSdk"];
        var latestBlockNode = (JsonNode)_contextMap["blockDataNode"];

        if (!latestBlockSdk.Hash.Equals(latestBlockNode["hash"]?.ToString().ToLower())) {
            //Fixes intermittent syncing issues with nctl/sdk latest blocks
            var rpcResponse = await GetCasperService().GetBlock();
            _contextMap["blockDataSdk"] = rpcResponse.Parse().Block;
        }

        Assert.That(latestBlockSdk.Body, Is.Not.Null);
        Assert.That(latestBlockSdk.Body.Proposer.ToString()!.ToLower(),
            Is.EqualTo(latestBlockNode["body"]!["proposer"]?.ToString().ToLower()));

        Assert.That(latestBlockSdk.Body, Is.Not.Null);
        Assert.That(latestBlockSdk.Body.Proposer.ToString()!.ToLower(),
            Is.EqualTo(latestBlockNode["body"]!["proposer"]?.ToString().ToLower()));

        if (((JsonArray)latestBlockNode["body"]["deploy_hashes"])!.Count == 0)
            Assert.That(latestBlockSdk.Body.DeployHashes, Is.Empty);
        else
            foreach (var d in (JsonArray)latestBlockNode["body"]["deploy_hashes"])
                Assert.Contains(d.ToString().ToLower(),
                    new[] { latestBlockSdk.Body.DeployHashes.ToString()!.ToLower() });

        if (((JsonArray)latestBlockNode["body"]["transfer_hashes"])!.Count == 0)
            Assert.That(latestBlockSdk.Body.TransferHashes, Is.Empty);
        else
            foreach (var d in (JsonArray)latestBlockNode["body"]["transfer_hashes"])
                Assert.Contains(d.ToString().ToLower(),
                    new[] { latestBlockSdk.Body.TransferHashes.ToString()!.ToLower() });
    }

    [Then(@"the hash of the returned block is equal to the hash of the returned test node block")]
    public void ThenTheHashOfTheReturnedBlockIsEqualToTheHashOfTheReturnedTestNodeBlock() {
        WriteLine("the hash of the returned block is equal to the hash of the returned test node block");

        var latestBlockSdk = (Block)_contextMap["blockDataSdk"];
        var latestBlockNode = (JsonNode)_contextMap["blockDataNode"];

        Assert.That(latestBlockSdk.Hash, Is.EqualTo(latestBlockNode["hash"]!.ToString()));
    }

    [Then(@"the header of the returned block is equal to the header of the returned test node block")]
    public void ThenTheHeaderOfTheReturnedBlockIsEqualToTheHeaderOfTheReturnedTestNodeBlock() {
        WriteLine("the header of the returned block is equal to the header of the returned test node block");

        var latestBlockSdk = (Block)_contextMap["blockDataSdk"];
        var latestBlockNode = (JsonNode)_contextMap["blockDataNode"];

        Assert.That(latestBlockSdk.Header.EraId,
            Is.EqualTo(int.Parse(latestBlockNode["header"]!["era_id"]?.ToString()!)));
        Assert.That(latestBlockSdk.Header.Height,
            Is.EqualTo(int.Parse(latestBlockNode["header"]!["height"]?.ToString()!)));
        Assert.That(latestBlockSdk.Header.ProtocolVersion,
            Is.EqualTo(latestBlockNode["header"]!["protocol_version"]?.ToString()));

        Assert.That(latestBlockSdk.Header.AccumulatedSeed,
            Is.EqualTo(latestBlockNode["header"]!["accumulated_seed"]?.ToString()));
        Assert.That(latestBlockSdk.Header.BodyHash, Is.EqualTo(latestBlockNode["header"]!["body_hash"]?.ToString()));
        Assert.That(latestBlockSdk.Header.StateRootHash,
            Is.EqualTo(latestBlockNode["header"]!["state_root_hash"]?.ToString()));
        Assert.That(latestBlockSdk.Header.Timestamp, Is.EqualTo(latestBlockNode["header"]!["timestamp"]?.ToString()));
    }

    [Then(@"the proofs of the returned block are equal to the proofs of the returned test node block")]
    public void ThenTheProofsOfTheReturnedBlockAreEqualToTheProofsOfTheReturnedTestNodeBlock() {
        WriteLine("the proofs of the returned block are equal to the proofs of the returned test node block");

        var latestBlockSdk = (Block)_contextMap["blockDataSdk"];
        var latestBlockNode = (JsonNode)_contextMap["blockDataNode"];

        Assert.That(((JsonArray)latestBlockNode["proofs"])!.Count, Is.EqualTo(latestBlockSdk.Proofs.Count));

        var proofsNode = (JsonArray)latestBlockNode["proofs"];

        foreach (var proofSdk in latestBlockSdk.Proofs)
        {
            Assert.That(proofsNode.Where(p => proofSdk.Signature != null &&
                                              p["signature"]!.ToString().ToLower()
                                                  .Equals(proofSdk.Signature.ToString()!.ToLower())),
                Is.Not.Empty);

            Assert.That(proofsNode.Where(p => proofSdk.PublicKey != null &&
                                              p["public_key"]!.ToString().ToLower()
                                                  .Equals(proofSdk.PublicKey.ToString()!.ToLower())),
                Is.Not.Empty);
        }
    }

    [Given(@"that a block is returned by height (.*) via the sdk")]
    public async Task GivenThatABlockIsReturnedByHeightViaTheSdk(int height) {
        WriteLine("that a block is returned by height {0} via the sdk", height);

        var rpcResponse = await GetCasperService().GetBlock(height);

        _contextMap["blockDataSdk"] = rpcResponse.Parse().Block;
        _contextMap["blockHashSdk"] = rpcResponse.Parse().Block.Hash;
    }

    [Then(@"request the returned block from the test node via its hash")]
    public void ThenRequestTheReturnedBlockFromTheTestNodeViaItsHash() {
        WriteLine("request the returned block from the test node via its hash");

        _contextMap["blockDataNode"] = _nctl.GetChainBlock(_contextMap["blockHashSdk"].ToString());
    }

    [Given(@"that a block is returned by hash via the sdk")]
    public async Task GivenThatABlockIsReturnedByHashViaTheSdk() {
        WriteLine("that a block is returned by hash via the sdk");

        var rpcResponse = await GetCasperService().GetBlock();

        _contextMap.Add("latestBlock", rpcResponse.Parse().Block.Hash);

        rpcResponse = await GetCasperService().GetBlock(_contextMap["latestBlock"].ToString());

        _contextMap["blockDataSdk"] = rpcResponse.Parse().Block;
    }

    [Then(@"request a block by hash via the test node")]
    public void ThenRequestABlockByHashViaTheTestNode() {
        WriteLine("request a block by hash via the test node");

        _contextMap["blockDataNode"] = _nctl.GetChainBlock(_contextMap["latestBlock"].ToString());
    }

    [Given(@"that an invalid block hash is requested via the sdk")]
    public void GivenThatAnInvalidBlockHashIsRequestedViaTheSdk() {
        WriteLine("that an invalid block hash is requested via the sdk");

        _contextMap["rpcClientException"] =
            Assert.ThrowsAsync<RpcClientException>(() => GetCasperService().GetBlock(InvalidBlockHash));
    }

    [Then(@"a valid error message is returned")]
    public void ThenAValidErrorMessageIsReturned() {
        WriteLine("a valid error message is returned");

        var rpcClientException = (RpcClientException)_contextMap["rpcClientException"];

        Assert.That(rpcClientException.RpcError, Is.Not.Null);
        Assert.That(rpcClientException.RpcError.Code.ToString(), Is.EqualTo(BlockErrorCode));
        Assert.That(rpcClientException.RpcError.Message, Is.EqualTo(BlockErrorMsg));
    }

    [Given(@"that an invalid block height is requested via the sdk")]
    public void GivenThatAnInvalidBlockHeightIsRequestedViaTheSdk() {
        WriteLine("that an invalid block height is requested via the sdk");

        _contextMap["rpcClientException"] =
            Assert.ThrowsAsync<RpcClientException>(() => GetCasperService().GetBlock(unchecked((int)InvalidHeight)));
    }

    [Given(@"that chain transfer data is initialised")]
    public void GivenThatChainTransferDataIsInitialised() {
        WriteLine("that chain transfer data is initialised");

        var senderPem = AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem");
        var senderKey = KeyPair.FromPem(senderPem);

        Assert.IsNotNull(senderKey);

        var receiverPem = AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem");

        var receiverKey = PublicKey.FromPem(receiverPem);

        Assert.IsNotNull(receiverKey);

        _contextMap["senderKey"] = senderKey;
        _contextMap["receiverKey"] = receiverKey;
        _contextMap["transferAmount"] = new BigInteger(2500000000);
        _contextMap["gasPrice"] = 1;
        _contextMap["ttl"] = "30m";
    }

    [When(@"the deploy data is put on chain")]
    public async Task WhenTheDeployDataIsPutOnChain() {
        WriteLine("the deploy data is put on chain");

        var senderKey = (KeyPair)_contextMap["senderKey"];
        
        var deploy = DeployTemplates.StandardTransfer(
            senderKey.PublicKey,
            (PublicKey)_contextMap["receiverKey"],
            (BigInteger)_contextMap["transferAmount"],
            100_000_000,
            "casper-net-1");
        
        deploy.Sign(senderKey);
        
        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        _contextMap["deployResult"] = putResponse;
        
        WriteLine(putResponse);
    }

    [Then(@"the deploy response contains a valid deploy hash")]
    public void ThenTheDeployResponseContainsAValidDeployHash() {
        WriteLine("the deploy response contains a valid deploy hash");

        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];
        
        Assert.IsNotNull(deployResult);
        Assert.IsNotNull(deployResult.Parse().DeployHash);
        
        WriteLine(deployResult.Parse().DeployHash);
    }

    [Then(@"request the block transfer")]
    public async Task ThenRequestTheBlockTransfer() {
        WriteLine("request the block transfer");

        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];
        
        var sseBlockAdded = new BlockAddedTask();
        sseBlockAdded.HasTransferHashWithin(deployResult.Parse().DeployHash, 300000);
        

    }

    [Then(@"request the block transfer from the test node")]
    public void ThenRequestTheBlockTransferFromTheTestNode() {
        WriteLine("request the block transfer from the test node");
        
        var transferData = (RpcResponse<GetBlockTransfersResult>)_contextMap["transferBlockSdk"];
        _contextMap["transferBlockNode"] = _nctl.GetChainBlockTransfers(transferData.Parse().BlockHash);
    }

    [Then(@"the returned block contains the transfer hash returned from the test node block")]
    public void ThenTheReturnedBlockContainsTheTransferHashReturnedFromTheTestNodeBlock() {
        WriteLine("the returned block contains the transfer hash returned from the test node block");

        var transfers = (JsonNode)_contextMap["transferBlockNode"];
        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];
        
        Assert.That(transfers["body"]!["transfer_hashes"]!.ToString(), Does.Contain(deployResult.Parse().DeployHash));
    }
}
