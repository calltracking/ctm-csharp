using System;
using System.IO;
using System.Net;
using System.Text;
using CTM;

namespace CTM {
	class Test {
		static void Main(string[] args) {
			AuthToken token = AuthToken.authorize(Environment.GetEnvironmentVariable("CTM_API_KEY"),
                                            Environment.GetEnvironmentVariable("CTM_API_SECRET"));
      Console.WriteLine("got token: " + token.auth_token);

      // search for some numbers in 410 area code
      CTM.Number[] numbers = CTM.Number.search(token, "410");
      foreach(Number number in numbers) {
        Console.WriteLine("Found Number: " + number.number);
      }
		}
	}
}
