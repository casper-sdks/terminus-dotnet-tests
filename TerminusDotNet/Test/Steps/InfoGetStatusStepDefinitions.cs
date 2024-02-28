using System;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * info_get_status step definitions
 */
[Binding]
public class InfoGetStatusStepDefinitions {

    private readonly ContextMap _contextMap = ContextMap.Instance;
    private static readonly TestProperties TestProperties = new();
    private readonly NodeClient _nodeClient = new(TestProperties.DockerName);

    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [Then(@"an info_get_status_result is returned")]
    public void ThenAnInfoGetStatusResultIsReturned() {
       WriteLine("an info_get_status_result is returned");

       var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
       Assert.That(statusData, Is.Not.Null);
       
    }

    [Given(@"that the info_get_status is invoked against nctl")]
    public async Task GivenThatTheInfoGetStatusIsInvokedAgainstNctl() {
        WriteLine("that the info_get_status is invoked against nctl");

        var expectedJsonNodeStatus = _nodeClient.GetNodeStatus(1);
       
        Assert.That(expectedJsonNodeStatus, Is.Not.Null);
        _contextMap.Add(StepConstants.EXPECTED_STATUS_DATA, expectedJsonNodeStatus);
       
        _contextMap.Add(StepConstants.STATUS_DATA, await GetCasperService().GetNodeStatus());

    }

    [Then(@"the info_get_status_result api_version is ""(.*)""")]
    public void ThenTheInfoGetStatusResultApiVersionIs(string apiVersion) {
        WriteLine("the info_get_status_result api_version is {0}", apiVersion);
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        Assert.That(statusData.Parse().ApiVersion, Is.EqualTo(apiVersion));

    }

    [Then(@"the info_get_status_result chainspec_name is ""(.*)""")]
    public void ThenTheInfoGetStatusResultChainspecNameIs(string chainName) {
        WriteLine("the info_get_status_result chainspec_name is {0}", chainName);
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        Assert.That(statusData.Parse().ChainspecName, Is.EqualTo(TestProperties.ChainName));
    }

    [Then(@"the info_get_status_result has a valid last_added_block_info")]
    public void ThenTheInfoGetStatusResultHasAValidLastAddedBlockInfo() {
        WriteLine("the info_get_status_result has a valid last_added_block_info"); 
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);

        var lastBlockAddedSdk = statusData.Parse().LastAddedBlockInfo;
        var lastBlockAddedNode = jsonNode["last_added_block_info"];
        
