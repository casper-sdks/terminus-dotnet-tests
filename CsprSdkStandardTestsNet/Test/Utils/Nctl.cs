using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CsprSdkStandardTestsNet.Test.Utils;

/**
 * Calls to the NCTL docker image 
 */

public class Nctl {
    private readonly string _dockerName;

    public Nctl(string dockerName) {
        _dockerName = dockerName;
    }
    
    public JsonNode GetNodeStatus(int nodeId) {
        var e=  Execute("view_node_status.sh", "node=" + nodeId, ParseJsonWithPreAmble);
        return e;
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
        return Regex.Replace(response, "\u001B\\[[;\\d]*m", "");
    }
    
}
