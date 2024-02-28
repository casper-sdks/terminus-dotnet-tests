using System.Linq;
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
 * State get account info step definitions
 */

[Binding]
public class StateGetAccountInfoStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private static readonly TestProperties TestProperties = new();
    private readonly NodeClient _nodeClient = new(TestProperties.DockerName);
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the state_get_account_info RCP method is invoked against nctl")]
    public async Task GivenThatTheStateGetAccountInfoRcpMethodIsInvokedAgainstNctl() {
        WriteLine("that the state_get_account_info RCP method is invoked against nctl");

        var hexPublicKey = GetUserOneHexPublicKey();
        var block = await GetCasperService().GetBlock();

        var stateAccountInfo = await GetCasperService().GetAccountInfo(hexPublicKey, block.Parse().Block.Hash);

        _contextMap.Add(StepConstants.STATE_ACCOUNT_INFO, stateAccountInfo);
        
    }

    [Then(@"a valid state_get_account_info_result is returned")]
    public void ThenAValidStateGetAccountInfoResultIsReturned() {
        WriteLine("a valid state_get_account_info_result is returned");
        
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);
        Assert.That(stateAccountInfo, Is.Not.Null);
        
    }

    [Then(@"the state_get_account_info_result contain a valid account hash")]
    public void ThenTheStateGetAccountInfoResultContainAValidAccountHash() {
        WriteLine("the state_get_account_info_result contain a valid account hash");
        
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);
        var expectedAccountHash = _nodeClient.GetAccountHash(1);
        
        Assert.That(stateAccountInfo.Parse().Account.AccountHash.ToString().ToUpper(), Is.EqualTo(expectedAccountHash.ToUpper()));
        
    }

    [Then(@"the state_get_account_info_result contain a valid action thresholds")]
    public void ThenTheStateGetAccountInfoResultContainAValidActionThresholds() {
        WriteLine("the state_get_account_info_result contain a valid action thresholds");
        
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);
        var userAccountJson = _nodeClient.GetUserAccount(1);
        var deployment = stateAccountInfo.Parse().Account.ActionThresholds;
        
        Assert.That(deployment, Is.Not.Null);
        Assert.That(deployment.Deployment.ToString(), 
            Is.EqualTo(userAccountJson["action_thresholds"]!["deployment"]!.ToString()));
        Assert.That(deployment.KeyManagement.ToString(), 
            Is.EqualTo(userAccountJson["action_thresholds"]!["key_management"]!.ToString()));

    }

    [Then(@"the state_get_account_info_result contain a valid main purse uref")]
    public void ThenTheStateGetAccountInfoResultContainAValidMainPurseUref() {
        WriteLine("the state_get_account_info_result contain a valid main purse uref");

        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);
        var accountMainPurse = _nodeClient.GetAccountMainPurse(1);
        
        Assert.That(stateAccountInfo.Parse().Account.MainPurse.ToString().ToUpper(), Is.EqualTo(accountMainPurse.ToUpper()));

    }

    [Then(@"the state_get_account_info_result contain a valid merkle proof")]
    public void ThenTheStateGetAccountInfoResultContainAValidMerkleProof() {
        WriteLine("the state_get_account_info_result contain a valid merkle proof");
        
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);
        
        Assert.That(stateAccountInfo.Parse().MerkleProof, Is.Not.Null);
        
        /*
         * The CCTL node does not return the Merkle Proof
         * Test no longer valid
         */

        // var merkleProof = _nodeClient.GetAccountMerkelProof(1);
        // Assert.That(stateAccountInfo.Parse().MerkleProof.Length,
        //     Is.EqualTo(int.Parse(merkleProof.Split(" ")[0][1..])));

    }

    [Then(@"the state_get_account_info_result contain a valid associated keys")]
    public void ThenTheStateGetAccountInfoResultContainAValidAssociatedKeys() {
        WriteLine("the state_get_account_info_result contain a valid associated keys");
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);

        var expectedAccountHash = _nodeClient.GetAccountHash(1);
        
        Assert.That(stateAccountInfo.Parse().Account.AssociatedKeys.First().AccountHash.ToString()!.ToUpper(),
            Is.EqualTo(expectedAccountHash.ToUpper()));
        
        Assert.That(stateAccountInfo.Parse().Account.AssociatedKeys.First().Weight, Is.EqualTo(1));
        
    }

    [Then(@"the state_get_account_info_result contain a valid named keys")]
    public void ThenTheStateGetAccountInfoResultContainAValidNamedKeys() {
        WriteLine("the state_get_account_info_result contain a valid named keys");
        
        var stateAccountInfo = _contextMap.Get<RpcResponse<GetAccountInfoResult>>(StepConstants.STATE_ACCOUNT_INFO);

        var userAccountJson = _nodeClient.GetUserAccount(1);
        Assert.That(stateAccountInfo.Parse().Account.NamedKeys.Count, 
            Is.EqualTo(userAccountJson["named_keys"]!.AsArray().Count));

    }

    private static string GetUserOneHexPublicKey() {

        var pem = AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem");
        var key = KeyPair.FromPem(pem);

        Assert.IsNotNull(key);

        return key.PublicKey.ToAccountHex();

    }
    
}
