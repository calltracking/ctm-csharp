using System;
using System.IO;
using System.Net;
using System.Text;
using CTM;
using NUnit.Framework;

namespace CTM {
	class Test {
		static void Main(string[] args) {
			AuthToken token = AuthToken.authorize(Environment.GetEnvironmentVariable("CTM_API_KEY"),
                                            Environment.GetEnvironmentVariable("CTM_API_SECRET"));
      Console.WriteLine("got token: " + token.auth_token);

      PurchaseAndConfigureNewNumber(token);

      ListNumbers(token);

      //SingleNumber(token, 10373);

      ListAccounts(token);

      //Account account = CreateAccount(token);
      //token.switch_account(account.id);

//      Number number = PurchaseAndConfigureNewNumber(token);
//      SingleNumber(token, number.id);

      //ListNumbers(token);

			/*
				query all calls by the given source between the given date range
			*/
			Filter[] filters = new Filter[3];
			filters[0] = new Filter("start_date", "2012-08-01");
			filters[1] = new Filter("end_date", "2012-08-31");
			filters[2] = new Filter("source", "Google Paid");
			Report[] reports = new Report[1];
			// note: http://localhost:8888/ is in examples/report_server.cs
			reports[0] = new Report("my test report", "sum", "source", "call", "http://localhost:8888/", filters);
			
			Report.Query(token, reports);

		}

    static Account CreateAccount(AuthToken token) {
      return Account.create_linked(token, "new account");
    }

    static void ListAccounts(AuthToken token) {
      int page = 1;
      Page<Account> accounts;
      do {
        
        accounts = Account.list(token, page);
        Console.WriteLine("Accounts Page: " + page.ToString());

        foreach(Account act in accounts.items) {
          Console.WriteLine("\tAccount: " + act.id.ToString() + ", " + act.name);
        }

        ++page;

      } while (accounts.page < accounts.total_pages);
    }

    static void ListNumbers(AuthToken token) {
      int page = 1;
      Page<Number> numbers;
      do {
        
        numbers = Number.list(token, page);
        Console.WriteLine("Numbers Page: " + page.ToString());

        foreach(Number num in numbers.items) {
          Console.WriteLine("\tNumber: " + num.id.ToString() + ", " + num.number);
        }

        ++page;

      } while (numbers.page < numbers.total_pages);
    }

    static Number PurchaseAndConfigureNewNumber(AuthToken token) {
      // search for some numbers in 410 area code
      Number[] numbers = Number.search(token, "410");

      foreach(Number num in numbers) {
        Console.WriteLine("Found Number: " + num.number);
      }

      Number number = Number.buy(token, numbers[0].number);
      //Number number = new Number(5416, "+1xxxxxxxxx", token);
      Console.WriteLine("Purchased Number: " + number.number + ", id: " + number.id);

      number.addReceivingNumber("+18888980513");

      Source source = number.addTrackingSource("Test Source1", "google.com", "", 100);

      return number;
    }

    static void SingleNumber(AuthToken token, int id) {
      Number number = Number.get(token, id);

      number.name = "Test";

      number.save();
      number = Number.get(token, id);
      Assert.AreEqual("Test", number.name);
    }
	}
}
