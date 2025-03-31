using System.Text.Json.Nodes;
using Azure;
using Microsoft.AspNetCore.Mvc;

namespace PayPal_Test.Controllers
{
    public class PayPalController(IConfiguration config) : Controller
    {
        public async Task<JsonResult> Index()
        {
            return Json(new { da = "da" }); ;
        }

        public async Task<JsonResult> OAuth()
        {
            // cria um objeto json
            var d = Json(new { da = "da" });

            return d;
        }

        public async Task<JsonNode> SetupClientPaymentMethod()
        {
            // First validate current Token
            await this.ValidateApiToken();
            // Only after procceed with logic

            var fetch_uri = config["PayPalSettings:Url"] + "/v3/vault/setup-tokens";
            var fetch_content = JsonContent.Create(new {
                payment_source = new {
                    paypal = new {
                        permit_multiple_payment_tokens = true, usage_pattern = "IMMEDIATE", usage_type = "MERCHANT", customer_type = "CONSUMER",
                        //customer_type = "BUSINESS",
                        experience_context = new {
                            payment_method_preference = "IMMEDIATE_PAYMENT_REQUIRED", brand_name = "AeroBites", locale = "en-US",
                            return_url = "https://example.com/returnUrl", cancel_url = "https://example.com/cancelUrl"
                        }
                    }
                }
            });
            var response = await new Helpers.Http(config, fetch_uri).Post(fetch_content);

            // Json parses the response
            return JsonNode.Parse(response);
        }


        /// <summary>
        /// Queries PayPal API to create a Api Token for the application
        /// </summary>
        private async Task<bool> CreateApiToken()
        {
            var fetch_uri = config["PayPalSettings:Url"] + "/v1/oauth2/token";
            var fetch_content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "client_credentials" },
                { "ignoreCache", "true" },
                { "return_unconsented_scopes", "true" },
            });

            // Fetch to get the Api Token
            var response = await new Helpers.Http(config, fetch_uri).BasicPost(fetch_content);

            // Json parses the response
            var json_response = JsonNode.Parse(response);
            if (json_response is null) return false;

            // Verifies if the token is something
            var token = json_response?["access_token"]?.ToString();
            if (token is "" || token is null) return false;

            // Save the token
            config["PayPalSettings:APIToken"] = token;
            return true;
        }

        /// <summary>
        /// Calls PayPal endpoint to validate if current Token is valid
        /// </summary>
        private async Task<bool> IsApiTokenValid()
        {
            var fetch_uri = config["PayPalSettings:Url"] + "/v1/oauth2/token";

            // Fetch to verify if API still valid
            string response = "";
            try {
                response = await new Helpers.Http(config, fetch_uri).Get();
            } catch (Exception) { return false; }

            // Json parses the response
            var json_response = JsonNode.Parse(response);
            if (json_response is null) return false;
            if (json_response["expires_in"] is not null) return true;
            return false;
        } 

        /// <summary>
        /// It will first query if the Token is valid, if is not create a token and save it
        /// 
        /// If its not possible to create a Api Token the application will throw a Exception
        /// </summary>
        private async Task<bool> ValidateApiToken()
        {
            if (await this.IsApiTokenValid()) return true;
            if (await this.CreateApiToken()) return true;
            throw new Exception("Cannot create PayPal Api Token");
        }

        public async Task<JsonNode> CreateUserToken(string tokenId)
        {
            await this.ValidateApiToken();

            var fetch_uri = config["PayPalSettings:Url"] + "/v3/vault/payment-tokens";
            var fetch_content = JsonContent.Create(new
            {
                payment_source = new
                {
                    token = new
                    {
                        id = "3LU32795BF5999742", //id = tokenId,
                        type = "SETUP_TOKEN"
                    }
                }
            });

            var response = await new Helpers.Http(config, fetch_uri).Post(fetch_content);
            return JsonNode.Parse(response);
        }

        public async Task<bool> DeletePaymentToken(string paymentTokenId)
        {
            await this.ValidateApiToken();

            var fetch_uri = config["PayPalSettings:Url"] + "/v3/vault/payment-tokens/" + paymentTokenId;
            var response = await new Helpers.Http(config, fetch_uri).Post(null);

            return true;
        }

        public async Task<JsonNode> PayUserVaulted(string paymentTokenId, float price)
        {
            await this.ValidateApiToken();

            var fetch_uri = config["PayPalSettings:Url"] + "/v2/checkout/orders";
            var fetch_content = JsonContent.Create(new
            {
                intent = "CAPTURE",
                payment_source = new
                {
                    paypal = new
                    {
                        vault_id = paymentTokenId,
                    }
                },
                purchase_units = new
                {
                    amount = new
                    {
                        currency_code = "usd",
                        value = price
                    }
                }
            });

            var response = await new Helpers.Http(config, fetch_uri).Post(fetch_content);
            return JsonNode.Parse(response);
        }
    }
}
