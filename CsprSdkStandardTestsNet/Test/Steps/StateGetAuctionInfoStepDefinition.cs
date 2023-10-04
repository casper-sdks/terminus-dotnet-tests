using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * State get auction info step definitions
 */

[Binding]
public class StateGetAuctionInfoStepDefinition {
    
    private static readonly TestProperties TestProperties = new();
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    private readonly SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the state_get_auction_info RPC method is invoked by hash block identifier")]
    public async Task GivenThatTheStateGetAuctionInfoRpcMethodIsInvokedByHashBlockIdentifier() {
        WriteLine("that the state_get_auction_info RPC method is invoked by hash block identifier");

        var block = await GetCasperService().GetBlock();
        var parentHash = block.Parse().Block.Header.ParentHash;
        _contextMap.Add(StepConstants.PARENT_HASH, parentHash);

        var auctionInfoByHash = await _simpleRcpClient.GetAuctionInfoByHash(parentHash);
        Assert.That(auctionInfoByHash, Is.Not.Null);
        _contextMap.Add(StepConstants.STATE_AUCTION_INFO_JSON, auctionInfoByHash);

        var auctionData = await GetCasperService().GetAuctionInfo(parentHash);
        _contextMap.Add(StepConstants.STATE_GET_AUCTION_INFO_RESULT, auctionData);

    }

    [Then(@"a valid state_get_auction_info_result is returned")]
    public void ThenAValidStateGetAuctionInfoResultIsReturned() {
        WriteLine("a valid state_get_auction_info_result is returned");

        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        Assert.That(auctionData, Is.Not.Null);

    }

    [Then(@"the state_get_auction_info_result action_state has a valid state root hash")]
    public void ThenTheStateGetAuctionInfoResultActionStateHasAValidStateRootHash() {
        WriteLine("the state_get_auction_info_result action_state has a valid state root hash");
        
        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.STATE_AUCTION_INFO_JSON)["result"];
        var expectedStateRootHash = jsonNode["auction_state"]!["state_root_hash"]!.ToString();

