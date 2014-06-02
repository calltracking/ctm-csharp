/*
  CTM Receiving Numbers API
*/
using CTM;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace CTM{
  public class ReceivingNumber{
    public string    id;
    public string    filter_id;
    public string    name;
    public string    number;
    public string    display_number;
    public string    country_code;
    public string    formatted;

    public string        error;
    public CTM.AuthToken token;

    /*
     * Contruct to return errors
     */
    public ReceivingNumber(string error, CTM.AuthToken token){
      this.token = token;
      this.error = error;
    }

    /*
     * Construct from CTM.Response data
     */
    public ReceivingNumber(JObject src, CTM.AuthToken token){
      this.token = token;
      this.update_from(src);
    }

    /*
     * Construct new attached to Number
     */
    public ReceivingNumber(Number number, string digits){
      this.token = number.token;

      if (number.id == ""){
        this.error = "Number has no id";
        return;
      }

      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/numbers/" + number.id + "/receiving_numbers.json";

      Hashtable parameters = new Hashtable();
       parameters["number"] = number.number;

       CTM.Request  req = new CTM.Request(url, number.token);
       CTM.Response res = req.post(parameters);

       if (res.error != null){
         this.error = res.error;

       } else {
         this.update_from(res.data.receiving_number);
      }
    }

    /*
     * Helper for parsing JObjects
     */
    private void update_from(JObject src){
      this.token.account_id = src.Value<string>("account_id");

      this.id             = src.Value<string>("id");
      this.filter_id      = src.Value<string>("filter_id");
      this.name           = src.Value<string>("name");
      this.number         = src.Value<string>("number");
      this.display_number = src.Value<string>("display_number");
      this.country_code   = src.Value<string>("country_code");
      this.formatted      = src.Value<string>("formatted");
    }

    /*
     * GET a receiving number by id
     */
    public static ReceivingNumber get(CTM.AuthToken token, string id) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/receiving_numbers/" + id + ".json";

      CTM.Request request = new CTM.Request(url, token);
      CTM.Response res    = request.get();

      if (res.error != null){
        return new ReceivingNumber(res.error, token);
      }
      return new ReceivingNumber(res.data, token);
    }

    /*
     * Update a receiving number
     */
    public bool save(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/receiving_numbers/" + this.id + ".json";

      Hashtable parameters = new Hashtable();
      parameters["name"]   = this.name;
      parameters["number"] = this.number;

      CTM.Request  req = new CTM.Request(url, this.token);
      CTM.Response res = req.put(parameters);

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * Reload the receiving number
     */
    public bool reload(){
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/receiving_numbers/" + this.id + ".json";

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
     * Release the receiving_number
     */
    public bool release() {
      string url = CTM.Config.Endpoint() + "/accounts/" + this.token.account_id + "/receiving_numbers/" + this.id + ".json";

      CTM.Request  req = new CTM.Request(url, this.token);
      CTM.Response res = req.delete();

      if (res.error != null){ this.error = res.error; }
      return res.error == null;
    }

    /*
     * List receiving numbers in the current account
     */
    public static Page<ReceivingNumber> list(CTM.AuthToken token, int page=0) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/receiving_numbers.json";

      Hashtable parameters = new Hashtable();
      parameters["page"]   = page.ToString();

      CTM.Request  req = new CTM.Request(url, token);
      CTM.Response res = req.get(parameters);

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

  }
}
