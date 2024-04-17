using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace E2ETest {
	public static class Extensions {
		public static async Task<HttpResponseMessage> PostJsonWithBearer<T>(this HttpClient client, string url, string accessToken, T body) {
			var content = JsonContent.Create(body);
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url);
			message.Content = content;
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			return await client.SendAsync(message);
		}

		public static async Task<HttpResponseMessage> GetWithBearer(this HttpClient client, string url, string accessToken) {
			var message = new HttpRequestMessage(HttpMethod.Get, url);
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			return await client.SendAsync(message);
		}
	}
}
