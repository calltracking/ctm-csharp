namespace CTM {
	class Config {
		public static string Endpoint() {
			string endpoint = System.Environment.GetEnvironmentVariable("CTM_TEST_ENDPOINT");
			if (endpoint == null) {
				return "https://api.calltrackingmetrics.com/api/v1";
			} else {
				return endpoint;
			}
		}
	}
}
