using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E2ETest {
	internal class AccountInfo {
		public required string Email { get; set; }
		public required string Password { get; set; }
		public required string AccessToken { get; set; }
	}

	public class WatchAuthTest : IClassFixture<ClientFixture> {
		private readonly ClientFixture _fixture;
		private readonly HttpClient _client;

		public WatchAuthTest(ClientFixture clientFixture) {
			_fixture = clientFixture;
			_client = _fixture.Client;
		}

		private async Task<AccountInfo> CreateAccount() {
			var email = $"{RandomNumberGenerator.GetHexString(16)}@chillchaser.ovh";
			var password = "abcABC1!";

			await _fixture.Client.PostAsJsonAsync("/register", new {
				email,
				password
			});

			var loginResponse = await _fixture.Client.PostAsJsonAsync("/login", new {
				email,
				password
			});

			var accessToken = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

			return new AccountInfo {
				Email = email,
				Password = password,
				AccessToken = accessToken.AccessToken
			};
		}

		private async Task<InfoResponse> GetInfo(string accessToken) {
			var infoResponse = await _fixture.Client.GetWithBearer("/manage/info", accessToken);

			return await infoResponse.Content.ReadFromJsonAsync<InfoResponse>();
		}

		[Fact(Timeout = 10000)]
		public async Task AuthWatch_Simple() {
			//Create account
			var account = await CreateAccount();
			var token = "abc123";

			var loginTask = _fixture.Client.PostAsJsonAsync("/api/watch/login", new {
				Token = token
			});

			await Task.Delay(250);

			var authorizeResponse = await _fixture.Client.PostJsonWithBearer("/api/watch/authorize", account.AccessToken, new {
				Token = token
			});

			var loginResponse = await loginTask;

			var acquiredAccessToken = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

			var info = await GetInfo(acquiredAccessToken.AccessToken);

			Assert.Equal(HttpStatusCode.OK, authorizeResponse.StatusCode);
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.NotNull(info);
			Assert.Equal(account.Email, info.Email);
		}

		[Fact(Timeout = 10000)]
		public async Task AuthWatch_duplicateToken() {
			var account = await CreateAccount();
			var token = "abc124";

			var cancelationToken = new CancellationTokenSource();

			var loginTask = _fixture.Client.PostAsJsonAsync("/api/watch/login", new {
				Token = token
			}, cancelationToken.Token);

			await Task.Delay(250);

			var duplicateResponse = await _fixture.Client.PostAsJsonAsync("/api/watch/login", new {
				Token = token
			});

			await _fixture.Client.PostJsonWithBearer("/api/watch/authorize", account.AccessToken, new {
				Token = token
			});

			var loginResponse = await loginTask;

			var acquiredAccessToken = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

			var info = await GetInfo(acquiredAccessToken.AccessToken);

			Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
			Assert.Equal("\"TokenAlreadyUsed\"", await duplicateResponse.Content.ReadAsStringAsync());
			Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
			Assert.Equal(account.Email, info.Email);
		}

		[Fact(Timeout = 10000)]
		public async Task AuthWatch_InvalidToken() {
			var account = await CreateAccount();

			var response = await _fixture.Client.PostJsonWithBearer("/api/watch/authorize", account.AccessToken, new {
				Token = "bafarfe"
			});
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			Assert.Equal("\"UnknownToken\"", await response.Content.ReadAsStringAsync());
		}

		[Fact(Timeout = 10000)]
		public async Task AuthWatch_TokenGetsFreed() {
			var account = await CreateAccount();
			var account2 = await CreateAccount();
			var token = "grlokgar";
			var loginTask = _fixture.Client.PostAsJsonAsync("/api/watch/login", new {
				Token = token
			});

			await Task.Delay(250);

			await _fixture.Client.PostJsonWithBearer("/api/watch/authorize", account.AccessToken, new {
				Token = token
			});

			var loginResponse = await loginTask;
			var acquiredAccessToken = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

			var loginTask2 = _fixture.Client.PostAsJsonAsync("/api/watch/login", new {
				Token = token
			});

			await Task.Delay(250);

			await _fixture.Client.PostJsonWithBearer("/api/watch/authorize", account2.AccessToken, new {
				Token = token
			});

			var loginResponse2 = await loginTask2;
			var acquiredAccessToken2 = await loginResponse2.Content.ReadFromJsonAsync<AccessTokenResponse>();

			var info = await GetInfo(acquiredAccessToken.AccessToken);
			var info2 = await GetInfo(acquiredAccessToken2.AccessToken);


			Assert.Equal(account.Email, info.Email);
			Assert.Equal(account2.Email, info2.Email);
		}
	}
}
