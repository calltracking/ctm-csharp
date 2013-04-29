using System;
using System.Collections;
using System.Collections.Generic;
using System.Json;

class Log {
  public static void Info(string msg) {
    Console.WriteLine(msg);
  }
  public static void Error(string msg) {
    Console.WriteLine(msg);
  }
}
 
namespace CTM
{
  /// <summary>
  /// This is began as ctm example code and SDL updated it.
  /// source https://github.com/calltracking/ctm-csharp
  /// </summary>
  public class BuyNumber
  {
    public int _id;
    public string Number;
    public AuthToken Token;
 
    public BuyNumber(int id, string _number, AuthToken authToken)
    {
      _id = id;
      Number = _number;
      Token = authToken;
    }
 
    public static BuyNumber buy(AuthToken token, string number)
    {
      string url = config.Endpoint() + "/accounts/" + token.account_id + "/numbers.json";
      var request = new Request(url, token);
      var parameters = new Hashtable();
      parameters["phone_number"] = number;
      Response res = request.post(parameters);
      if ((string)res.data["status"] == "success")
      {
        return new BuyNumber((int)res.data["number"]["id"], (string)res.data["number"]["number"], token);
      }
      else
      {
        return null;
      }
    }
 
    public bool AddReceivingNumber(string number)
    {
      log.Info("Begin add receiving number " + number + " for " +this._id);
      if (_id < 1)
        return false;
      var url = config.Endpoint() + "/accounts/" + Token.account_id + "/numbers/" + this._id + "/receiving_numbers.json";
      try
      {
        var request = new Request(url, Token);
        var parameters = new Hashtable();
        parameters["number"] = number;
        var res = request.post(parameters);
        var wasSuccess = ((string) res.data["status"]).ToLower() == "success";
        log.Info(wasSuccess ? "Successfully added receiving number  " + number + " for " + this._id : "Failed to add receiving number " + number + " for " +this._id);
        return wasSuccess;
      } catch (Exception problem)
      {
        log.Error("Unable to add receiving number " + number + " for id " + _id + "", problem);
      }
      return false;
    }
 
    public Source addTrackingSource(string name, string referrer, string location, int position)
    {
      Source source = new Source(this.Token, name, referrer, location, position);
      source.addToNumber(this);
      return source;
    }
  }
}
