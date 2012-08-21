using System;
using System.Collections;

namespace CTM {
  public class AuthToken {
    public int account_id;
    public string auth_token;

    public AuthToken(string token, int first_account_id) {
      auth_token = token;
      account_id = first_account_id;
    }

    public static AuthToken authorize(string api_key, string api_secret) {
      string url = CTM.Config.Endpoint() + "/authentication.json";
      Console.WriteLine(url);
      CTM.Request req = new CTM.Request(url);
      Hashtable options = new Hashtable();
      options["token"] = api_key;
      options["secret"] = api_secret;
      CTM.Response res = req.post(options);
 
      return new AuthToken((string)res.data["token"], (int)res.data["first_account"]["id"]);
    }

  }
}
