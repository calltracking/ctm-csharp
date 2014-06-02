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
      this.update_from(src);
    }

    /*
     * Helper for parsing JObjects
     */
    private void update_from(JObject src){
      dynamic status_obj = src.GetValue("status");
      string status_type = status_obj.GetType().ToString();

      switch(status_type){
      case "Newtonsoft.Json.Linq.JValue": this.status = src.Value<string>("status"); break;
      case "Newtonsoft.Json.Linq.JArray": this.status = (string)status_obj.First;    break;
      }

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

      if (res.error != null){
        return ErrorAccount(token, res.error, account_id);
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

      if (res.error != null){
        return new Page<Account>(res.error);

      } else {
        int index = 0;
        Account[] accounts = new Account[res.data.accounts.Count];

        foreach (JObject account in res.data.accounts.Children<JToken>()) {
          accounts[index++] = new Account(account, token);
        }
        return new Page<Account>(accounts, (int)res.data.page, (int)res.data.total_entries, (int)res.data.total_pages);
      }
    }

    /*
     * Update the account
     */
    public bool save(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.id + ".json";

      Hashtable parameters            = new Hashtable();
      parameters["account[name]"]     = this.name;
      parameters["account[website]"]  = (this.website == null ? "" : this.website);
      parameters["account[timezone]"] = this.timezone;

      CTM.Request  request = new CTM.Request(url, token);
      CTM.Response res     = request.put(parameters);

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * Reload the account data
     */
    public bool reload(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.id + ".json";

      CTM.Request  req = new CTM.Request(url, token);
      CTM.Response res = req.get();

      if (res.error != null){
        this.error = res.error;
      } else{
        this.update_from(res.data);
      }

      return res.error == null;
    }

    /*
     * Add new account with billing linked to master account
     */
    public Account create() {
      string url = CTM.Config.Endpoint() + "/accounts.json";

      Hashtable parameters        = new Hashtable();
      parameters["account[name]"]        = this.name;
      parameters["account[website_url]"] = this.website;
      parameters["account[timezone]"]    = this.timezone;
      parameters["billing_type"]         = this.shared_billing ? "existing" : "new";

      CTM.Response res = new CTM.Request(url, token).post(parameters);

      if (res.error != null){ return ErrorAccount(token, res.error); }

      return new Account(res.data, token);
    }

  }
}
