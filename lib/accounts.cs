/*
  CTM Accounts API
*/
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CTM {
  public class Account {
    public string id;

    public string name;
    public string website;
    public string timezone;

    public string status;
    public bool   shared_billing;
    public int    balance;  // US Cents

    public string        error;
    public CTM.AuthToken token;

    public Account(CTM.AuthToken token){
      this.token = token;
    }

    public Account(string id, string name, string status, CTM.AuthToken auth_token) {
      this.id     = id;
      this.token  = auth_token;
      this.name   = name;
      this.status = status;
    }

    /*
     * Construct from CTM.Response data
     */
    public Account(JObject src, CTM.AuthToken token){
      this.token = token;

      dynamic status_obj = src.GetValue("status");
      string status_type = status_obj.GetType().ToString();

      if      (status_type == "Newtonsoft.Json.Linq.JValue"){ this.status = src.Value<string>("status"); }
      else if (status_type == "Newtonsoft.Json.Linq.JArray"){ this.status = (string)status_obj.First;    }

      this.id       = src.Value<string>("id");
      this.name     = src.Value<string>("name");
      this.website  = src.Value<string>("website");
      this.timezone = src.Value<string>("timezone");

      this.shared_billing = src.Value<bool>("shared_billing");

      JToken balance = src.GetValue("balance");
      if (balance != null) this.balance = balance.Value<int>("cents");
    }

    /*
     * Return a special account for dealing with error messages without crossing types
     */
    public static Account ErrorAccount(CTM.AuthToken token, string error, string account_id = null){
      Account account = new Account(account_id, "", "", token);
      account.error = error;
      return account;
    }

    /*
     * Get the stats for a single account
     */
    public static Account get(CTM.AuthToken token, string account_id){
      CTM.Request  request = new CTM.Request(CTM.Config.Endpoint() + "/accounts/" + account_id + ".json", token);
      CTM.Response res     = request.get(new Hashtable());

      if (res.error){
        return ErrorAccount(token, res.error_text, account_id);
      }

      return new Account(res.data, token);
    }

    /*
     * List accounts accessible to the given auth token
     */
    public static Page<Account> list(CTM.AuthToken token, int page=0, string status="active") {
      CTM.Request request  = new CTM.Request(CTM.Config.Endpoint() + "/accounts.json", token);

      Hashtable parameters = new Hashtable();
      parameters["page"]   = page.ToString();
      parameters["status"] = status.ToString();

      CTM.Response res = request.get(parameters);

      if (res.error){
        Account[] accounts = new Account[1];
        accounts[0] = ErrorAccount(token, res.error_text);
        return new Page<Account>(accounts, 0, 1, 1);

      } else{
        int index = 0;
        Account[] accounts = new Account[res.data.accounts.Count];

        foreach (JObject account in res.data.accounts.Children<JToken>()) {
          accounts[index++] = new Account(account, token);
        }
        return new Page<Account>(accounts, page, (int)res.data.total_entries, (int)res.data.total_pages);
      }
    }

    /*
     * Update the account
     */
    public bool save(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.id + ".json";

      Hashtable parameters  = new Hashtable();
      parameters["account[name]"]     = this.name;
      parameters["account[website]"]  = this.website;
      parameters["account[timezone]"] = this.timezone;

      CTM.Request  request = new CTM.Request(url, token);
      CTM.Response res     = request.put(parameters);

      if (res.error){ Console.WriteLine("Error: " + res.error_text); }
      return res.error;
    }


    /*
     * Add new account with billing linked to master account
     */
    public Account create() {
      CTM.Request request = new CTM.Request(CTM.Config.Endpoint() + "/accounts.json", token);

      Hashtable parameters        = new Hashtable();
      parameters["account[name]"]        = this.name;
      parameters["account[website_url]"] = this.website;
      parameters["account[timezone]"]    = this.timezone;
      parameters["billing_type"]         = this.shared_billing ? "existing" : "new";

      CTM.Response res = request.post(parameters);

      Console.WriteLine(res.body);

      if (res.error){ return ErrorAccount(token, res.error_text); }

      return new Account(res.data, token);
    }

  }
}
