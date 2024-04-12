using Xunit.Abstractions;

namespace E2ETest {
	public class SmokeTest : IClassFixture<ClientFixture> {
		private readonly ClientFixture _fixture;
		private readonly HttpClient _client;
		private readonly ITestOutputHelper _output;

		public SmokeTest(ClientFixture clientFixture, ITestOutputHelper output) {
			_fixture = clientFixture;
			_client = _fixture.Client;
			_output = output;
		}

		[Fact]
		public async Task IsIndexAvailable() {
			_output.WriteLine($"benis: {_fixture.benis}");
			var res = await _client.GetAsync("/");
			Assert.Equal(System.Net.HttpStatusCode.OK, res.StatusCode);
		}
	}
}