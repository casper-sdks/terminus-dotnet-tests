using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CsprSdkStandardTestsNet.Test.Utils; 

/*
 * Provides like commands to a node to obtain raw JSON.
 */
public class SimpleRcpClient {
    private readonly string _hostname;
    private readonly int _port;

    public SimpleRcpClient(string hostname, int port) {
        _hostname = hostname;
        _port = port;
    }
    
    /**
     * Obtains the era summary
     * Since 1.5
     * This replaces chain_get_era_info_by_switch_block
     * No need now to wait for era end to query the era info
     **/
    public Task<JsonNode> GetEraSummary(string hash) {
        return Rcp("chain_get_era_summary", "[{\"Hash\":  \"" + hash + "\"}]");
    }
    
    public Task<JsonNode> GetValidatorChanges(){
        return Rcp("info_get_validator_changes", "[]");
    }

    public Task<JsonNode> GetInfoGetChainspec(){
        return Rcp("info_get_chainspec", "[]");
    }
    
    public Task<JsonNode> GetBalance(string stateRootHash, string purseUref){
        return Rcp("state_get_balance", 
            $"{{\"state_root_hash\":\"{stateRootHash}\",\"purse_uref\":\"{purseUref}\"}}");
    }
    
    private async Task<JsonNode> Rcp(string method, string _params) {

        var client = new HttpClient();
        var payload =
            $"{{\"id\":\"{DateTime.Now.Millisecond}\",\"jsonrpc\":\"2.0\",\"method\":\"{method}\",\"params\":{_params}}}";
        
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"http://{_hostname}:{_port}/rpc"));
        request.Content = new StringContent(payload, Encoding.UTF8, new MediaTypeWithQualityHeaderValue("application/json"));
        
        var httpResponseMessage = await client.PostAsync(request.RequestUri, request.Content);

        var resp = await httpResponseMessage.Content.ReadAsStringAsync();

        return JsonNode.Parse(resp);

    }
    
}
