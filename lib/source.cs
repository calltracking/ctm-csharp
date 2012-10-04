/*
  CTM Sources API
*/
using System;
using System.Json;
using System.Collections;
using System.Collections.Generic;

namespace CTM {
  public class Source {
    public AuthToken token;
    public string name;
    public string referrer;
    public string location;
    public int position;

    public Source(AuthToken token, string name, string referrer, string location, int position) {
      this.token = token;
      this.name = name;
      this.referrer = referrer;
      this.location = location;
      this.position = position;
    }

    public bool addToNumber(Number number) {
      if (number.id < 1) { return false; }
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/" + number.id + "/tracking_sources.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["name"] = this.name;
      parameters["ref_pattern"] = this.referrer;
      parameters["url_pattern"] = this.location;
      parameters["position"] = this.position.ToString();
      CTM.Response res = request.post(parameters);
      Console.WriteLine(res.body);
      if ((string)res.data["status"] == "success") {
        return true;
      } else {
        return false;
      }

    }

  }
}
