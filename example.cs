using CTM;
using System;
using System.IO;
using System.Net;
using System.Text;
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
      if (token.error != null){
        Console.WriteLine("Error: " + token.error);
        return;
      }

      Console.WriteLine("Got token: " + token.auth_token);

      // +-----------------------+
      // | Working with Accounts |
      // +-----------------------+

      string id = ListAccounts(token, 2);
      if (id == null){ return; }

      Account account = Account.get(token, id);

      UpdateAccount(account);

      token.switch_account(id);

      // +----------------------+
      // | Working with Numbers |
      // +----------------------+

      id = ListNumbers(token, 2);

      Number number = Number.get(token, id);

      UpdateNumber(number);

      // +--------------------------------+
      // | Working with Receiving Numbers |
      // +--------------------------------+

      id = ListReceivingNumbers(token, 2);
      if (id != null){
        ReceivingNumber receiving_number = ReceivingNumber.get(token, id);

        UpdateReceivingNumber(receiving_number);

        // +-------------------------------+
        // | Working with Tracking Sources |
        // +-------------------------------+

        id = ListSources(token, 2);
        Source source = Source.get(token, id);

        UpdateSource(source);
      }

      // +--------------------------+
      // | Setting up a new account | <- not currently working?
      // +--------------------------+

      Console.WriteLine();
      Account new_account = CreateAccount(token);

      if (new_account.error != null){ return; }

      token.switch_account(new_account.id);

      // +----------------------------------------+
      // | Purchasing and setting up a new number |
      // +----------------------------------------+

      PurchaseAndConfigureNewNumber(token);
    }

    // +------------------+
    // | Printing Helpers |
    // +------------------+

    static void PrintAccount(Account account){
      Console.WriteLine();
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
        Console.WriteLine("Error in account: " + account.error);
      }
    }

    static void PrintNumber(Number number){
      Console.WriteLine();
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
        Console.WriteLine("Error in number: " + number.error);
      }
    }

    static void PrintReceivingNumber(ReceivingNumber rn){
      Console.WriteLine();
      if (rn.error == null){
        Console.WriteLine("ReceivingNumber: " + rn.id);
        Console.WriteLine("     Name: " + rn.name);
        Console.WriteLine("   Number: " + rn.number);
        Console.WriteLine("  Display: " + rn.display_number);
        Console.WriteLine("Formatted: " + rn.formatted);
      } else {
        Console.WriteLine("Error in receiving number: " + rn.error);
      }
    }

    static void PrintTrackingSource(Source s){
      Console.WriteLine();
      if (s.error == null){
        Console.WriteLine("Tracking Source: " + s.id);
        Console.WriteLine("       Name: " + s.name);
        Console.WriteLine("     Online: " + (s.online ? "Yes" : "No"));
        Console.WriteLine("Ref pattern: " + s.referring_url);
        Console.WriteLine("Ref exclude: " + s.not_referrer_url);
        Console.WriteLine("URL pattern: " + s.landing_url);
        Console.WriteLine("URL exclude: " + s.not_landing_url);
        Console.WriteLine("   Position: " + s.position);
      }
    }

    // +-----------------+
    // | Listing Helpers |
    // +-----------------+

    static string ListAccounts(AuthToken token, int? max_page = null) {
      int           page = 1;
      string        second_account_id = null;
      bool          first_account = false;
      Page<Account> accounts;

      Console.WriteLine();

      do {
        accounts = Account.list(token, page);

        if (accounts.error != null){
          Console.WriteLine("Error listing accounts: " + accounts.error);
          return null;
        }

        Console.WriteLine("Accounts Page: " + page);

        foreach(Account act in accounts.items) {
          if (first_account == false){
            first_account = true;
          }else if(second_account_id == null){
            second_account_id = act.id;
          }

          Console.WriteLine("  Account " + act.id + ": " + act.name);
        }

        ++page;
      } while (accounts.page < accounts.total_pages && (max_page ==  null || accounts.page < max_page));
      return second_account_id;
    }

    static string ListSources(AuthToken token, int? max_page = null){
      int          page = 1;
      string       id = null;
      Page<Source> sources;

      Console.WriteLine();

      do {
        sources = Source.list(token, page);

        if (sources.error != null){
          Console.WriteLine("Error listing sources: " + sources.error);
          return null;
        }

        Console.WriteLine("Sources Page: " + page);

        foreach(Source src in sources.items) {
          if (id == null){ id = src.id; }
          Console.WriteLine("  Source " + src.id + ": " + src.name);
        }

        ++page;
      } while (sources.page < sources.total_pages && (max_page ==  null || sources.page < max_page));
      return id;
    }

    static string ListNumbers(AuthToken token, int? max_page = null) {
      int          page = 1;
      string       first_number_id = null;
      Page<Number> numbers;

      Console.WriteLine();

      do {
        numbers = Number.list(token, page);

        if (numbers.error != null){
          Console.WriteLine("Error listing numbers: " + numbers.error);
          return null;
        }

        Console.WriteLine("Numbers Page: " + page.ToString());

        foreach(Number num in numbers.items) {
          if (first_number_id == null){ first_number_id = num.id; }
          Console.WriteLine("  " + num.id + " " + num.number);
        }

        ++page;

      } while (numbers.page < numbers.total_pages && (max_page == null || numbers.page < max_page));
      return first_number_id;
    }

    static string ListReceivingNumbers(AuthToken token, int? max_page = null) {
      int          page = 1;
      string       first_number_id = null;
      Page<ReceivingNumber> numbers;

      Console.WriteLine();

      do {
        numbers = ReceivingNumber.list(token, page);

        if (numbers.error != null){
          Console.WriteLine("Error listing receiving numbers: " + numbers.error);
          return null;
        }

        Console.WriteLine("ReceivingNumbers Page: " + page.ToString());

        foreach(ReceivingNumber num in numbers.items) {
          if (first_number_id == null){ first_number_id = num.id; }
          Console.WriteLine("  " + num.id + " " + num.number);
        }

        ++page;

      } while (numbers.page < numbers.total_pages && (max_page == null || numbers.page < max_page));
      return first_number_id;
    }

    static void ListReceivingNumbersOnNumber(Number number, int? max_page = null){
      int          page = 1;
      Page<ReceivingNumber> numbers;

      Console.WriteLine();

      do {
        numbers = number.receiving_numbers(page);

        if (numbers.error != null){
          Console.WriteLine("Error listing receiving numbers: " + numbers.error);
          return;
        }

        Console.WriteLine("ReceivingNumbers Page: " + page.ToString());

        foreach(ReceivingNumber num in numbers.items) {
          Console.WriteLine("  " + num.id + " " + num.number);
        }

        ++page;
      } while (numbers.page < numbers.total_pages && (max_page == null || numbers.page < max_page));
    }
    // +----------------------------+
    // | Editing / Updating Helpers |
    // +----------------------------+

    static void UpdateAccount(Account account){
      if (account.error != null){
        Console.WriteLine("Account has error: " + account.error);
        return;
      }

      PrintAccount(account);

      string oldname = account.name;
      string oldsite = account.website;
      string oldtz   = account.timezone;

      account.name     = "Changed the name!";
      account.website  = "http://www.example.com/ctm-api";
      account.timezone = "UTC";

      if (!account.save()){
        Console.WriteLine("Error saving acount: " + account.error);
        account.reload();
        return;
      } else{
        account.reload();
      }

      PrintAccount(account);

      account.name     = oldname;
      account.website  = oldsite;
      account.timezone = oldtz;

      if (!account.save()){
        Console.WriteLine("Error saving acount: " + account.error);
        account.reload();
        return;
      } else{
        account.reload();
      }

      PrintAccount(account);
    }

    static void UpdateSource(Source src){
      if (src.error != null){
        Console.WriteLine("Source has error: " + src.error);
        return;
      }

      PrintTrackingSource(src);

      string oldname     = src.name;
      int?   oldposition = src.position;
      bool   oldonline   = src.online;

      string oldref_pattern = src.referring_url;
      string oldref_exclude = src.not_referrer_url;
      string oldurl_pattern = src.landing_url;
      string oldurl_exclude = src.not_landing_url;

      src.name     = "Changed the name!";
      src.position = 23;
      src.online   = false;

      src.referring_url    = "Referring URL Regex";
      src.not_referrer_url = "Referring URL Exclude";
      src.landing_url      = "Landing URL Regex";
      src.not_landing_url  = "Landing URL Exclude";

      if (!src.save()){
        Console.WriteLine("Error saving source: " + src.error);
        src.reload();
        return;
      }else{
        src.reload();
      }

      PrintTrackingSource(src);

      src.name     = oldname;
      src.position = oldposition;
      src.online   = oldonline;

      src.referring_url    = oldref_pattern;
      src.not_referrer_url = oldref_exclude;
      src.landing_url      = oldurl_pattern;
      src.not_landing_url  = oldurl_exclude;

      if (!src.save()){
        Console.WriteLine("Error saving source: " + src.error);
        src.reload();
        return;
      }else{
        src.reload();
      }

      PrintTrackingSource(src);
    }

    static void UpdateNumber(Number number){
      if (number.error != null){
        Console.WriteLine("Number has error: " + number.error);
        return;
      }

      PrintNumber(number);

      string oldname   = number.name;
      bool   oldactive = number.active;
      string oldformat = number.formatted;

      number.name      = "Changed the name!";
      number.active    = !number.active;
      number.formatted = "(: " + number.formatted + " :)";

      if (!number.save()){
        Console.WriteLine("Error saving number: "  + number.error);
        number.reload();
        return;
      }else{
        number.reload();
      }

      PrintNumber(number);

      number.name      = oldname;
      number.active    = oldactive;
      number.formatted = oldformat;

      if (!number.save()){
        Console.WriteLine("Error saving number: "  + number.error);
        number.reload();
        return;
      }else{
        number.reload();
      }

      PrintNumber(number);
    }

    static void UpdateReceivingNumber(ReceivingNumber rn){
      if (rn.error != null){
        Console.WriteLine("ReceivingNumber has error: " + rn.error);
        return;
      }

      PrintReceivingNumber(rn);

      string oldname   = rn.name;
      string oldnumber = rn.number;

      rn.name = "Changed the name!";
      rn.number = "+266696687";      //anonymous

      if (!rn.save()){
        Console.WriteLine("Error saving receiving number: " + rn.error);
        rn.reload();
        return;
      }else{
        rn.reload();
      }

      PrintReceivingNumber(rn);

      rn.name   = oldname;
      rn.number = oldnumber;

      if (!rn.save()){
        Console.WriteLine("Error saving receiving number: " + rn.error);
        rn.reload();
        return;
      }else{
        rn.reload();
      }

       PrintReceivingNumber(rn);
    }

    // +--------------------------------+
    // | Creation / Destruction Helpers |
    // +--------------------------------+

    static Account CreateAccount(CTM.AuthToken token){
      Account settings = new Account(token);
      settings.name           = "API Test Account";
      settings.website        = "API Test Website";
      settings.timezone       = "Eastern Standard Time";
      settings.shared_billing = true;

      Account new_account = settings.create();
      PrintAccount(new_account);

      return new_account;
    }

    static void PurchaseAndConfigureNewNumber(AuthToken token) {
      Number.SearchResult[] numbers = null;

      // search for some toll-free numbers
      numbers = Number.search_tollfree(token);

      Console.WriteLine();
      if (numbers.Length == 0){
        Console.WriteLine("No US toll free numbers found");

      } else if (numbers[0].error != null){
        Console.WriteLine("Error searching for US toll free numbers: "  + numbers[0].error);
        return;
      }

      foreach(Number.SearchResult num in numbers) {
        Console.WriteLine("US Toll free: " + num.friendly_name);
      }

      // search for some numbers in 410 area code
      numbers = Number.search(token, "410");

      Console.WriteLine();
      if (numbers.Length == 0) {
        Console.WriteLine("No numbers found for US 410 area code");
        return;

      } else if (numbers[0].error != null) {
        Console.WriteLine("Error searching for numbers local to US 410 area code: " + numbers[0].error);
        return;
      }

      foreach(Number.SearchResult num in numbers) {
        Console.WriteLine("Local to 410: " + num.friendly_name + " " + num.rate_center);
      }

      // Buy the number
      Number number = Number.buy(token, numbers[0].phone_number);
      if (number.error != null){
        Console.WriteLine("Error buying number [" + numbers[0].phone_number + "]: " + number.error);
        return;
      }

      Console.WriteLine();
      Console.WriteLine("Purchased Number: " + number.number + ", id: " + number.id);

      // Add a receiving number
      ReceivingNumber rn = number.addReceivingNumber("+18888980510");
      if (rn.error != null){
        Console.WriteLine("Error adding receiving number: " + rn.error);
        return;
      }

      Console.WriteLine("Added receiving number " + rn.id + " to number");
      PrintReceivingNumber(rn);
      ListReceivingNumbersOnNumber(number);

      // Add a tracking source
      Source source = number.addTrackingSource("Test Source2",
                                               "google.com", "",
                                               "gclid=.+",   "",
                                               100,          true);

      Console.WriteLine("Created source: " + source.name);
      PrintTrackingSource(source);

      // Remove the receiving number
      if (!number.remReceivingNumber(rn)){
        Console.WriteLine("Error removing receiving_number: " + number.error);
        number.error = null;
      }else{
        Console.WriteLine();
        Console.WriteLine("Removed receiving number " + rn.id + " from number");
        ListReceivingNumbersOnNumber(number);
      }

      // Release the receiving / tracking numbers
      Console.WriteLine();
      if (!number.release()){
        Console.WriteLine("Error releasing number: " + number.error);
      }else{
        Console.WriteLine("Released number " + number.formatted);
      }

      if (!rn.release()){
        Console.WriteLine("Error releasing receiving number: " + rn.error);
        return;
      }
      Console.WriteLine("Released receiving number " + rn.number);
    }
  }
}
