/*
  CTM Numbers API
*/
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CTM {
  public class Number {
    public string id;
    public string name;
    public bool   active;

    public string country_code;
    public string number;
    public string formatted;

    public DateTime next_billing_date;

    public string routing;

    public string error;
    public CTM.AuthToken token;

    public Number(string id, string _number, CTM.AuthToken auth_token, string name = null) {
      this.id     = id;
      this.number = _number;
      this.name   = name;

      this.token  = auth_token;
    }

    /*
     * Construct from CTM.Response data
     */
    public Number(JObject src, CTM.AuthToken token){
      this.token = token;
      this.update_from(src);
    }

    /*
     * Helper for parsing JObjects
     */
    private void update_from(JObject src){
      this.token.account_id = src.Value<string>("account_id");

      this.id     = src.Value<string>("id");
      this.name   = src.Value<string>("name");
      this.active = src.Value<bool>("active");

      this.country_code = src.Value<string>("country_code");
      this.number       = src.Value<string>("number");
      this.formatted    = src.Value<string>("formatted");

      string billing_date_str = src.Value<string>("next_billing_date");
      if (billing_date_str != null){
        this.next_billing_date = DateTime.Parse(billing_date_str).Date;
      }

      this.routing = src.Value<string>("routing");
    }

    /*
     * Return a special number for dealing with error messages without crossing types
     */
    public static Number ErrorNumber(CTM.AuthToken token, string error, string number_id = null){
      Number number = new Number(number_id, "", token);
      number.error = error;
      return number;
    }

    /*
     * GET a number by id
     */
    public static Number get(CTM.AuthToken token, string id) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + id + ".json";

      CTM.Request request = new CTM.Request(url, token);
      CTM.Response res    = request.get();

      if (res.error != null){
        return ErrorNumber(token, res.error, id);
      }
      return new Number(res.data, token);
    }

    /*
     * List numbers in the current account
     */
    public static Page<Number> list(CTM.AuthToken token, int page=0) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers.json";

      Hashtable parameters = new Hashtable();
      parameters["page"]   = page.ToString();

      CTM.Request request  = new CTM.Request(url, token);
      CTM.Response res     = request.get(parameters);

      if (res.error != null){
        return new Page<Number>(res.error);

      } else{
        int index = 0;
        Number[] numbers = new Number[res.data.numbers.Count];

        foreach (JObject number in res.data.numbers.Children<JToken>()) {
          numbers[index++] = new Number(number, token);
        }
        return new Page<Number>(numbers, page, (int)res.data.total_entries, (int)res.data.total_pages);
      }
    }

    /*
     * List receiving numbers on a number
     */
    public Page<ReceivingNumber> receiving_numbers(int page = 0){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + this.id + "/receiving_numbers.json";

      Hashtable parameters = new Hashtable();
      parameters["page"]   = page.ToString();

      CTM.Response res = new CTM.Request(url, token).get(parameters);

      if (res.error != null){
        return new Page<ReceivingNumber>(res.error);

      } else {
        int index = 0;
        ReceivingNumber[] numbers = new ReceivingNumber[res.data.receiving_numbers.Count];

        foreach (JObject number in res.data.receiving_numbers.Children<JToken>()) {
          numbers[index++] = new ReceivingNumber(number, token);
        }
        return new Page<ReceivingNumber>(numbers, page, (int)res.data.total_entries, (int)res.data.total_pages);
      }
    }

    /*
     * Update the number e.g. save the name
     */
    public bool save() {
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + this.id + ".json";

      Hashtable parameters = new Hashtable();
      parameters["name"]      = this.name;
      parameters["active"]    = this.active ? "1" : "0";
      parameters["formatted"] = this.formatted;

      CTM.Request  req = new CTM.Request(url, this.token);
      CTM.Response res = req.put(parameters);

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * Reload the number
     */
    public bool reload(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + this.id + ".json";

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
     * Data structure for returned results
     */
    public class SearchResult{
      public CTM.AuthToken token;

      public string phone_number;
      public string friendly_name;
      public float? latitude;
      public float? longitude;
      public string rate_center;
      public int?   lata;
      public string region;
      public string postal_code;
      public string iso_country;
      public bool   voice;
      public bool   sms;
      public bool   mms;

      public string error;

      public static SearchResult ErrorResult(CTM.AuthToken token, string error){
        return new SearchResult(token, error);
      }

      public SearchResult(CTM.AuthToken token, string error){
        this.token = token;
        this.error = error;
      }

      public SearchResult(JObject src, CTM.AuthToken token){
        this.token = token;

        this.phone_number  = src.Value<string>("phone_number");
        this.friendly_name = src.Value<string>("friendly_name");
        this.latitude      = src.Value<float?>("latitude");
        this.longitude     = src.Value<float?>("longitude");
        this.rate_center   = src.Value<string>("rate_center");
        this.lata          = src.Value<int?>("lata");
        this.region        = src.Value<string>("region");
        this.postal_code   = src.Value<string>("postal_code");
        this.iso_country   = src.Value<string>("iso_country");

        JToken capabilities = src.GetValue("capabilities");

        this.voice = capabilities.Value<bool>("voice");
        this.sms   = capabilities.Value<bool>("SMS");
        this.mms   = capabilities.Value<bool>("MMS");
      }
    }

    /*
     * Find numbers available for purchase within the given areacode and country code
     */
    public static SearchResult[] search(CTM.AuthToken token, string areacode, string country_code="US", string pattern="") {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/search.json";

      Hashtable parameters = new Hashtable();
      parameters["area_code"]    = areacode;
      parameters["searchby"]     = "area";
      parameters["country_code"] = country_code;
      parameters["pattern"]      = pattern;

      CTM.Request  req = new CTM.Request(url, token);
      CTM.Response res = req.get(parameters);

      SearchResult[] numbers;
      if (res.error != null){
        numbers = new SearchResult[1];
        numbers[0] = SearchResult.ErrorResult(token, res.error);

      } else {
        int index = 0;

        if (res.data.results != null) {
          numbers = new SearchResult[res.data.results.Count];
          foreach (JObject number in res.data.results.Children<JToken>()) {
            numbers[index++] = new SearchResult(number, token);
          }
        } else {
          numbers = new SearchResult[0];
        }
      }
      return numbers;
    }

    /*
     * Find numbers available for purchase within the given areacode and country code
     * toll free is US or UK
     */
    public static SearchResult[] search_tollfree(CTM.AuthToken token, string areacode = null, string country_code="US", string pattern="") {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/search.json";

      Hashtable parameters = new Hashtable();
      parameters["area_code"]    = areacode;
      parameters["searchby"]     = "tollfree";
      parameters["country_code"] = country_code;
      parameters["pattern"]      = pattern;

      CTM.Request  req = new CTM.Request(url, token);
      CTM.Response res = req.get(parameters);

      SearchResult[] numbers;
      if (res.error != null){
        numbers = new SearchResult[1];
        numbers[0] = SearchResult.ErrorResult(token, res.error);

      } else {
        int index = 0;

        if (res.data.results != null) {
          numbers = new SearchResult[res.data.results.Count];
          foreach (JObject number in res.data.results.Children<JToken>()) {
            numbers[index++] = new SearchResult(number, token);
          }
        } else {
          numbers = new SearchResult[0];
        }
      }
      return numbers;
    }

    /*
     * Purchase a number, the number should be the full digit string from the .number within the list of numbers returned from
     * Number#search
     */
    public static Number buy(CTM.AuthToken token, string number) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers.json";

      Hashtable parameters = new Hashtable();
      parameters["phone_number"] = number;

      CTM.Request  req = new CTM.Request(url, token);
      CTM.Response res = req.post(parameters);

      if (res.error != null) {
        return ErrorNumber(token, res.error);

      } else{
        return new Number(res.data.number, token);
      }
    }

    public bool release() {
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + this.id + ".json";

      CTM.Request  req = new CTM.Request(url, this.token);
      CTM.Response res = req.delete();

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    public ReceivingNumber addReceivingNumber(string number) {
      return new ReceivingNumber(this, number);
    }

    public bool remReceivingNumber(ReceivingNumber num){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + this.id + "/receiving_numbers/" + num.id + "/rem.json";

      CTM.Response res = new CTM.Request(url, this.token).delete();

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    public Source addTrackingSource(string name,
                                    string referring_url, string not_referring_url,
                                    string landing_url,   string not_landing_url,
                                    int    position,      bool   online) {
      return new Source(this,          name,
                        referring_url, not_referring_url,
                        landing_url,   not_landing_url,
                        position,      online);
    }
  }
}
