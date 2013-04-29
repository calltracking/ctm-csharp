using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Json;

namespace CTM {
  class Response {
    public string body;
    public JsonValue data;

    public Response(string _body) {
      body = _body;
      //var json = JsonValue.Parse(@"{""simple"":""value1"",""complex"":{""name"":""value2"",""id"":""value3""}}");
      data = JsonValue.Parse(_body);
    }
  }
  class Request {
    string _url;
    HttpWebRequest _req;
    AuthToken _token;

    public Request(string url, CTM.AuthToken token=null) {
      _token = token;
      _url = url;
      _req = (HttpWebRequest)WebRequest.Create(url);
    }

    public Response request(string method, Hashtable headers, Hashtable parameters) {
      string[] queryList = new string[parameters.Keys.Count];
      int index = 0;
      ASCIIEncoding encoding = new ASCIIEncoding();
      byte[] paramDataBytes = encoding.GetBytes("");
      foreach (DictionaryEntry kv in parameters) {
        queryList[index++] = HttpUtility.UrlEncode((string)kv.Key) + "=" + HttpUtility.UrlEncode((string)kv.Value);
      }
      String query = String.Join("&", queryList);

      if (method == "GET") {
        _url = _url + "?" + query;
        _req = (HttpWebRequest)WebRequest.Create(_url);
      } else {
        paramDataBytes = encoding.GetBytes(query);
        _req.ContentType = "application/x-www-form-urlencoded";
        _req.ContentLength = paramDataBytes.Length;
      }
      //_req.AllowAutoRedirect = true;
      _req.UserAgent = "CTM/1.0 .NET-SDK";
      //Console.WriteLine("Request: " + method + " : " + _url);

      foreach (DictionaryEntry kv in headers) {
        _req.Headers.Add((String)kv.Key, (String)kv.Value);
      }
      _req.Method = method;

      if (method != "GET" && paramDataBytes.Length > 0) {
        Stream stream = _req.GetRequestStream();
        stream.Write(paramDataBytes, 0, paramDataBytes.Length);
        stream.Close();
      }
      WebResponse res = null;
      try {
        res = _req.GetResponse();
      } catch(WebException e) {
        res = e.Response;
      }

      StreamReader web = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
      Response ctmRes = new Response(web.ReadToEnd());
      web.Close();
      res.Close();
      return ctmRes;
    }

    public Response get(Hashtable parameters=null) {
      return custom("GET", parameters);
    }
    public Response post(Hashtable parameters=null) {
      return custom("POST", parameters);
    }
    public Response put(Hashtable parameters=null) {
      return custom("PUT", parameters);
    }
    public Response delete(Hashtable parameters=null) {
      return custom("DELETE", parameters);
    }

    public Response custom(string method, Hashtable parameters=null) {
      Hashtable headers = new Hashtable();
      if (_token != null) {
        if (parameters == null) { parameters = new Hashtable(); }
        parameters["auth_token"] = _token.auth_token; 
      }
      return request(method, headers, parameters); 
    }

  }
}
