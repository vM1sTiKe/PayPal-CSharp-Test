using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace PayPal_Test.Helpers
{
    public class Http: ComponentBase
    {
        private bool _disposed;
        private readonly HttpClient _client;
        private IConfiguration _config;

        public Http(IConfiguration configuration, string uri)
        {
            this._config = configuration;

            // Create the Http Client
            this._client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };
        }

        private async Task<HttpResponseMessage> Request(HttpMethod method, HttpContent? content)
        {
            var request = new HttpRequestMessage(method, this._client.BaseAddress)
            {
                Content = content
            };

            return await _client.SendAsync(request);
        }

        /// <summary>
        /// Takes care of the response of the Fetch and returns the response content as a string
        /// </summary>
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
        /// Executes a Post with Basic Authorization
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Json response or empty Json in case of error</returns>
        public async Task<string> BasicPost(HttpContent content)
        {
            if (this._disposed) throw new Exception("Cannot use the same Http Client");

            // Create the basic Authorization string
            var basic_auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._config["PayPalSettings:ClientId"] + ":" + this._config["PayPalSettings:ClientSecret"]));
            // Add the basic Authorization Header
            this._client.DefaultRequestHeaders.Add("Authorization", "Basic " + basic_auth);

            // Fetch and return response
            var response = await this.Request(HttpMethod.Post, content);
            return await this.Response(response);
        }

        public async Task<string> Get(HttpContent? content = null)
        {
            if (this._disposed) throw new Exception("Cannot use the same Http Client");

            // Add Headers
            this._client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._config["PayPalSettings:APIToken"]);
            this._client.DefaultRequestHeaders.Add("ContentType", "application/json");

            // Fetch and return response
            var response = await this.Request(HttpMethod.Get, content);
            return await this.Response(response);
        }

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
