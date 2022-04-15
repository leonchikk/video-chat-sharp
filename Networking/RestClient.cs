using System;
using System.Net.Http;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.Clients;

namespace VoiceEngine.Network
{
    public class RestClient : IRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://video-chat-sharp.azurewebsites.net";

        public RestClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<string> GetAuthorizationToken()
        {
            var response = await _httpClient.PostAsync("/api/auth", null).ConfigureAwait(false);
            var jwtToken = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return jwtToken;
        }
    }
}
