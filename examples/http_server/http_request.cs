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
	enum RState
	{
		METHOD, URL, URLPARM, URLVALUE, VERSION, 
		HEADERKEY, HEADERVALUE, BODY, OK
	};

	enum RespState
	{
		OK = 200, 
		BAD_REQUEST = 400,
		NOT_FOUND = 404
	}
	public struct HttpRequestData {
		public string Method;
		public string URL;
		public string Version;
		public Hashtable Args;
		public bool Execute;
		public Hashtable Headers;
		public int BodySize;
		public byte[] BodyData;
  }

	public struct HttpResponseData {
		public int status;
		public string version;
		public Hashtable Headers;
		public int BodySize;
		public byte[] BodyData;
		public System.IO.FileStream fs;
	}

	public class HttpRequest {
    private TcpClient client;
		private RState ParserState;
		private HttpRequestData HTTPRequest;
		private HttpResponseData HTTPResponse;
		byte[] buffer;

		HttpServer server;

		public HttpRequest(TcpClient client, HttpServer server) {
			this.client = client;
			this.server = server;

			this.HTTPResponse.BodySize = 0;
    }
    public void process() {
      this.buffer = new byte[client.ReceiveBufferSize];
      String myCompleteMessage = "";
      int numberOfBytesRead = 0;

      server.log("Connection accepted. Buffer: " + client.ReceiveBufferSize.ToString());
      NetworkStream ns = client.GetStream();

      string hValue = "";
      string hKey = "";

      try  {
        // binary data buffer index
        int bfndx = 0;

        // Incoming message may be larger than the buffer size.
				do {
					numberOfBytesRead = ns.Read(this.buffer, 0, this.buffer.Length);  
					myCompleteMessage = 
					String.Concat(myCompleteMessage, Encoding.ASCII.GetString(this.buffer, 0, numberOfBytesRead));  

					// read buffer index
					int ndx = 0;
					do {
						switch ( ParserState ) {
						case RState.METHOD:
							if (this.buffer[ndx] != ' ') {
								HTTPRequest.Method += (char)this.buffer[ndx++];
							} else {
								ndx++;
								ParserState = RState.URL;
							}
							break;
						case RState.URL:
							if (this.buffer[ndx] == '?') {
								ndx++;
								hKey = "";
								HTTPRequest.Execute = true;
								HTTPRequest.Args = new Hashtable();
								ParserState = RState.URLPARM;
							} else if (this.buffer[ndx] != ' ') {
								HTTPRequest.URL += (char)this.buffer[ndx++];
							} else {
								ndx++;
								HTTPRequest.URL = HttpUtility.UrlDecode(HTTPRequest.URL);
								ParserState = RState.VERSION;
							}
							break;
						case RState.URLPARM:
							if (this.buffer[ndx] == '=') {
								ndx++;
								hValue="";
								ParserState = RState.URLVALUE;
							} else if (this.buffer[ndx] == ' ') {
								ndx++;
								HTTPRequest.URL = HttpUtility.UrlDecode(HTTPRequest.URL);
								ParserState = RState.VERSION;
							}
							else {
								hKey += (char)this.buffer[ndx++];
							}
						break;
						case RState.URLVALUE:
							if (this.buffer[ndx] == '&') {
								ndx++;
								hKey=HttpUtility.UrlDecode(hKey);
								hValue=HttpUtility.UrlDecode(hValue);
								HTTPRequest.Args[hKey] =  HTTPRequest.Args[hKey] != null ? HTTPRequest.Args[hKey] + ", " + hValue : hValue;
								hKey="";
								ParserState = RState.URLPARM;
							} else if (this.buffer[ndx] == ' ') {
								ndx++;
								hKey=HttpUtility.UrlDecode(hKey);
								hValue=HttpUtility.UrlDecode(hValue);
								HTTPRequest.Args[hKey] =  HTTPRequest.Args[hKey] != null ? HTTPRequest.Args[hKey] + ", " + hValue : hValue;

								HTTPRequest.URL = HttpUtility.UrlDecode(HTTPRequest.URL);
								ParserState = RState.VERSION;
							} else {
								hValue += (char)this.buffer[ndx++];
							}
							break;
						case RState.VERSION:
							if (this.buffer[ndx] == '\r') {
								ndx++;
							} else if (this.buffer[ndx] != '\n') {
								HTTPRequest.Version += (char)this.buffer[ndx++];
							} else  {
								ndx++;
								hKey = "";
								HTTPRequest.Headers = new Hashtable();
								ParserState = RState.HEADERKEY;
							}
							break;
						case RState.HEADERKEY:
							if (this.buffer[ndx] == '\r') {
								ndx++;
							} else if (this.buffer[ndx] == '\n') {
								ndx++;
								if (HTTPRequest.Headers["Content-Length"] != null) {
									HTTPRequest.BodySize = Convert.ToInt32(HTTPRequest.Headers["Content-Length"]);
									this.HTTPRequest.BodyData = new byte[this.HTTPRequest.BodySize];
									ParserState = RState.BODY;
								} else {
									ParserState = RState.OK;
								}
							} else if (this.buffer[ndx] == ':') {
								ndx++;
							} else if (this.buffer[ndx] != ' ') {
								hKey += (char)this.buffer[ndx++];
							} else {
								ndx++;
								hValue = "";
								ParserState = RState.HEADERVALUE;
							}
							break;
						case RState.HEADERVALUE:
							if (this.buffer[ndx] == '\r') {
								ndx++;
							} else if (this.buffer[ndx] != '\n') {
								hValue += (char)this.buffer[ndx++];
							} else  {
								ndx++;
								HTTPRequest.Headers.Add(hKey, hValue);
								hKey = "";
								ParserState = RState.HEADERKEY;
							}
							break;
						case RState.BODY:
						// Append to request BodyData
						Array.Copy(this.buffer, ndx, this.HTTPRequest.BodyData, bfndx, numberOfBytesRead - ndx);
						bfndx += numberOfBytesRead - ndx;
						ndx = numberOfBytesRead;
						if ( this.HTTPRequest.BodySize <=  bfndx)
						{
						ParserState = RState.OK;
						}
						break;
						//default:
						//	ndx++;
						//	break;

						}
					} while(ndx < numberOfBytesRead);

				} while(ns.DataAvailable);

        // Print out the received message to the console.
        server.log("You received the following message : \n" +
        myCompleteMessage);

        HTTPResponse.version = "HTTP/1.1";

        if (ParserState != RState.OK) {
					HTTPResponse.status = (int)RespState.BAD_REQUEST;
        } else {
					HTTPResponse.status = (int)RespState.OK;
				}

        this.HTTPResponse.Headers = new Hashtable();
        this.HTTPResponse.Headers.Add("Server", server.Name);
        this.HTTPResponse.Headers.Add("Date", DateTime.Now.ToString("r"));

        // if (HTTPResponse.status == (int)RespState.OK)
        this.server.OnResponse(ref this.HTTPRequest, ref this.HTTPResponse);

        string HeadersString = this.HTTPResponse.version + " " + this.server.res_status[this.HTTPResponse.status] + "\n";

        foreach (DictionaryEntry Header in this.HTTPResponse.Headers)  {
					HeadersString += Header.Key + ": " + Header.Value + "\n";
        }

        HeadersString += "\n";
        byte[] bHeadersString = Encoding.ASCII.GetBytes(HeadersString);

        // Send headers	
        ns.Write(bHeadersString, 0, bHeadersString.Length);

        // Send body
        if (this.HTTPResponse.BodyData != null) {
					ns.Write(this.HTTPResponse.BodyData, 0, this.HTTPResponse.BodyData.Length);
				}

        if (this.HTTPResponse.fs != null) {
					using (this.HTTPResponse.fs) 
					{
						byte[] b = new byte[client.SendBufferSize];
						int bytesRead;
						while ((bytesRead = this.HTTPResponse.fs.Read(b,0,b.Length)) > 0) {
							ns.Write(b, 0, bytesRead);
						}

						this.HTTPResponse.fs.Close();
					}
				}

      } catch (Exception e)  {
        server.log(e.ToString());
      } finally  {
        ns.Close();
        client.Close();
        if (this.HTTPResponse.fs != null)
        this.HTTPResponse.fs.Close();
        Thread.CurrentThread.Abort();
      }
    }
	}
}
