/*
  CTM Sources API
*/
using CTM;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace CTM {
  public class Source {
    public string id;
    public string name;
    public string referring_url;
    public string not_referrer_url;
    public string landing_url;
    public string not_landing_url;
    public bool   online;
    public int?   position;

    public AuthToken token;
    public string    error;

    /*
     * Construct an empty Source for error returns
     */
    public Source(string error, CTM.AuthToken token){
      this.token = token;
      this.error = error;
    }

    /*
     * Construct from CTM.Response data
     */
    public Source(JObject src, CTM.AuthToken token){
      this.token = token;
      this.update_from(src);
    }

    /*
     * Construct attatched to a number
     */
    public Source(Number number,        string name,
                  string referring_url, string not_referrer_url,
                  string landing_url,   string not_landing_url,
                  int    position,      bool   online) {

      this.token            = number.token;
      this.name             = name;
      this.referring_url    = referring_url;
      this.not_referrer_url = not_referrer_url;
      this.landing_url      = landing_url;
      this.not_landing_url  = not_landing_url;
      this.online           = online;
      this.position         = position;

      if (number.id == ""){
        this.error = "Number has no id";
        return;
      }

      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + number.id + "/tracking_sources.json";

      CTM.Response res = new CTM.Request(url, token).post(this.params_hash());

      if (res.error != null){
        this.error = res.error;
      }else{
        this.update_from(res.data.source);
      }
    }

    /*
     * Helper for parsing JObjects
     */
    private void update_from(JObject src){
      this.token.account_id = src.Value<string>("account_id");

      this.id               = src.Value<string>("id");
      this.name             = src.Value<string>("name");
      this.referring_url    = src.Value<string>("referring_url");
      this.not_referrer_url = src.Value<string>("not_referrer_url");
      this.landing_url      = src.Value<string>("landing_url");
      this.not_landing_url  = src.Value<string>("not_landing_url");
      this.online           = src.Value<bool>("online");
      this.position         = src.Value<int?>("position");
    }

    /*
     * Helper for creating parameters
     */
    private Hashtable params_hash(){
      Hashtable parameters = new Hashtable();
      parameters["name"]             = this.name;
      parameters["referring_url"]    = this.referring_url;
      parameters["not_referrer_url"] = this.not_referrer_url;
      parameters["landing_url"]      = this.landing_url;
      parameters["not_landing_url"]  = this.not_landing_url;
      parameters["online"]           = this.online ? "1" : "0";
      parameters["position"]         = this.position.ToString();

      return parameters;
    }

    /*
     * Get source
     */
    public static Source get(CTM.AuthToken token, string id){
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/sources/" + id + ".json";

      CTM.Response res = new CTM.Request(url, token).get();

      if (res.error != null){
        return new Source(res.error, token);
      }else{
        return new Source(res.data, token);
      }
    }

    /*
     * Reload source
     */
    public bool reload(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/sources/" + this.id + ".json";

      CTM.Response res = new CTM.Request(url, this.token).get();

      if (res.error != null){
        this.error = res.error;
      }else{
        this.update_from(res.data);
      }

      return res.error == null;
    }

    /*
     * Update source
     */
    public bool save(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/sources/" + this.id + ".json";

      CTM.Response res = new CTM.Request(url, this.token).put(this.params_hash());

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * Release the source
     */
    public bool release() {
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/sources/" + this.id + ".json";

      CTM.Request  req = new CTM.Request(url, this.token);
      CTM.Response res = req.delete();

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * List sources in the current account
     */
    public static Page<Source> list(CTM.AuthToken token, int page=0) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/sources.json";

      Hashtable parameters = new Hashtable();
      parameters["page"]   = page.ToString();

      CTM.Response res = new CTM.Request(url, token).get(parameters);

      if (res.error != null){
        return new Page<Source>(res.error);

      } else {
        int index = 0;
        Source[] sources = new Source[res.data.sources.Count];

        foreach (JObject item in res.data.sources.Children<JToken>()) {
          sources[index++] = new Source(item, token);
        }
        return new Page<Source>(sources, page, (int)res.data.total_entries, (int)res.data.total_pages);
      }
    }

  }
}