        Assert.That(lastBlockAddedSdk.Hash.ToUpper(), 
            Is.EqualTo(lastBlockAddedNode!["hash"]!.ToString().ToUpper()));
        Assert.That(GetDate(lastBlockAddedSdk.Timestamp), 
            Is.EqualTo(GetDate(lastBlockAddedNode["timestamp"]!.ToString())));
        Assert.That(lastBlockAddedSdk.EraId.ToString(), 
            Is.EqualTo(lastBlockAddedNode!["era_id"]!.ToString()));
        Assert.That(lastBlockAddedSdk.Height.ToString(), 
            Is.EqualTo(lastBlockAddedNode!["height"]!.ToString()));
        Assert.That(lastBlockAddedSdk.StateRootHash.ToUpper(),
            Is.EqualTo(lastBlockAddedNode!["state_root_hash"]!.ToString().ToUpper()));
        Assert.That(lastBlockAddedSdk.PublicKey.ToString().ToUpper(), 
            Is.EqualTo(lastBlockAddedNode!["creator"]!.ToString().ToUpper()));

    }

    [Then(@"the info_get_status_result has a valid our_public_signing_key")]
    public void ThenTheInfoGetStatusResultHasAValidOurPublicSigningKey() {
        WriteLine("the info_get_status_result has a valid our_public_signing_key");
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        Assert.That(statusData.Parse().OurPublicSigningKey.ToString().ToUpper(), 
            Is.EqualTo(jsonNode["our_public_signing_key"]!.ToString().ToUpper()));
        
    }

    [Then(@"the info_get_status_result has a valid starting_state_root_hash")]
    public void ThenTheInfoGetStatusResultHasAValidStartingStateRootHash() {
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        Assert.That(statusData.Parse().StartingStateRootHash.ToUpper(), 
            Is.EqualTo(jsonNode["starting_state_root_hash"]!.ToString().ToUpper()));
    }

    [Then(@"the info_get_status_result has a valid build_version")]
    public void ThenTheInfoGetStatusResultHasAValidBuildVersion() {
        WriteLine("the info_get_status_result has a valid build_version");
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        Assert.That(statusData.Parse().BuildVersion, 
            Is.EqualTo(jsonNode["build_version"]!.ToString()));
        
    }

    [Then(@"the info_get_status_result has a valid round_length")]
    public void ThenTheInfoGetStatusResultHasAValidRoundLength() {
        WriteLine("the info_get_status_result has a valid round_length");
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        Assert.That(statusData.Parse().RoundLength, 
            Is.EqualTo(jsonNode["round_length"]!.ToString()));

        
    }

    [Then(@"the info_get_status_result has a valid uptime")]
    public void ThenTheInfoGetStatusResultHasAValidUptime() {
        WriteLine("the info_get_status_result has a valid uptime");
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        var expectedUptime = jsonNode["uptime"]!.ToString();
        Assert.That(expectedUptime, Is.Not.Null);

        var expectedSeconds = GetTimeInSecondsFromUptime(expectedUptime);
        var actualSeconds = GetTimeInSecondsFromUptime(statusData.Parse().Uptime);
        
        
        Assert.That(expectedSeconds, Is.GreaterThan(10));
        Assert.That(actualSeconds, Is.GreaterThan(10));

        // assert there are less than 5seconds between both times
        Assert.That(actualSeconds - expectedSeconds, Is.LessThanOrEqualTo(5));

        Assert.That(statusData.Parse().Uptime, Does.Contain("m "));
        Assert.That(statusData.Parse().Uptime, Does.Contain("s "));
        Assert.That(statusData.Parse().Uptime, Does.Contain("ms"));
        
    }

    [Then(@"the info_get_status_result has a valid peers")]
    public void ThenTheInfoGetStatusResultHasAValidPeers() {
        WriteLine("the info_get_status_result has a valid peers");
        
        var statusData = _contextMap.Get<RpcResponse<GetNodeStatusResult>>(StepConstants.STATUS_DATA);
        var jsonNode = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_STATUS_DATA);
        
        Assert.That(statusData.Parse().Peers.Count, 
            Is.EqualTo(jsonNode!["peers"]!.AsArray().Count));
        Assert.That(statusData.Parse().Peers[0].Address, 
            Is.EqualTo(jsonNode!["peers"][0]!["address"]!.ToString()));
        Assert.That(statusData.Parse().Peers[3].Address, 
            Is.EqualTo(jsonNode!["peers"][3]!["address"]!.ToString()));
        Assert.That(statusData.Parse().Peers[3].NodeId, 
            Is.EqualTo(jsonNode!["peers"][3]!["node_id"]!.ToString()));
        
    }

    private static string GetDate(string date) {
        return DateTime.Parse(date, new CultureInfo("en-US"),
            DateTimeStyles.NoCurrentDateDefault).Date.ToString(CultureInfo.InvariantCulture);
    }

    private static int GetTimeInSecondsFromUptime(string uptime) {
        var uptimeParts = uptime.Split(" ");
        var hours = 0;
        var minutes = 0;
        var seconds = 0;
        
        foreach (var part in uptimeParts) {
            if (part.EndsWith("h")) {
                hours = ExtractNumber(part);
            } else if (part.EndsWith("m")) {
                minutes = ExtractNumber(part);
            } else if (part.EndsWith("s") && !part.EndsWith("ms")) {
                seconds = ExtractNumber(part);
            }
        }
        
        return (hours * 60 * 60) + (minutes * 60) + seconds;  
        
    }
    
    private static int ExtractNumber(string part) {
        return int.Parse(part[..^1]);
    }
    
}
