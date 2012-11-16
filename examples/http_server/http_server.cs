// originally based on http://www.codeproject.com/Articles/137979/Simple-HTTP-Server-in-C
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Web;

namespace Examples {
	public abstract class HttpServer  {
    protected int port;
    TcpListener listener;
    bool is_active = true;
		public Hashtable res_status;
		public string Name = "CTMCSharpAgent/1.0.*";

    public HttpServer(int port) {
      this.port = port;
      this.response_status_init();
    }
      
    public void listen() {
      this.listener = new TcpListener(port);
      this.listener.Start();

      while (is_active) {
        HttpRequest request = new HttpRequest(this.listener.AcceptTcpClient(), this);
        System.Threading.Thread thread = new System.Threading.Thread(new ThreadStart(request.process));
        thread.Name = "HTTP Request";
        thread.Start();
      }

    }
		public abstract void OnResponse(ref HttpRequestData rq, ref HttpResponseData rp);

		private void response_status_init() {
			this.res_status = new Hashtable();
	
			res_status.Add(200, "200 Ok");
			res_status.Add(201, "201 Created");
			res_status.Add(202, "202 Accepted");
			res_status.Add(204, "204 No Content");

			res_status.Add(301, "301 Moved Permanently");
			res_status.Add(302, "302 Redirection");
			res_status.Add(304, "304 Not Modified");
	
			res_status.Add(400, "400 Bad Request");
			res_status.Add(401, "401 Unauthorized");
			res_status.Add(403, "403 Forbidden");
			res_status.Add(404, "404 Not Found");

			res_status.Add(500, "500 Internal Server Error");
			res_status.Add(501, "501 Not Implemented");
			res_status.Add(502, "502 Bad Gateway");
			res_status.Add(503, "503 Service Unavailable");
		}
		public void log(string EventMessage) {
			Console.WriteLine(EventMessage);
		}
  } 
}
