using System;
using System.Collections;

namespace CTM {
  public class AuthToken {
    public string account_id;
    public string auth_token;

    public AuthToken(string token, string first_account_id) {
      auth_token = token;
      account_id = first_account_id;
    }

    public static AuthToken authorize(string api_key, string api_secret) {
      string url = CTM.Config.Endpoint() + "/authentication.json";
      Console.WriteLine(url);

      Hashtable options = new Hashtable();
      options["token"]  = api_key;
      options["secret"] = api_secret;

      CTM.Request req  = new CTM.Request(url);
      CTM.Response res = req.post(options);

      return new AuthToken((string)res.data.token, (string)res.data.first_account.id);
    }

    /*
     * For master account's switch to auth token account
     */
    public void switch_account(string account_id) {
      this.account_id = account_id;
    }

  }
}
