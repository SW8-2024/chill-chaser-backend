using Xunit.Abstractions;

namespace E2ETest {
	public class SmokeTest : IClassFixture<ClientFixture> {
		private readonly ClientFixture _fixture;
		private readonly HttpClient _client;

		public SmokeTest(ClientFixture clientFixture) {
			_fixture = clientFixture;
			_client = _fixture.Client;
		}

		[Fact]
		public async Task IsIndexAvailable() {
			var res = await _client.GetAsync("/");
			Assert.Equal(System.Net.HttpStatusCode.OK, res.StatusCode);
		}
	}
}