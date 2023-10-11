using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * wasm contract step definitions
 */
[Binding]
public class WasmStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a smart contract ""(.*)"" is located in the ""(.*)"" folder")]
    public void GivenThatASmartContractIsLocatedInTheFolder(string contract, string folder) {
        WriteLine("that a smart contract {0} is located in the {1} folder", contract, folder);
        
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!;
        var wasmPath = directoryInfo.Parent!.FullName + "/Test/" + folder + "/" + contract;
        
        Assert.That(File.Exists(wasmPath), Is.True);
        
        _contextMap.Add(StepConstants.WASM_PATH, wasmPath);
        
    }

    [When(@"the wasm is loaded as from the file system")]
    public async Task WhenTheWasmIsLoadedAsFromTheFileSystem() {
        WriteLine("the wasm is loaded as from the file system");

        var wasmBytes = File.ReadAllBytes(_contextMap.Get<string>(StepConstants.WASM_PATH));
        Assert.That(wasmBytes.Length, Is.EqualTo(189336));
        
        var chainName = "casper-net-1";
        var paymentAmount = BigInteger.Parse("200000000000");
        byte tokenDecimals = 11;
        var tokenName = "Acme Token";
        var tokenTotalSupply = BigInteger.Parse("500000000000");
        var tokenSymbol = "ACME";
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);

        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);
        
        _contextMap.Add(StepConstants.FAUCET_PRIVATE_KEY, faucetKey);

        var runtimeArgs = new List<NamedArg>{ 
            new ("token_decimals", CLValue.U8(tokenDecimals)), 
            new ("token_name", CLValue.String(tokenName)),
            new ("token_symbol", CLValue.String(tokenSymbol)), 
            new ("token_total_supply", CLValue.U256(tokenTotalSupply)) 
        };
        
        var header = new DeployHeader(){
            Account = faucetKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = (ulong) TimeSpan.FromMinutes(30).TotalMilliseconds,
            ChainName = chainName,
            GasPrice = 1
        };

        var session = new ModuleBytesDeployItem(wasmBytes, runtimeArgs);
        var payment = new ModuleBytesDeployItem(paymentAmount);
        var deploy = new Deploy(header, payment, session);       
       
        deploy.Sign(faucetKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);

        _contextMap.Add(StepConstants.DEPLOY_RESULT, putResponse);

    }

    [When(@"the wasm has been successfully deployed")]
    public async Task WhenTheWasmHasBeenSuccessfullyDeployed() {
        WriteLine("the wasm has been successfully deployed");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);

        RpcResponse<GetDeployResult> deployData = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
        
        Assert.That(deployData, Is.Not.Null);
        Assert.That(deployData.Parse().Deploy, Is.Not.Null);
        Assert.That(deployData!.Parse().ExecutionResults[0].IsSuccess);

    }

    [Then(@"the account named keys contain the ""(.*)"" name")]
    public async Task ThenTheAccountNamedKeysContainTheName(string name) {
        WriteLine("the account named keys contain the {0} name", name);

        var publicKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY).PublicKey;
        var accountHash = publicKey.GetAccountHash();
        var stateRootHash = await GetCasperService().GetStateRootHash();
        _contextMap.Add(StepConstants.STATE_ROOT_HASH, stateRootHash);
        
        var stateItem = await GetCasperService().QueryGlobalState(accountHash, stateRootHash);
        
        Assert.That(stateItem, Is.Not.Null);
        Assert.That(stateItem.Parse().StoredValue, Is.Not.Null);

        var account = stateItem.Parse().StoredValue.Account;
        
        Assert.That(account.AssociatedKeys, Is.Not.Empty);
        
        foreach (var key in account.NamedKeys) {
            Assert.That(key.Name.ToUpper().StartsWith(name.ToUpper()));
            if (key.Key.ToString()!.StartsWith("hash")) {
                _contextMap.Add(StepConstants.CONTRACT_HASH, key.Key);
            }
        }

    }

    [Then(@"the contract data ""(.*)"" is a ""(.*)"" with a value of ""(.*)"" and bytes of ""(.*)""")]
    public async Task ThenTheContractDataIsAWithAValueOfAndBytesOf(string path, CLType typeName, string value, string hexBytes) {
        WriteLine("the contract data {0} is a {1} with a value of {2} and bytes of {3}", path, typeName, value, hexBytes);

        var stateRootHash = _contextMap.Get<string>(StepConstants.STATE_ROOT_HASH);
        var contractHash = _contextMap.Get<GlobalStateKey>(StepConstants.CONTRACT_HASH);
        
        var stateItem = await GetCasperService().QueryGlobalState(contractHash, stateRootHash, path);

        var clValue = stateItem.Parse().StoredValue.CLValue;
        
        Assert.That(clValue.TypeInfo.Type, Is.EqualTo(typeName));

        var expectedValue = CLTypeUtils.ConvertToClTypeValue(typeName, value);
        Assert.That(clValue.Parsed.ToString(), Is.EqualTo(expectedValue.ToString()));
        Assert.That(GetHexValue(clValue), Is.EqualTo(hexBytes.ToUpper()));

    }

    [When(@"the contract entry point is invoked by hash with a transfer amount of ""(.*)""")]
    public async Task WhenTheContractEntryPointIsInvokedByHashWithATransferAmountOf(string transferAmount) {
        WriteLine("the contract entry point is invoked by hash with a transfer amount of {0}", transferAmount);

        var recipient = KeyPair.CreateNew(KeyAlgo.ED25519).PublicKey;
        var amount = BigInteger.Parse(transferAmount);
        var contractHash = _contextMap.Get<GlobalStateKey>(StepConstants.CONTRACT_HASH).ToString()![5..];

        var faucetPrivateKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY);
        var accountHash = recipient.GetAccountHash();
        
        var args = new List<NamedArg> {
            new("recipient", CLValue.ByteArray(Hex.Decode(accountHash[13..]))),
            new("amount", CLValue.U256(amount))
        };

        var session = new StoredContractByHashDeployItem(contractHash, "transfer", args);
        var payment = new ModuleBytesDeployItem(amount);
        
        var header = new DeployHeader(){
            Account = faucetPrivateKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = (ulong) TimeSpan.FromMinutes(30).TotalMilliseconds,
            ChainName =  "casper-net-1",
            GasPrice = 1
        };
        
        var deploy = new Deploy(header, payment, session);       
       
        deploy.Sign(faucetPrivateKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);

        _contextMap.Add(StepConstants.TRANSFER_DEPLOY, putResponse);

    }

    [Then(@"the contract invocation deploy is successful")]
    public async Task ThenTheContractInvocationDeployIsSuccessful() {
        WriteLine("the contract invocation deploy is successful");

        var transferDeploy  = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.TRANSFER_DEPLOY);

        var deployData = await GetCasperService().GetDeploy(
            transferDeploy.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
        
        Assert.That(deployData, Is.Not.Null);
        Assert.That(deployData.Parse().Deploy, Is.Not.Null);
        Assert.That(deployData!.Parse().ExecutionResults[0].IsSuccess);
        
    }

    [When(@"the the contract is invoked by name ""(.*)"" and a transfer amount of ""(.*)""")]
    public async Task WhenTheTheContractIsInvokedByNameAndATransferAmountOf(string name, string transferAmount) {
        WriteLine("the the contract is invoked by name {0} and a transfer amount of {1}", name, transferAmount);

        var recipient = KeyPair.CreateNew(KeyAlgo.ED25519).PublicKey;
        var amount = BigInteger.Parse(transferAmount);
        var faucetPrivateKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY);
        var accountHash = recipient.GetAccountHash();
        
        var args = new List<NamedArg> {
            new("recipient", CLValue.ByteArray(Hex.Decode(accountHash[13..]))),
            new("amount", CLValue.U256(amount))
        };

        var session = new StoredContractByNameDeployItem(name, "transfer", args);
        var payment = new ModuleBytesDeployItem(amount);
        
        var header = new DeployHeader(){
            Account = faucetPrivateKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = (ulong) TimeSpan.FromMinutes(30).TotalMilliseconds,
            ChainName =  "casper-net-1",
            GasPrice = 1
        };
        
        var deploy = new Deploy(header, payment, session);       
       
        deploy.Sign(faucetPrivateKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);

        _contextMap.Add(StepConstants.TRANSFER_DEPLOY, putResponse);
        
    }

    [When(@"the the contract is invoked by hash and version with a transfer amount of ""(.*)""")]
    public async Task WhenTheTheContractIsInvokedByHashAndVersionWithATransferAmountOf(string transferAmount) {
        WriteLine("the the contract is invoked by hash and version with a transfer amount of {0}", transferAmount);

        var recipient = KeyPair.CreateNew(KeyAlgo.ED25519).PublicKey;
        var amount = BigInteger.Parse(transferAmount);
        var faucetPrivateKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY);
        var accountHash = recipient.GetAccountHash();
        var contractHash = _contextMap.Get<GlobalStateKey>(StepConstants.CONTRACT_HASH).ToString()![5..];
        
        var args = new List<NamedArg> {
            new("recipient", CLValue.ByteArray(Hex.Decode(accountHash[13..]))),
            new("amount", CLValue.U256(amount))
        };

        var session = new StoredVersionedContractByHashDeployItem(contractHash, 1, "transfer", args);
        var payment = new ModuleBytesDeployItem(amount);
        
        var header = new DeployHeader(){
            Account = faucetPrivateKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = (ulong) TimeSpan.FromMinutes(30).TotalMilliseconds,
            ChainName =  "casper-net-1",
            GasPrice = 1
        };
        
        var deploy = new Deploy(header, payment, session);       
       
        deploy.Sign(faucetPrivateKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);

        _contextMap.Add(StepConstants.TRANSFER_DEPLOY, putResponse);
        
    }

    [When(@"the the contract is invoked by name ""(.*)"" and version with a transfer amount of ""(.*)""")]
    public async Task WhenTheTheContractIsInvokedByNameAndVersionWithATransferAmountOf(string name, string transferAmount) {
        WriteLine("the the contract is invoked by name {0} and version with a transfer amount of {1}", name, transferAmount);
      
        var recipient = KeyPair.CreateNew(KeyAlgo.ED25519).PublicKey;
        var amount = BigInteger.Parse(transferAmount);
        var faucetPrivateKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY);
        var accountHash = recipient.GetAccountHash();
        
        var args = new List<NamedArg> {
            new("recipient", CLValue.ByteArray(Hex.Decode(accountHash[13..]))),
            new("amount", CLValue.U256(amount))
        };

        var session = new StoredVersionedContractByNameDeployItem(name.ToUpper(), 1, "transfer", args);
        var payment = new ModuleBytesDeployItem(amount);
        
        var header = new DeployHeader(){
            Account = faucetPrivateKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = (ulong) TimeSpan.FromMinutes(30).TotalMilliseconds,
            ChainName =  "casper-net-1",
            GasPrice = 1
        };
        
        var deploy = new Deploy(header, payment, session);       
       
        deploy.Sign(faucetPrivateKey);

        var putResponse = await GetCasperService().PutDeploy(deploy);

        _contextMap.Add(StepConstants.TRANSFER_DEPLOY, putResponse);
    }

    [Then(@"the contract dictionary item ""(.*)"" is a ""(.*)"" with a value of ""(.*)"" and bytes of ""(.*)""")]
    public async Task ThenTheContractDictionaryItemIsAWithAValueOfAndBytesOf(string dictionary, string type, string value, string bytes) {
        WriteLine("the contract dictionary item {0} is a {1} with a value of {2} and bytes of {3}", dictionary, type, value, bytes);

        var stateRootHash = _contextMap.Get<string>(StepConstants.STATE_ROOT_HASH);
        var contractHash = _contextMap.Get<GlobalStateKey>(StepConstants.CONTRACT_HASH);
        var faucetPrivateKey = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY);

        var clValuePublicKey = CLValue.PublicKey(faucetPrivateKey.PublicKey);
        // var decode = Hex.Decode(clValuePublicKey.Bytes);
        // var encode = Base64.Encode(decode);
        // var balanceKey = Hex.Encode(encode);

        var accountInfo = await GetCasperService().GetAccountInfo(faucetPrivateKey.PublicKey);

        // var stateDictionaryItem = await GetCasperService()
        //     .GetDictionaryItemByContract(accountInfo.Parse().Account.NamedKeys.First().Key.ToString(), dictionary, contractHash.ToString(), stateRootHash);
        
       var stateDictionaryItem = await GetCasperService()
            .GetDictionaryItemByContract(contractHash.ToString(), "balances", "name");

       // var stateDictionaryItem = await GetCasperService()
       //     .GetDictionaryItemByURef(accountInfo.Parse().Account.NamedKeys[1].Key.ToString(), dictionary);

    }
    
    private string GetHexValue(CLValue clValue) {

        var clValueHex = BitConverter.ToString(clValue.Bytes).Replace("-", "");
        
        if (clValue.TypeInfo.Type.Equals(CLType.Key)) {
            clValueHex = clValueHex[2..];
        }

        return clValueHex;

    }
}
