using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data.SqlClient;
using System.Collections.Generic;



namespace api_tester
{
    public class CodeJarClient
    {
        public HttpClient Client = new HttpClient();
        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Gets a list of all batches from the API
        /// </summary>
        public async Task<HttpResponseMessage> GetBatchListAsync()
        {
            return await Client.GetAsync(requestUri: "http://localhost:5000/batch/");
        }

        /// <summary>
        /// Gets a specified page of a batch from the API
        /// </summary>
        public async Task<HttpResponseMessage> GetBatchAsync(int id, int page)
        {
            return await Client.GetAsync(requestUri: $"http://localhost:5000/batch/{id}?page={page}");
        }

        /// <summary>
        /// Creates a batch using the /batch route.
        /// </summary>
        public async Task<HttpResponseMessage> CreateBatchAsync(Batch batch)
        {
            // Format dates
            var dateActive = FormatDate.YearMonthDay(batch.DateActive);
            var dateExpires = FormatDate.YearMonthDay(batch.DateExpires);

            // Create content object
            HttpContent content = new StringContent(
                content: "{" +
                    $"\"BatchName\": \"{batch.BatchName}\"," +
                    $"\"BatchSize\": {batch.BatchSize}," +
                    $"\"DateActive\": \"{dateActive}\"," +
                    $"\"DateExpires\": \"{dateExpires}\"" +
                "}",
                encoding: Encoding.UTF8,
                mediaType: "application/json"
           );

            // Return response
            return await Client.PostAsync("http://localhost:5000/batch", content);
        }

        public async Task<HttpResponseMessage> DeleteBatchAsync(Batch batch)
        {
            var dateActive = FormatDate.YearMonthDay(batch.DateActive);
            var dateExpires = FormatDate.YearMonthDay(batch.DateExpires);

            var payload = "{" +
            $"\"BatchName\": \"{batch.BatchName}\"," +
            $"\"BatchSize\": {batch.BatchSize}," +
            $"\"DateActive\": \"{dateActive}\"," +
            $"\"DateExpires\": \"{dateExpires}\"" +
            "}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("http://localhost:5000/batch"),
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = await Client.SendAsync(request);
            return response;
        }
        /// <summary>
        /// Deactivates a code using the provided codeStringValue argument.
        /// </summary>
        public async Task<HttpResponseMessage> DeactivateCodeAsync(string codeStringValue)
        {
            string[] arr = new string[] { codeStringValue };
            var payload = JsonSerializer.Serialize(arr, _jsonOptions);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("http://localhost:5000/codes"),
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = await Client.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> RedeemCodeAsync(string codeStringValue)
        {
            var content = JsonSerializer.Serialize(codeStringValue, _jsonOptions);
            var response = await Client.PostAsync(
                requestUri: "http://localhost:5000/redeem-code",
                content:
                    new StringContent(
                        content: content,
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    )
            );
            return response;
        }

        /// <summary>
        /// Returns a code if it was found, otherwise returns a 404 status code
        /// </summary>
        public async Task<HttpResponseMessage> SearchCodeAsync(string codeStringValue)
        {
            return await Client.GetAsync(requestUri: $"http://localhost:5000/codes?stringValue={codeStringValue}");
        }
    }
}
