using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Web;
using CTM;

namespace Examples {

  public class ReportHTTPServer : HttpServer {
    public ReportHTTPServer(int port) : base(port) {
    }
    public override void OnResponse(ref HttpRequestData req, ref HttpResponseData res) {
      Console.WriteLine("request: {0}", req.URL);
      res.status = (int)RespState.OK;
      string bodyStr = "hello world\r\n\r\n";
      res.BodyData = Encoding.ASCII.GetBytes(bodyStr);
    }
  }

  class ExampleServer {
    static void Main(string[] args) {
      HttpServer httpServer = new ReportHTTPServer(8888);
      Thread thread = new Thread(new ThreadStart(httpServer.listen));
      thread.Start(); 
    }
  }

}
