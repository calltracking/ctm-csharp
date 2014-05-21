using System;
using System.IO;
using System.Net;
using System.Text;
using CTM;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace CTM {
  class Test {
    static void Main(string[] args) {

      // +----------------+
      // | Authentication |
      // +----------------+

      AuthToken token = AuthToken.authorize(Environment.GetEnvironmentVariable("CTM_TOKEN"),
                                            Environment.GetEnvironmentVariable("CTM_SECRET"));

      Console.WriteLine("Got token: " + token.auth_token);

      // +-----------------------+
      // | Working with Accounts |
      // +-----------------------+

      Console.WriteLine();
      string first_id = ListAccounts(token, 2);

      Account account = Account.get(token, first_id);

      Console.WriteLine();
      PrintAccount(account);

      UpdateAccount(account);

      token.switch_account(account.id);

      // +----------------------+
      // | Working with Numbers |
      // +----------------------+

      Console.WriteLine();
      first_id = ListNumbers(token, 2);

      Number number = Number.get(token, first_id);

      Console.WriteLine();
      PrintNumber(number);

      UpdateNumber(number);

      // +--------------------------+
      // | Setting up a new account | <- not currently working?
      // +--------------------------+

      // Console.WriteLine();
      // Account new_account = CreateAccount(token);

      // if (new_account.errors != null){ return; }

      // token.switch_account(new_account.id)

      // +----------------------------------------+
      // | Purchasing and setting up a new number |
      // +----------------------------------------+

      Console.WriteLine();
      PurchaseAndConfigureNewNumber(token);

      // SingleNumber(token, number.id);

      //ListNumbers(token);

      /*
        query all calls by the given source between the given date range
      */
      /*Filter[] filters = new Filter[3];
        filters[0] = new Filter("start_date", "2012-08-01");
        filters[1] = new Filter("end_date", "2012-08-31");
        filters[2] = new Filter("source", "Google Paid");
        Report[] reports = new Report[1];
        // note: http://localhost:8888/ is in examples/report_server.cs
        reports[0] = new Report("my test report", "sum", "source", "call", "http://localhost:8888/", filters);

        Report.Query(token, reports);
      */

    }

    // +------------------+
    // | Printing Helpers |
    // +------------------+

    static void PrintAccount(Account account){
      if (account.error == null){
        Console.WriteLine("Account: " + account.id);
        Console.WriteLine("    Name: " + account.name);
        Console.WriteLine("  Status: " + account.status);
        Console.WriteLine(" Website: " + account.website);
        Console.WriteLine("Timezone: " + account.timezone);
        Console.WriteLine(" Billing: " + (account.shared_billing
                                         ? "Shared"
                                         : "Separate"));
        Console.WriteLine(" Balance: $" +
                          (int)(account.balance / 100) + "." +
                          (int)(account.balance % 100));
      } else{
        Console.WriteLine("Error: " + account.error);
      }
    }

    static void PrintNumber(Number number){
      if (number.error == null){
        Console.WriteLine("Number: " + number.id);
        Console.WriteLine("     Name: " + number.name);
        Console.WriteLine("   Active: " + (number.active ? "Yes" : "No"));
        Console.WriteLine("   Number: " + number.number);
        Console.WriteLine("Formatted: " + number.formatted);
        Console.WriteLine("  Routing: " + number.routing);
        Console.WriteLine("Next bill on " +
                          number.next_billing_date.ToString("MMM d, yyyy"));
      } else{
        Console.WriteLine("Error: " + number.error);
      }
    }

    // +-----------------+
    // | Listing Helpers |
    // +-----------------+

    static string ListAccounts(AuthToken token, int? max_page = null) {
      int page = 1;
      string first_account_id = null;

      Page<Account> accounts;
      do {

        accounts = Account.list(token, page);
        Console.WriteLine("Accounts Page: " + page.ToString());

        foreach(Account act in accounts.items) {
          if (first_account_id == null){ first_account_id = act.id; }
          Console.WriteLine("  Account " + act.id + ": " + act.name);
        }

        ++page;

      } while (accounts.page < accounts.total_pages && (max_page ==  null || accounts.page < max_page));
      return first_account_id;
    }

    static string ListNumbers(AuthToken token, int? max_page = null) {
      int page = 1;
      string first_number_id = null;

      Page<Number> numbers;
      do {

        numbers = Number.list(token, page);
        Console.WriteLine("Numbers Page: " + page.ToString());

        foreach(Number num in numbers.items) {
          if (first_number_id == null){ first_number_id = num.id; }
          Console.WriteLine("  " + num.id + " " + num.number);
        }

        ++page;

      } while (numbers.page < numbers.total_pages && (max_page == null || numbers.page < max_page));
      return first_number_id;
    }

    // +----------------------------+
    // | Editing / Updating Helpers |
    // +----------------------------+

    static void UpdateAccount(Account account){
      string oldname = account.name;
      string oldsite = account.website;
      string oldtz   = account.timezone;

      account.name     = "Changed the name!";
      account.website  = "http://www.example.com/ctm-api";
      account.timezone = "UTC";

      account.save();

      Console.WriteLine();
      PrintAccount(account);

      account.name     = oldname;
      account.website  = oldsite;
      account.timezone = oldtz;

      account.save();

      Console.WriteLine();
      PrintAccount(account);
    }

    static void UpdateNumber(Number number){
      string oldname   = number.name;
      bool   oldactive = number.active;
      string oldformat = number.formatted;

      number.name      = "Changed the name!";
      number.active    = !number.active;
      number.formatted = "(: " + number.formatted + " :)";

      number.save();

      Console.WriteLine();
      PrintNumber(number);

      number.name      = oldname;
      number.active    = oldactive;
      number.formatted = oldformat;

      number.save();

      Console.WriteLine();
      PrintNumber(number);
    }

    // +--------------------------------+
    // | Creation / Destruction Helpers |
    // +--------------------------------+

    static Account CreateAccount(CTM.AuthToken token){
      Account settings = new Account(token);
      settings.name           = "API Test Account";
      settings.website        = "API Test Website";
      settings.timezone       = "Eastern Standard Time";
      settings.shared_billing = false;

      Account new_account = settings.create();
      PrintAccount(new_account);

      return new_account;
    }

    static Number PurchaseAndConfigureNewNumber(AuthToken token) {
      // search for some numbers in 410 area code
      Number.SearchResult[] numbers = Number.search(token, "410");

      if (numbers.Length == 0) { return null; }
      if (numbers.Length == 1 && numbers[0].error != null){
        Console.WriteLine("Error: " + numbers[0].error);
        return null;
      }

      foreach(Number.SearchResult num in numbers) {
        Console.WriteLine("Found Number: " + num.friendly_name);
      }

      return null;

      // Number number = Number.buy(token, numbers[0].number);
      // //Number number = new Number(5416, "+1xxxxxxxxx", token);
      // Console.WriteLine("Purchased Number: " + number.number + ", id: " + number.id);
      // //Number number = new Number("TPNC3C4B23C348AEC2EE54EFD301979CD2E3DB8E9F64D03380D8CCFD902D3BEF3AC", "+15005550006", token, "test number1");

      // if (!number.addReceivingNumber("+18888980510")) {
      //   Console.WriteLine("failed with:" + number.error);
      //   return number;
      // }

      // Source source = number.addTrackingSource("Test Source2", "google.com", "", 80);
      // Console.WriteLine("Created source: " + source.name);

      // return number;
    }
  }
}
