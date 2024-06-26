﻿using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E2ETest {
	public class ClientFixture : IDisposable {
		public HttpClient Client { get; private set; }
		private readonly WebApplicationFactory<Program>? _application;
		public ClientFixture() {
			if (Environment.GetEnvironmentVariable("INTEGRATED_ENVIRONMENT") == null) {
				_application = new WebApplicationFactory<Program>();
				Client = _application.CreateClient();
			} else {
				Client = new HttpClient {
					BaseAddress = new Uri("http://localhost:2345/")
				};
			}

		}

		public void Dispose() {
			Client.Dispose();
			_application?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
