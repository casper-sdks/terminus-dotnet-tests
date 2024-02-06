using System;
using System.IO;
using System.Numerics;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * Read deploy step definitions
 */

[Binding]
public class ReadDeployStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }

    [Given(@"that the ""(.*)"" JSON deploy is loaded")]
    public void GivenThatTheJsonDeployIsLoaded(string json) {
        WriteLine("that the {0} JSON deploy is loaded", json);
        
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!;
        var jsonPath = directoryInfo.Parent!.FullName + "/Test/Json/" + json;
        
        using StreamReader reader = new(jsonPath);
        var jsonFile = reader.ReadToEnd();

        var transfer = Deploy.Parse(jsonFile);
        
        _contextMap.Add(StepConstants.TRANSFER, transfer);

    }

    [Then(@"a valid transfer deploy is created")]
    public void ThenAValidTransferDeployIsCreated() {
        WriteLine("a valid transfer deploy is created");
        
        Assert.That(GetDeploy(), Is.Not.Null);
        
    }

    [Then(@"the deploy hash is ""(.*)""")]
    public void ThenTheDeployHashIs(string deployHash) {
        WriteLine("the deploy hash is {0}", deployHash);
        
        Assert.That(GetDeploy().Hash.ToUpper(), Is.EqualTo(deployHash.ToUpper()));
        
    }

    [Then(@"the account is ""(.*)""")]
    public void ThenTheAccountIs(string account) {
        WriteLine("the account is {0}", account);
        
        Assert.That(GetDeploy().Header.Account.ToAccountHex().ToUpper(), Is.EqualTo(account.ToUpper()));
    }

    [Then(@"the timestamp is ""(.*)""")]
    public void ThenTheTimestampIs(string timestamp) {
        WriteLine("the timestamp is {0}", timestamp);
        
        Assert.That(GetDeploy().Header.Timestamp, Is.EqualTo(DateUtils.ToEpochTime(timestamp)));
        
    }

    [Then(@"the ttl is (.*)m")]
    public void ThenTheTtlIsM(int ttl) {
        WriteLine("the ttl is {0}", ttl);
        
        Assert.That(GetDeploy().Header.Ttl, Is.EqualTo(TimeSpan.FromMinutes(ttl).TotalMilliseconds));
        
    }

    [Then(@"the gas price is (.*)")]
    public void ThenTheGasPriceIs(int gas) {
        WriteLine("the gas price is {0}", gas);
        
        Assert.That(GetDeploy().Header.GasPrice, Is.EqualTo(gas));
        
    }

    [Then(@"the body_hash is ""(.*)""")]
    public void ThenTheBodyHashIs(string bodyHash) {
        WriteLine("the body_hash is {0}", bodyHash);
        
        Assert.That(GetDeploy().Header.BodyHash.ToUpper(), Is.EqualTo(bodyHash.ToUpper()));
        
    }

    [Then(@"the chain name is  ""(.*)""")]
    public void ThenTheChainNameIs(string chain) {
        WriteLine("the chain name is {0}", chain);
        
        Assert.That(GetDeploy().Header.ChainName.ToUpper(), Is.EqualTo(chain.ToUpper()));
        
    }

    [Then(@"dependency (.*) is ""(.*)""")]
    public void ThenDependencyIs(int dependency, string value) {
        WriteLine("dependency {0} is {1}", dependency, value);
        
        Assert.That(GetDeploy().Header.Dependencies[dependency].ToUpper(), Is.EqualTo(value.ToUpper()));

    }

    [Then(@"the payment amount is (.*)")]
    public void ThenThePaymentAmountIs(string payment) {
        WriteLine("the payment amount is {0}", payment);

        var arg = GetDeploy().Payment.RuntimeArgs.Find(n => n.Name.Equals("amount"));
        
        Assert.That(arg, Is.Not.Null);
        
        Assert.That(arg.Value.Parsed, Is.EqualTo(payment));
        
    }

    [Then(@"the session is a transfer")]
    public void ThenTheSessionIsATransfer() {
        WriteLine("the session is a transfer");
        
        Assert.That(GetDeploy().Session is TransferDeployItem, Is.True);
        
    }

    [Then(@"the session ""(.*)"" is (.*)")]
    public void ThenTheSessionIs(string session, object value) {
        WriteLine("the session {0} is {1}", session, value);
        
        var arg = GetDeploy().Session.RuntimeArgs.Find(n => n.Name.Equals(session));
        Assert.That(arg, Is.Not.Null);

        if (value is string) {

            var val = value.ToString()!.Replace("\"", string.Empty);
            
            if (arg.Value.TypeInfo.Type is CLType.ByteArray) {
                Assert.That(arg.Value.Bytes, Is.EqualTo(Hex.Decode(val)));
            } else {
                Assert.That(arg.Value.ToString(), Is.EqualTo(val));
            }
            
        } else {
            Assert.That(arg.Value, Is.EqualTo(CLValue.U512(BigInteger.Parse(value.ToString()!))));
        }
        
    }

    [Then(@"the session ""(.*)"" type is ""(.*)""")]
    public void ThenTheSessionTypeIs(string session, string type) {
        WriteLine("the session {0} type is {1}", session, type);
        
        var arg = GetDeploy().Session.RuntimeArgs.Find(n => n.Name.Equals(session));
        Assert.That(arg, Is.Not.Null);

        Assert.That(arg.Value.TypeInfo.ToString(), Is.EqualTo(type) );
        
    }

    [Then(@"the session ""(.*)"" bytes is ""(.*)""")]
    public void ThenTheSessionBytesIs(string session, string bytes) {
        WriteLine("the session {0} bytes is {1}", session, bytes);
        
        var arg = GetDeploy().Session.RuntimeArgs.Find(n => n.Name.Equals(session));
        Assert.That(arg, Is.Not.Null);
        
        Assert.That(arg.Value.Bytes, Is.EqualTo(Hex.Decode(bytes)));
        
    }

    [Then(@"the session ""(.*)"" parsed is ""(.*)""")]
    public void ThenTheSessionParsedIs(string session, string parsed) {
        WriteLine("the session {0} parsed is {1}", session, parsed);
        
        var arg = GetDeploy().Session.RuntimeArgs.Find(n => n.Name.Equals(session));
        Assert.That(arg, Is.Not.Null);

        Assert.That(arg.Value.Parsed.ToString(), Is.EqualTo(parsed));
        
    }

    [Then(@"the deploy has (.*) approval")]
    public void ThenTheDeployHasApproval(int approval) {
        WriteLine("the deploy has {0} approval", approval);
        
        Assert.That(GetDeploy().Approvals.Count, Is.EqualTo(approval));
        
    }

    [Then(@"the approval signer is ""(.*)""")]
    public void ThenTheApprovalSignerIs(string signer) {
        WriteLine("the deploy signer is {0}", signer);
        
        Assert.That(GetDeploy().Approvals[0].Signer.ToString().ToUpper(), Is.EqualTo(signer.ToUpper()));
        
    }

    [Then(@"the approval signature is ""(.*)""")]
    public void ThenTheApprovalSignatureIs(string signature) {
        WriteLine("the approval signature is {0}", signature);
        
        Assert.That(GetDeploy().Approvals[0].Signature.ToString().ToUpper(), Is.EqualTo(signature.ToUpper()));
        
    }

    private Deploy GetDeploy() {
        return _contextMap.Get<Deploy>(StepConstants.TRANSFER);
    }
    
}
