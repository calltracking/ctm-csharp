/*
  CTM Numbers API
*/
using System;
using System.Json;
using System.Collections;
using System.Collections.Generic;

namespace CTM {
  public class Number {
    public int id;
    public string name;
    public string number;
    public CTM.AuthToken token;

    public Number(int id, string _number, CTM.AuthToken auth_token, string name=null) {
      this.id = id;
      number = _number;
      token = auth_token;
      this.name = name;
    }

    /*
     * GET a number by id
     */
    public static Number get(CTM.AuthToken token, int id) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + id.ToString() + ".json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      CTM.Response res = request.get(parameters);
      Number number = new Number((int)res.data["id"], (string)res.data["number"], token, (string)res.data["name"]);
      return number;
    }

    /*
     * Update the number e.g. save the name
     */
    public bool save() {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + this.id.ToString() + ".json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["name"] = this.name;
      CTM.Response res = request.put(parameters);
      Console.WriteLine(res.body);
      return false;
    }

    /*
     * Find numbers available for purchase within the given areacode and country code
     */
    public static Number[] search(CTM.AuthToken token, string areacode, string country_code="US", string pattern="") {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/search.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["area_code"] = areacode;
      parameters["searchby"] = "area";
      parameters["country_code"] = country_code;
      parameters["pattern"] = pattern;
      CTM.Response res = request.get(parameters);
      int index = 0;
      Number[] numbers;
      if (res.data.ContainsKey("results")) {
        numbers = new Number[res.data["results"].Count];
        foreach (KeyValuePair<string,System.Json.JsonValue> number in res.data["results"]) {
          numbers[index++] = new Number(-1, (string)number.Value["phone_number"], token);
        }
      } else {
        numbers = new Number[0];
      }
      return numbers;
    }

    /*
     * Find numbers available for purchase within the given areacode and country code
     * toll free is US or UK
     */
    public static Number[] search_tollfree(CTM.AuthToken token, string areacode, string country_code="US", string pattern="") {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/search.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["area_code"] = areacode;
      parameters["searchby"] = "tollfree";
      parameters["country_code"] = country_code;
      parameters["pattern"] = pattern;
      CTM.Response res = request.get(parameters);
      int index = 0;
      Number[] numbers = new Number[res.data["results"].Count];
      foreach (KeyValuePair<string,System.Json.JsonValue> number in res.data["results"]) {
        numbers[index++] = new Number(-1, (string)number.Value["phone_number"], token);
      }
      return numbers;
    }

    /*
     * Purchase a number, the number should be the full digit string from the .number within the list of numbers returned from
     * Number#search
     */
    public static Number buy(CTM.AuthToken token, string number) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["phone_number"] = number;
      CTM.Response res = request.post(parameters);
      if ((string)res.data["status"] == "success") {
        return new Number((int)res.data["number"]["id"], (string)res.data["number"]["number"], token);
      } else {
        return null;
      }
    }

    /*
     * List numbers in the current account
     */
    public static Page<Number> list(CTM.AuthToken token, int page=0) {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["page"] = page.ToString();
      CTM.Response res = request.get(parameters);
      int index = 0;
      Number[] numbers = new Number[res.data["numbers"].Count];
      foreach (KeyValuePair<string,System.Json.JsonValue> number in res.data["numbers"]) {
        numbers[index++] = new Number((int)number.Value["id"], (string)number.Value["number"], token);
      }
      return new Page<Number>(numbers, page, (int)res.data["total_pages"], (int)res.data["total_pages"]);
    }

    public bool release() {
      return true;
    }

    public bool addReceivingNumber(string number) {
      if (this.id < 1) { return false; }
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + this.id + "/receiving_numbers.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["number"] = number;
      CTM.Response res = request.post(parameters);
      if ((string)res.data["status"] == "success") {
        return true;
      } else {
        return false;
      }
    }
    public Source addTrackingSource(string name, string referrer, string location, int position) {
      Source source = new Source(this.token, name, referrer, location, position);
      source.addToNumber(this);
      return source;
    }
  }
}
