using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using NHamcrest;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace CsprSdkStandardTestsNet.Test.Utils;

/**
 * Calls to the NCTL docker image 
 */

public partial class Nctl {
    private readonly string _dockerName;

    public Nctl(string dockerName) {
        _dockerName = dockerName;
    }


    public string GetAccountMainPurse(int userId) {
        
        var node = Execute("view_user_account.sh", "user=" + userId, ParseJsonWithPreAmble);

        var mainPurse = node["stored_value"]!["Account"]!["main_purse"]!.ToString();
        
        Assert.That(mainPurse, Is.Not.Null);
        Assert.That(mainPurse.StartsWith("uref-"), Is.True);

        return mainPurse;
    }
    

    public string GetAccountHash(int userId) {

        var node = GetUserAccount(userId);
        var accountHash = node["stored_value"]!["Account"]!["account_hash"];
        
        Assert.That(accountHash, Is.Not.Null);
        Assert.That(accountHash.ToString().StartsWith("account-hash-"), Is.True);
        
        return accountHash.ToString();

    }
    
    public JsonNode GetNodeStatus(int nodeId) {
        return Execute("view_node_status.sh", "node=" + nodeId, ParseJsonWithPreAmble);
    }
    
    public JsonNode GetChainBlock(string blockHash) {
        return Execute("view_chain_block.sh", "block=" + blockHash, ParseJson);
    }
    
    public JsonNode GetChainBlockTransfers(string blockHash) {
        return Execute("view_chain_block_transfers.sh", "block=" + blockHash, ParseJson);
    }
    
    public string GetStateRootHash(int nodeId) {
        return Execute("view_chain_state_root_hash.sh", "node=" + nodeId, ParseString)
            .Split("=")[1].Trim();
    }

    public JsonNode GetUserAccount(int userId) {
        return Execute("view_user_account.sh", "user=" + userId, ParseJsonWithPreAmble);
    }
    
    private T Execute<T> (string shellCommand, string parameters, Func<string, T> func) {
        
        ProcessStartInfo startInfo = new() {
            FileName = "docker",
            Arguments =
                $"exec -t {_dockerName}  /bin/bash -c \"source casper-node/utils/nctl/sh/views/{shellCommand} {parameters ?? ""}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var proc = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(proc);

        return func(proc.StandardOutput.ReadToEnd());

    }

    private static JsonNode ParseJson(string input) {
        return JsonNode.Parse(ReplaceAnsiConsoleCodes(input));
    }
    private static JsonNode ParseJsonWithPreAmble(string input) {
        return JsonNode.Parse(ReplaceAnsiConsoleCodes(input[input.IndexOf("{", StringComparison.Ordinal)..]));
    }
    
    private static string ParseString(string input) {
        return ReplaceAnsiConsoleCodes(input);
    }
    
    private static string ReplaceAnsiConsoleCodes(string response) {
        //remove any console colour ANSI info
        return AnsiRegex().Replace(response, "");
    }

    [GeneratedRegex("\u001b\\[[;\\d]*m")]
    private static partial Regex AnsiRegex();
}
