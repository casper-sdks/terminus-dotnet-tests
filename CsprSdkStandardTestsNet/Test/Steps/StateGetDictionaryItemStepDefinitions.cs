using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * State Get Dictionary Item step definitions
 */
[Binding]
public class StateGetDictionaryItemStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the state_get_dictionary_item RCP method is invoked")]
    public async Task GivenThatTheStateGetDictionaryItemRcpMethodIsInvoked() {
        WriteLine("that the state_get_dictionary_item RCP method is invoked");

        var stateRootHash = await GetCasperService().GetStateRootHash();
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        var block = await GetCasperService().GetBlock();
        var accountData = await GetCasperService().GetAccountInfo(
            faucetKey.PublicKey, block.Parse().Block.Hash);
        _contextMap.Add(StepConstants.MAIN_PURSE, accountData.Parse().Account.MainPurse);

        var accountHash = faucetKey.PublicKey.GetAccountHash();
        var key = GlobalStateKey.FromString(accountHash);
        _contextMap.Add(StepConstants.ACCOUNT_HASH, accountHash);

        var dictionaryData = await GetCasperService().GetDictionaryItem(key.ToString(), stateRootHash);
        _contextMap.Add(StepConstants.STATE_GET_DICTIONARY_ITEM, dictionaryData);
        
    }

    [Then(@"a valid state_get_dictionary_item_result is returned")]
    public void ThenAValidStateGetDictionaryItemResultIsReturned() {
        WriteLine("a valid state_get_dictionary_item_result is returned");

        var dictionaryData =
            _contextMap.Get<RpcResponse<GetDictionaryItemResult>>(StepConstants.STATE_GET_DICTIONARY_ITEM);
        Assert.That(dictionaryData.Parse(), Is.Not.Null);

        var accountHash = _contextMap.Get<string>(StepConstants.ACCOUNT_HASH);
        Assert.That(dictionaryData.Parse().DictionaryKey.ToUpper(), Is.EqualTo(accountHash.ToUpper()));

        var storedValueAccount = dictionaryData.Parse().StoredValue.Account;
        Assert.That(storedValueAccount.AccountHash.ToString().ToUpper(), Is.EqualTo(accountHash.ToUpper()));

        var mainPurse = _contextMap.Get<URef>(StepConstants.MAIN_PURSE);
        Assert.That(storedValueAccount.MainPurse.ToString().ToUpper(), Is.EqualTo(mainPurse.ToString().ToUpper()));
        
    }
    
}
