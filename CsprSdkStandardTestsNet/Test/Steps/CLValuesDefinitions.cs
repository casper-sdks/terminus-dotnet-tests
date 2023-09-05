using System;
using System.Collections.Generic;
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
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * CLTypes step definitions
 */
[Binding]
public partial class CLValuesDefinitions {
    
    private readonly Dictionary<string, object> _contextMap = new();
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a CL value of type ""(.*)"" has a value of ""(.*)""")]
    public void GivenThatAclValueOfTypeHasAValueOf(CLType typeName, string strValue) {
        WriteLine("that a CL value of type {0} has a value of {1}", typeName, strValue);

        _contextMap["clValue"] = CLValueFactory.CreateValue(typeName, strValue);;

        AddValueToContext(typeName, CLValueFactory.CreateValue(typeName, strValue));
    }

    [Then(@"it's bytes will be ""(.*)""")]
    public void ThenItsBytesWillBe(string hexBytes) {
        WriteLine("it's bytes will be {0}", hexBytes);
        
        CLValue clValue = (CLValue)_contextMap["clValue"];
        
        Assert.That(hexBytes.ToUpper(), Is.EqualTo(GetHexValue(clValue)));    

    }

    [Given(@"that the CL complex value of type ""(.*)"" with an internal types of ""(.*)"" values of ""(.*)""")]
    public void GivenThatTheClComplexValueOfTypeWithAnInternalTypesOfValuesOf(CLType type, string innerTypes, string innerValues) {
        WriteLine("that the CL complex value of type {0} with an internal types of {1} values of {2}", type, innerTypes, innerValues);
        
        var types = GetInnerClTypeData(innerTypes);
        var values = new List<string>(innerValues.Split(","));
        
        var complexValue = CLValueFactory.CreateComplexValue(type, types, values);

        AddValueToContext(type, complexValue);

    }

    [When(@"the values are added as arguments to a deploy")]
    public void WhenTheValuesAreAddedAsArgumentsToADeploy() {
        WriteLine("the values are added as arguments to a deploy");
        
        var runtimeArgs = new List<NamedArg>
            { new ("amount", CLValue.U512(BigInteger.Parse("2500000000"))), 
              new ("target", CLValue.PublicKey(PublicKey.FromPem(AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem")))), 
              new ("id", CLValue.Option(CLValue.U64((ulong)BigInteger.Parse("200")))) 
        };

        var clValues = (List<NamedArg>)_contextMap["clValues"];
        runtimeArgs.AddRange(clValues);

        var session = new TransferDeploy(runtimeArgs);

        var senderKey = KeyPair.FromPem(AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem"));
       
        var header = new DeployHeader {
            Account = senderKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = 1800000,
            ChainName = "casper-net-1",
            GasPrice = 1
        };
        var payment = new ModuleBytesDeployItem(100000000);
        
        var deploy = new Deploy(header, payment, session);
        
        deploy.Sign(senderKey);

        _contextMap["deploy"] = deploy;

    }
   
    
    [Then(@"the deploys NamedArgument ""(.*)"" has a value of ""(.*)"" and bytes of ""(.*)""")]
    public void ThenTheDeploysNamedArgumentHasAValueOfAndBytesOf(CLType name, string strValue, string hexBytes) {
        WriteLine("the deploys NamedArgument {0} has a value of {1} and bytes of {2}", name, strValue, hexBytes);
        
        var deploy = (RpcResponse<GetDeployResult>)_contextMap["getDeploy"];

        var namedArg = deploy.Parse().Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals(name.ToString()));
        
        Assert.That(namedArg, Is.Not.Null);

        var value = CLTypeUtils.ConvertToClTypeValue(name, strValue);
        
        Assert.That(namedArg.Value.ToString()!.ToUpper(), Is.EqualTo(value.ToString()!.ToUpper()));
        Assert.That(GetHexValue(namedArg.Value), Is.EqualTo(hexBytes.ToUpper()));
        Assert.That(namedArg.Value.TypeInfo.Type, Is.EqualTo(name));
        
    }

    [Then(@"the deploys NamedArgument Complex value ""(.*)"" has internal types of ""(.*)"" and values of ""(.*)"" and bytes of ""(.*)""")]
    public void ThenTheDeploysNamedArgumentComplexValueHasInternalTypesOfAndValuesOfAndBytesOf(CLType name, string types, string values, string bytes) {
        WriteLine("the deploys NamedArgument Complex value {0} has internal types of {1} and values of {2} and bytes of {3}");
        
        var deploy = (RpcResponse<GetDeployResult>)_contextMap["getDeploy"];
        
        var namedArg = deploy.Parse().Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals(name.ToString()));
        
        Assert.That(namedArg, Is.Not.Null);

        Assert.That(GetHexValue(namedArg.Value), Is.EqualTo(bytes.ToUpper()));

        switch (name) {
            
            case CLType.Option:
                AssertOption(namedArg, types, values);
                break;
            
            case CLType.List:
                AssertList(namedArg, types, values);
                break;
            
            case CLType.Map:
                AssertMap(namedArg, types, values);
                break;
            
            case CLType.Tuple1:
                AssertTupleOne(namedArg, types, values);
                break;
            
            case CLType.Tuple2:
                AssertTupleTwo(namedArg, types, values);
                break;
            
            case CLType.Tuple3:
                AssertTupleThree(namedArg, types, values);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown CLType");
        }

    }
   

    [When(@"the deploy is put on chain")]
    public async Task WhenTheDeployIsPutOnChain() {
        WriteLine("the deploy is put on chain");

        var deploy = (Deploy)_contextMap["deploy"];
        
        var deployResult = await GetCasperService().PutDeploy(deploy);
        
        Assert.IsNotNull(deployResult);
        Assert.IsNotNull(deployResult.Parse().DeployHash);

        _contextMap["deployResult"] = deployResult;

    }

    [Then(@"the deploy has successfully executed")]
    public async Task ThenTheDeployHasSuccessfullyExecuted() {
        WriteLine("the deploy has successfully executed");
        
        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];

        RpcResponse<GetDeployResult> deploy = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
       
        Assert.That(deploy!.Parse().ExecutionResults[0].IsSuccess);
        
    }

    [When(@"the deploy is obtained from the node")]
    public async Task WhenTheDeployIsObtainedFromTheNode() {
        WriteLine("the deploy is obtained from the node");
        
        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];
        var deploy = await GetCasperService().GetDeploy(deployResult.Parse().DeployHash, true);
        
        Assert.That(deploy, Is.Not.Null);
        _contextMap["getDeploy"] = deploy;
        
    }

    [Then(@"the deploy response contains a valid deploy hash of length (.*) and an API version ""(.*)""")]
    public void ThenTheDeployResponseContainsAValidDeployHashOfLengthAndAnApiVersion(int hashLength, string apiVersion) {
        WriteLine("the deploy response contains a valid deploy hash of length {0} and an API version {1}", hashLength, apiVersion);
        
        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];

        Assert.That(deployResult.Parse(), Is.Not.Null);
        Assert.That(deployResult.Parse().DeployHash, Is.Not.Null);
        Assert.That(deployResult.Parse().DeployHash.Length, Is.EqualTo(hashLength));
        Assert.That(deployResult.Parse().ApiVersion, Is.EqualTo(apiVersion));

    }
    
    
    private void AssertTupleThree(NamedArg namedArg, string types, string values) {
        throw new NotImplementedException();
    }

    private void AssertTupleTwo(NamedArg namedArg, string types, string values) {
        throw new NotImplementedException();
    }

    private void AssertTupleOne(NamedArg namedArg, string types, string values) {
        throw new NotImplementedException();
    }

    private void AssertMap(NamedArg namedArg, string types, string values) {
        throw new NotImplementedException();
    }

    private void AssertList(NamedArg namedArg, string types, string values) {
        throw new NotImplementedException();
    }
    
    private void AssertOption(NamedArg arg, string types, string values) {
        throw new NotImplementedException();
    }

    private void AssertClValues(CLValue actual, CLValue expected, CLType type) {
        
        Assert.That(actual.TypeInfo, Is.EqualTo(expected.TypeInfo));
        Assert.That(actual.Bytes, Is.EqualTo(expected.Bytes));
        
    }
    
    
    private string GetHexValue(CLValue clValue) {

        var clValueHex = BitConverter.ToString(clValue.Bytes).Replace("-", "");
        
        if (clValue.TypeInfo.Type.Equals(CLType.Key)) {
            clValueHex = clValueHex[2..];
        }

        return clValueHex;

    }
    
    private List<CLType> GetInnerClTypeData(string innerTypes) {

        List<CLType> types = new List<CLType>();
        
        var list = new List<string>(innerTypes.Split(","));
        
        foreach (var s in list) {
            types!.Add((CLType)Enum.Parse(typeof(CLType), s, true));
        }

        return types;

    }
    
    private void AddValueToContext(CLType type, CLValue value) {

        _contextMap["clValue"] = value;

        List<NamedArg> clValues;
        
        if (!_contextMap.ContainsKey("clValues")) {
             clValues = new List<NamedArg>();
            _contextMap["clValues"] = clValues;
        } else {
            clValues = (List<NamedArg>)_contextMap["clValues"];
        }

        clValues.Add(new NamedArg(type.ToString() , value));

    }

}
