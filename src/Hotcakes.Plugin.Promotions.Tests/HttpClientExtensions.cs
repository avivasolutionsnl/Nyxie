using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Xunit;

namespace Hotcakes.Plugin.Promotions.Tests
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string requestUri, T body)
        {
            string json = JsonConvert.SerializeObject(body);

            return await client.PostAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"));
            ;
        }

        public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string requestUri)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, requestUri);

            return await client.SendAsync(message);
        }

        public static async Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string requestUri, T body)
        {
            string json = JsonConvert.SerializeObject(body);

            return await client.PutAsync(requestUri, new StringContent(json, Encoding.UTF8, "application/json"));
            ;
        }

        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string requestUri,
            Dictionary<string, string> headers = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, requestUri);

            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                    message.Headers.Add(header.Key, header.Value);

            HttpResponseMessage response = await client.SendAsync(message);

            Assert.True(response.IsSuccessStatusCode);

            string responseAsString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseAsString);
        }

        public static async Task<HttpResponseMessage> DeleteJsonAsync<T>(this HttpClient client, string requestUri, T body)
        {
            string json = JsonConvert.SerializeObject(body);

            var message = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return await client.SendAsync(message);
        }
    }
}
