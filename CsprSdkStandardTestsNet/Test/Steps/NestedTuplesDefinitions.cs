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

[Binding]
public class NestedTuplesDefinitions {
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a nested Tuple(.*) is defined as \((.*)\) using U32 numeric values")]
    public void GivenThatANestedTupleIsDefinedAsUsingUNumericValues1(int tuple, string value) {
        WriteLine("that a nested Tuple{0} is defined as \\(\\{1}\\)\\' using U32 numeric values", tuple, value);
        
          // Specflow cucumber doesn't recognise complex regex feature steps
          // It will default to this step
          // Hence the tuple if condition

        var val = value.Replace("(", "").Replace(")", "").Split(",");

        if (tuple == 1) {
            _contextMap.Add("TUPLE_ROOT_1",
                CLValue.Tuple1(CLValue.Tuple1(CLValue.U32(Convert.ToUInt32(val[0])))));
        }
        if (tuple == 2) {
            _contextMap.Add("TUPLE_ROOT_2",
                CLValue.Tuple2(CLValue.U32(Convert.ToUInt32(val[0])),
                    CLValue.Tuple2(CLValue.U32(Convert.ToUInt32(val[1])),
                        CLValue.Tuple2(CLValue.U32(Convert.ToUInt32(val[2])),
                            CLValue.U32(Convert.ToUInt32(val[3]))))));
            
        }
        if (tuple == 3) {
            _contextMap.Add("TUPLE_ROOT_3",
                CLValue.Tuple3(CLValue.U32(Convert.ToUInt32(val[0])),
                    CLValue.U32(Convert.ToUInt32(val[1])),
                        CLValue.Tuple3(CLValue.U32(Convert.ToUInt32(val[2])),
                            CLValue.U32(Convert.ToUInt32(val[3])),
                            CLValue.Tuple3(
                                CLValue.U32(Convert.ToUInt32(val[4])),
                                CLValue.U32(Convert.ToUInt32(val[5])),
                                CLValue.U32(Convert.ToUInt32(val[6]))))));
            

        }
    }

    [Then(@"the ""(.*)"" element of the Tuple(.*) is ""(.*)""")]
    public void ThenTheElementOfTheTupleIs(string index, int tuple, string value) {
        WriteLine("the '{0}' element of the Tuple{1} is '{2}'", index, tuple, value);

        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }
    
    [Then(@"the ""(.*)"" element of the Tuple(.*) is (.*)")]
    public void ThenTheElementOfTheTupleIs(string index, int tuple, int value) {
        WriteLine("the '{0}' element of the Tuple{1} is '{2}'", index, tuple, value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();

    }

    [Then(@"the Tuple(.*) bytes are ""(.*)""")]
    public void ThenTheTupleBytesAre(int tuple, string hexBytes) {
        WriteLine("the Tuple{0} bytes are '{1}'", tuple, hexBytes);
        
        var clType = GetTuple(tuple);
        Assert.That(CLTypeUtils.GetHexValue(clType), Is.EqualTo(hexBytes));

    }

    [Given(@"that a nested Tuple(.*) is defined as \((.*), \((.*), \((.*), (.*)\)\) using U32 numeric values")]
    public void GivenThatANestedTupleIsDefinedAsUsingUNumericValues2(int tuple, int arg0, int arg1, int arg2, int arg3) {
        WriteLine("that a nested Tuple{0} is defined as ({1}), ({2}, ({3}, {4}))) using U32 numeric values", tuple, arg0, arg1, arg2, arg3);
        
        //Executed in the method GivenThatANestedTupleIsDefinedAsUsingUNumericValues1
        
    }
    
    [Given(@"that a nested Tuple(.*) is defined as \((.*), (.*), \((.*), (.*), (.*), (.*), (.*)\)\) using U32 numeric values")]
    public void GivenThatANestedTupleIsDefinedAsUsingUNumericValues3(int tuple, int arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) {
        WriteLine("that a nested Tuple{0} is defined as ({1}), ({2}, ({3}, {4}, {5}, {6}, {7}))) using U32 numeric values", tuple, arg0, arg1, arg2, arg3, arg4, arg5, arg6);

        //Executed in the method GivenThatANestedTupleIsDefinedAsUsingUNumericValues1

    }
    

    [Given(@"that the nested tuples are deployed in a transfer")]
    public async Task GivenThatTheNestedTuplesAreDeployedInATransfer() {
        WriteLine("that the nested tuples are deployed in a transfer");

        var runtimeArgs = new List<NamedArg>{ 
            new ("amount", CLValue.U512(BigInteger.Parse("2500000000"))), 
            new ("target", CLValue.PublicKey(PublicKey.FromPem(AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem")))), 
            new ("id", CLValue.Option(CLValue.U64((ulong)BigInteger.Parse("200")))),
            new ("TUPLE_1", _contextMap.Get<CLValue>("TUPLE_ROOT_1")), 
            new ("TUPLE_2", _contextMap.Get<CLValue>("TUPLE_ROOT_2")), 
            new ("TUPLE_3", _contextMap.Get<CLValue>("TUPLE_ROOT_3"))
        };

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

        _contextMap.Add(StepConstants.PUT_DEPLOY, deploy);

        WriteLine(deploy.SerializeToJson());

        var deployResult = await GetCasperService().PutDeploy(deploy);
        
        Assert.IsNotNull(deployResult);
        Assert.IsNotNull(deployResult.Parse().DeployHash);

        _contextMap.Add(StepConstants.DEPLOY_RESULT, deployResult);

    }

    [Given(@"the transfer is successful")]
    public async Task GivenTheTransferIsSuccessful() {
        WriteLine("the transfer is successful");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);

        RpcResponse<GetDeployResult> deploy = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
       
        Assert.That(deploy!.Parse().ExecutionResults[0].IsSuccess);

        _contextMap.Add(StepConstants.DEPLOY, deploy);

    }

    [When(@"the tuples deploy is obtained from the node")]
    public void WhenTheTuplesDeployIsObtainedFromTheNode() {
        WriteLine("the tuples deploy is obtained from the node");

        var deploy = _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.DEPLOY).Parse();

        _contextMap.Add("TUPLE_ROOT_1", deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("TUPLE_1")).Value);
        _contextMap.Add("TUPLE_ROOT_2", deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("TUPLE_2")).Value);
        _contextMap.Add("TUPLE_ROOT_3", deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("TUPLE_3")).Value);

        Assert.That(_contextMap.Get<CLValue>("TUPLE_ROOT_1"), Is.Not.Null);
        Assert.That(_contextMap.Get<CLValue>("TUPLE_ROOT_2"), Is.Not.Null);
        Assert.That(_contextMap.Get<CLValue>("TUPLE_ROOT_3"), Is.Not.Null);

    }

    private CLValue GetTuple(int tuple) {
        return tuple switch {
            1 => _contextMap.Get<CLValue>("TUPLE_ROOT_1"),
            2 => _contextMap.Get<CLValue>("TUPLE_ROOT_2"),
            _ => _contextMap.Get<CLValue>("TUPLE_ROOT_3")
        };
    }

}
