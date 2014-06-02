using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace CTM {
  class Response {
    public string  body;
    public dynamic data;
    public string  error;

    public Response(HttpWebResponse res){
      StreamReader web = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
      this.body = web.ReadToEnd();
      web.Close();

      try{
        this.data = JObject.Parse(this.body);
        this.extract_error();

      } catch (Newtonsoft.Json.JsonReaderException){
        //Console.WriteLine(e.Message);
        this.error = ((int)res.StatusCode) + " " + res.StatusDescription;
      }
    }

    public Response(string _body) {
      this.body = _body;

      try{
        this.data = JObject.Parse(_body);
        this.extract_error();

      } catch (Newtonsoft.Json.JsonReaderException e){
        this.error = "Json.NET parse error: " + e.Message;
        return;
      }
    }

    void extract_error(){
      if (this.data == null){ return; }

      bool has_error     = false;
      JArray error_array = null;

      dynamic status_obj = this.data.GetValue("status");
      if (status_obj != null){
        string status_type = status_obj.GetType().ToString();
        string status = null;

        switch (status_type) {
        case "Newtonsoft.Json.Linq.JValue": status = this.data.Value<string>("status"); break;
        case "Newtonsoft.Json.Linq.JArray": status = (string)status_obj.First;          break;
        }

        if (status != null && status == "error"){ has_error = true; }
      }

      if (this.data.Value<bool?>("success") == false){ has_error = true; }
      if (this.data.Value<string>("error")  != null ){ has_error = true; }
      if (this.data.Value<JArray>("errors") != null ){
        has_error   = true;
        error_array = this.data.Value<JArray>("errors");
      }

      if (has_error){
        if (error_array != null){
          this.error = String.Join("\n", error_array.Values<string>());

        }else{
          this.error =
            this.data.Value<string>("reason")  ??
            this.data.Value<string>("text")    ??
            this.data.Value<string>("message") ??
            this.data.Value<string>("error")   ??
            "unknown server error";
        }
      }
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
        paramDataBytes     = encoding.GetBytes(query);
        _req.ContentType   = "application/x-www-form-urlencoded";
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

      HttpWebResponse res = null;

      try {
        res = (HttpWebResponse)_req.GetResponse();

      } catch(WebException e) {
        res = (HttpWebResponse)e.Response;
      }

      Response  ctmRes = new Response(res);
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
