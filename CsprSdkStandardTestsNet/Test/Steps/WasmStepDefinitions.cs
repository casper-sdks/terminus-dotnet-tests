using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
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
        // Assert.That(wasmBytes.Length, Is.EqualTo(189336));
        
        var chainName = "casper-net-1";
        var payment = BigInteger.Parse("200000000000");
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

        var paymentArgs = new List<NamedArg>{ 
            new ("amount", CLValue.U512(payment)), 
            new ("token_decimals", CLValue.U8(tokenDecimals)), 
            new ("token_name", CLValue.String(tokenName)),
            new ("token_symbol", CLValue.String(tokenSymbol)), 
            new ("token_total_supply", CLValue.U256(tokenTotalSupply)) 
        };
        
        var deploy = DeployTemplates.ContractDeploy(
            wasmBytes,
            faucetKey.PublicKey,
            payment,
            chainName,
            1, 
            (ulong)TimeSpan.FromMinutes(30).TotalMilliseconds); 
        
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
    public void ThenTheAccountNamedKeysContainTheName(string name) {
        WriteLine("the account named keys contain the {0} name", name);
    }

    [Then(@"the contract data ""(.*)"" is a ""(.*)"" with a value of ""(.*)"" and bytes of ""(.*)""")]
    public void ThenTheContractDataIsAWithAValueOfAndBytesOf(string data, string type, string value, string bytes) {
        WriteLine("the contract data {0} is a {1} with a value of {2} and bytes of {3}", data, type, value, bytes);
        
    }

    [When(@"the contract entry point is invoked by hash with a transfer amount of ""(.*)""")]
    public void WhenTheContractEntryPointIsInvokedByHashWithATransferAmountOf(string amount) {
        WriteLine("the contract entry point is invoked by hash with a transfer amount of {0}", amount);
    }

    [Then(@"the contract invocation deploy is successful")]
    public void ThenTheContractInvocationDeployIsSuccessful() {
        WriteLine("the contract invocation deploy is successful");
    }

    [When(@"the the contract is invoked by name ""(.*)"" and a transfer amount of ""(.*)""")]
    public void WhenTheTheContractIsInvokedByNameAndATransferAmountOf(string name, string amount) {
        WriteLine("the the contract is invoked by name {0} and a transfer amount of {1}", name, amount);
    }

    [When(@"the the contract is invoked by hash and version with a transfer amount of ""(.*)""")]
    public void WhenTheTheContractIsInvokedByHashAndVersionWithATransferAmountOf(string amount) {
        WriteLine("the the contract is invoked by hash and version with a transfer amount of {0}", amount);
    }

    [When(@"the the contract is invoked by name ""(.*)"" and version with a transfer amount of ""(.*)""")]
    public void WhenTheTheContractIsInvokedByNameAndVersionWithATransferAmountOf(string name, string amount) {
        WriteLine("the the contract is invoked by name {0} and version with a transfer amount of {1}", name, amount);
    }
}
