using System.Text;
using Microsoft.AspNetCore.Components;

namespace PayPal_Test.Helpers
{
    public class Http: ComponentBase
    {
        private bool _disposed;
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public Http(IConfiguration configuration, string uri)
        {
            this._config = configuration;
            // Create the Http Client
            this._client = new HttpClient { BaseAddress = new Uri(uri) };
        }


        /// <summary>
        /// Takes care of the request
        /// </summary>
        /// <returns>Request response</returns>
        private async Task<HttpResponseMessage> Request(HttpMethod method, HttpContent? content)
        {
            // Cannot execute another request with the same HttpClient
            if (this._disposed) throw new Exception("Cannot use the same Http Client");

            // Creates the request with the given properties
            var request = new HttpRequestMessage(method, this._client.BaseAddress) { Content = content };
            // Executes the request
            return await _client.SendAsync(request);
        }

        /// <summary>
        /// Takes care of the response
        /// </summary>
        /// <returns>String with the JSON response</returns>
        private async Task<string> Response(HttpResponseMessage response)
        {
            // After usage dispose of the client
            this._client.Dispose();
            this._disposed = true;

            // Throws if status code is not success
            // If there is any wierd bad request, Debug the response.Content.ReadAsStringAsync to see the content of the response
            response.EnsureSuccessStatusCode();

            // Returns data
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Executes a Post request with basic authorization
        /// </summary>
        public async Task<string> BasicPost(HttpContent content)
        {
            // Create the basic Authorization string
            var basic_auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._config["PayPalSettings:ClientId"] + ":" + this._config["PayPalSettings:ClientSecret"]));
            // Add the basic Authorization Header
            this._client.DefaultRequestHeaders.Add("Authorization", "Basic " + basic_auth);

            // Fetch and return response
            var response = await this.Request(HttpMethod.Post, content);
            return await this.Response(response);
        }

        /// <summary>
        /// Executes a Get request with the given content
        /// </summary>
        public async Task<string> Get(HttpContent? content = null)
        {
            // Add Headers
            this._client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._config["PayPalSettings:APIToken"]);
            this._client.DefaultRequestHeaders.Add("ContentType", "application/json");

            // Fetch and return response
            var response = await this.Request(HttpMethod.Get, content);
            return await this.Response(response);
        }

        /// <summary>
        /// Executes a Post request with the given content
        /// </summary>
        public async Task<string> Post(HttpContent? content = null)
        {
            if (this._disposed) throw new Exception("Cannot use the same Http Client");

            // Add Headers
            this._client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._config["PayPalSettings:APIToken"]);
            this._client.DefaultRequestHeaders.Add("ContentType", "application/json");

            // Fetch and return response
            var response = await this.Request(HttpMethod.Post, content);
            return await this.Response(response);
        }
    }
}
