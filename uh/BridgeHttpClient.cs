using System.Net.Http.Json;
using TShockAPI;
using System.Text.Json;
namespace SpleefResurgence.uh
{
    public class BridgeHttpClient
    {
        private HttpClient client = new();

        public BridgeHttpClient(string ip, int port, string apiKey)
        {
            client.BaseAddress = new Uri($"http://{ip}:{port}/api/");

            client.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);

            TShock.Log.ConsoleInfo($"http base address: \"{client.BaseAddress}\"");
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await client.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {

#if DEBUG
            TShock.Log.ConsoleInfo($"Sending a POST request to {client.BaseAddress}{endpoint}\n {JsonSerializer.Serialize(data)}");
#endif

            var response = await client.PostAsJsonAsync(endpoint, data);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PostAsync<TRequest>(string endpoint, TRequest data)
        {

#if DEBUG
            TShock.Log.ConsoleInfo($"Sending a POST request to {client.BaseAddress}{endpoint}\n {JsonSerializer.Serialize(data)}");
#endif

            var response = await client.PostAsJsonAsync(endpoint, data);

            response.EnsureSuccessStatusCode();
        }
    }
}
