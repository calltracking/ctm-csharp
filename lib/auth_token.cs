using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace CTM {
  public class AuthToken {
    public string account_id;
    public string auth_token;
    public string error;

    public AuthToken(string error_message){
      this.error = error_message;
    }

    public AuthToken(string token, string first_account_id) {
      auth_token = token;
      account_id = first_account_id;
    }

    public static AuthToken authorize(string api_key, string api_secret) {
      string url = CTM.Config.Endpoint() + "/authentication.json";

      // if (api_key == null || api_secret == null){
      //   Console.WriteLine("Error: missing API_KEY, API_SECRET ");
      //   return null;
      // }

      Hashtable options = new Hashtable();

      options["token"]  = api_key;
      options["secret"] = api_secret;

      CTM.Request  req = new CTM.Request(url);
      CTM.Response res = req.post(options);

      return (res.error != null ? new AuthToken(res.error)
                                : new AuthToken((string)res.data.token,
                                                (string)res.data.first_account.id)
              );
    }

    /*
     * For master account's switch to auth token account
     */
    public void switch_account(string account_id) {
      this.account_id = account_id;
    }

  }
}
