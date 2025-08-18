using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EventApi.Interfaces;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http.Features;
using Org.BouncyCastle.Asn1.X509;

namespace EventApi.Services
{
    public class FirebaseAdminService : IFirebaseAdminService
    {
        private readonly ILogger<FirebaseAdminService> _logger;
        private readonly IHttpClientFactory _httpClientFact;
        private readonly IConfiguration _config;

        public FirebaseAdminService(ILogger<FirebaseAdminService> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _logger = logger;
            _httpClientFact = httpClientFactory;
            _config = config;
        }

        public async Task<string> GetEmailFromOobCodeAsync(string oobCode)
        {

            var webApiKey = _config["Firebase:WebApiKey"];
            if (string.IsNullOrEmpty(webApiKey))
            {
                _logger.LogError("Firebase Web API Key is not configured.");
                return null;
            }

            var firebaseEndpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:resetPassword?key={webApiKey}";
            var requestPayload = new { oobCode = oobCode };

            try
            {
                var httpclient = _httpClientFact.CreateClient();
                var response = await httpclient.PostAsJsonAsync(firebaseEndpoint, requestPayload);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadFromJsonAsync<FirebaseOobResponse>();
                    return responseBody?.Email;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to verify OOB code via REST API. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to Firebase Auth REST API failed.");
                return null;
            }
        }
    }
    public class FirebaseOobResponse
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }
        }
}