namespace CTM {
  class Config {
    public static string Endpoint() {
      string endpoint = System.Environment.GetEnvironmentVariable("CTM_API_URL");
      if (endpoint == null) {
        return "https://api.calltrackingmetrics.com/api/v1";
      } else {
        return endpoint;
      }
    }
  }
}
