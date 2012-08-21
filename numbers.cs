/*
	CTM Numbers API
*/
using System;
using System.Json;
using System.Collections;
using System.Collections.Generic;

namespace CTM {
	public class Number {
		int _id;
		public string number;
    public CTM.AuthToken token;

		public Number(int id, string _number, CTM.AuthToken auth_token) {
			_id = id;
			number = _number;
      token = auth_token;
		}

		public static Number[] search(CTM.AuthToken token, string areacode, string country_code="US") {
      string url = CTM.Config.Endpoint() + "/accounts/" + token.account_id + "/numbers/search.json";
      CTM.Request request = new CTM.Request(url, token);
      Hashtable parameters = new Hashtable();
      parameters["area_code"] = areacode;
      parameters["country_code"] = country_code;
      CTM.Response res = request.get(parameters);
      int index = 0;
      Number[] numbers = new Number[res.data["results"].Count];
      foreach (KeyValuePair<string,System.Json.JsonValue> number in res.data["results"]) {
        numbers[index++] = new Number(-1, (string)number.Value["phone_number"], token);
      }
      return numbers;
		}
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
//		public bool addReceivingNumber(string number) {
//		}
//		public Source addTrackingSource(string number) {
//		}
	}
}
