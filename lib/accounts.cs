/*
	CTM Accounts API
*/
using System;
using System.Json;
using System.Collections;
using System.Collections.Generic;

namespace CTM {
	public class Account {
		public int id;
    public string name;
    public string status;
    public CTM.AuthToken token;

		public Account(int id, string name, string status, CTM.AuthToken auth_token) {
			this.id = id;
      token = auth_token;
      this.name = name;
      this.status = status;
		}

    /*
     * List accounts accessible to the given auth token
     */
		public static Page<Account> list(CTM.AuthToken token, int page=0, string status="active") {
      string url = CTM.Config.Endpoint() + "/accounts.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["page"] = page.ToString();
      parameters["status"] = status.ToString();
      CTM.Response res = request.get(parameters);
      int index = 0;
      Account[] accounts = new Account[res.data["accounts"].Count];
      foreach (KeyValuePair<string,System.Json.JsonValue> account in res.data["accounts"]) {
        accounts[index++] = new Account((int)account.Value["id"], (string)account.Value["name"], (string)account.Value["status"], token);
      }
      return new Page<Account>(accounts, page, (int)res.data["total_pages"], (int)res.data["total_pages"]);
    }

    /*
     * Add new account with billing linked to master account
     */
    public static Account create_linked(CTM.AuthToken token, string name) {
      string url = CTM.Config.Endpoint() + "/accounts.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["account[name]"] = name.ToString();
      parameters["billing_type"] = "existing";
      CTM.Response res = request.post(parameters);
      Console.WriteLine(res.body);
      return new Account((int)res.data["id"], (string)res.data["name"], "active", token);
    }

	}
}