        Assert.That(auctionData.Parse().AuctionState.StateRootHash, Is.EqualTo(expectedStateRootHash));
        
    }

    [Then(@"the state_get_auction_info_result action_state has a valid height")]
    public void ThenTheStateGetAuctionInfoResultActionStateHasAValidHeight() {
        WriteLine("the state_get_auction_info_result action_state has a valid height");
        
        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.STATE_AUCTION_INFO_JSON)["result"];
        var expectedBlockHeight = jsonNode!["auction_state"]!["block_height"]!.ToString();
        
        Assert.That(auctionData.Parse().AuctionState.BlockHeight.ToString(), Is.EqualTo(expectedBlockHeight));
        
    }

    [Then(@"the state_get_auction_info_result action_state has valid bids")]
    public void ThenTheStateGetAuctionInfoResultActionStateHasValidBids() {
        WriteLine("the state_get_auction_info_result action_state has valid bids");
        
        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        Assert.That(auctionData.Parse().AuctionState.Bids.Count, Is.GreaterThan(0));
        
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.STATE_AUCTION_INFO_JSON)["result"];
        var bidsJson = jsonNode!["auction_state"]!["bids"];
        Assert.That(auctionData.Parse().AuctionState.Bids.Count, Is.EqualTo(bidsJson!.AsArray().Count));

        var publicKey = bidsJson[0]!["public_key"];
        var firstBid = auctionData.Parse().AuctionState.Bids.First();
        Assert.That(firstBid.PublicKey.ToString().ToUpper(), Is.EqualTo(publicKey!.ToString().ToUpper()));

        var bondingPurse = bidsJson[0]!["bid"]!["bonding_purse"]!.ToString();
        Assert.That(firstBid.BondingPurse.ToString().ToUpper(), Is.EqualTo(bondingPurse.ToUpper()));

        var delegationRate = bidsJson[0]!["bid"]!["delegation_rate"]!.ToString();
        Assert.That(firstBid.DelegationRate.ToString(), Is.EqualTo(delegationRate));

        var inactive = bidsJson[0]!["bid"]!["inactive"]!.ToString();
        Assert.That(firstBid.Inactive, Is.EqualTo(bool.Parse(inactive)));

        var stakedAmount = BigInteger.Parse(bidsJson[0]!["bid"]!["staked_amount"]!.ToString());
        Assert.That(firstBid.StakedAmount, Is.EqualTo(stakedAmount));

        var delegatorBondingPurse = bidsJson[0]!["bid"]!["delegators"]![0]!["bonding_purse"]!.ToString();
        Assert.That(firstBid.Delegators.First().BondingPurse.ToString().ToUpper(), 
            Is.EqualTo(delegatorBondingPurse.ToUpper()));

        var delegatee = bidsJson[0]!["bid"]!["delegators"]![0]!["delegatee"]!.ToString();
        Assert.That(firstBid.Delegators.First().Delegatee.ToString().ToUpper(),
            Is.EqualTo(delegatee.ToUpper()));

        var delegateePublicKey = bidsJson[0]!["bid"]!["delegators"]![0]!["public_key"]!.ToString();
        Assert.That(firstBid.Delegators.First().PublicKey.ToString().ToUpper(), 
            Is.EqualTo(delegateePublicKey!.ToUpper()));

        var delegateeStakedAmount =bidsJson[0]!["bid"]!["delegators"]![0]!["staked_amount"]!.ToString();
        Assert.That(firstBid.Delegators.First().StakedAmount.ToString(), Is.EqualTo(delegateeStakedAmount));

    }

    [Given(@"that the state_get_auction_info RPC method is invoked by height block identifier")]
    public async Task GivenThatTheStateGetAuctionInfoRpcMethodIsInvokedByHeightBlockIdentifier() {
        WriteLine("that the state_get_auction_info RPC method is invoked by height block identifier");
        
        var currentBlock = await GetCasperService().GetBlock();
        var parentHash = currentBlock.Parse().Block.Header.ParentHash;
        var block = await GetCasperService().GetBlock(parentHash);

        var stateAuctionInfoJson = await _simpleRcpClient.GetAuctionInfoByHash(parentHash);
        Assert.That(stateAuctionInfoJson, Is.Not.Null);
        _contextMap.Add(StepConstants.STATE_AUCTION_INFO_JSON, stateAuctionInfoJson);

        var auctionData = await GetCasperService().GetAuctionInfo((int)block.Parse().Block.Header.Height);
        _contextMap.Add(StepConstants.STATE_GET_AUCTION_INFO_RESULT, auctionData);

    }

    [Then(@"the state_get_auction_info_result action_state has valid era validators")]
    public void ThenTheStateGetAuctionInfoResultActionStateHasValidEraValidators() {
        WriteLine("the state_get_auction_info_result action_state has valid era validators");
        
        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        Assert.That(auctionData.Parse().AuctionState.EraValidators.Count, Is.GreaterThan(0));
        
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.STATE_AUCTION_INFO_JSON)["result"];
        var eraValidatorsJson = jsonNode!["auction_state"]!["era_validators"];
        Assert.That(eraValidatorsJson!.AsArray().Count, Is.GreaterThan(0));

        var firstValidatorJson = eraValidatorsJson[0];
        var firstValidator = auctionData.Parse().AuctionState.EraValidators.First();
        Assert.That(firstValidator.EraId.ToString(), Is.EqualTo(firstValidatorJson!["era_id"]!.ToString()));
        
        Assert.That(firstValidator.ValidatorWeights.Count, Is.GreaterThan(0));

        var weight = firstValidator.ValidatorWeights.First();
        
        Assert.That(weight.PublicKey.ToString().ToUpper(), 
            Is.EqualTo(firstValidatorJson["validator_weights"]![0]!["public_key"]!.ToString().ToUpper()));
        
        Assert.That(weight.Weight, 
            Is.EqualTo(BigInteger.Parse(firstValidatorJson["validator_weights"][0]["weight"]!.ToString())));

    }

    [Given(@"that the state_get_auction_info RPC method is invoked by an invalid block hash identifier")]
    public void GivenThatTheStateGetAuctionInfoRpcMethodIsInvokedByAnInvalidBlockHashIdentifier() {
        WriteLine("that the state_get_auction_info RPC method is invoked by an invalid block hash identifier");
        
        _contextMap.Add(StepConstants.CLIENT_EXCEPTION,
            Assert.ThrowsAsync<RpcClientException>(() => 
                GetCasperService().GetAuctionInfo("9608b4b7029a18ae35373eab879f523850a1b1fd43a3e6da774826a343af4ad2")));
        
    }

    [Then(@"the state_get_auction_info_result has and api version of ""(.*)""")]
    public void ThenTheStateGetAuctionInfoResultHasAndApiVersionOf(string api) {
        WriteLine("the state_get_auction_info_result has and api version of {0}", api);
        
        var auctionData =
            _contextMap.Get<RpcResponse<GetAuctionInfoResult>>(StepConstants.STATE_GET_AUCTION_INFO_RESULT);
        Assert.That(auctionData.Parse().ApiVersion, Is.EqualTo(api));

    }
    
}
