# Call Tracking Metrics C# Library


## Authentication

Enable API Access within your account

			AuthToken token = AuthToken.authorize(Environment.GetEnvironmentVariable("CTM_API_KEY"),
                                            Environment.GetEnvironmentVariable("CTM_API_SECRET"));

      // search for some numbers in 410 area code
      CTM.Number[] numbers = CTM.Number.search(token, "410");
      foreach(Number number in numbers) {
        Console.WriteLine("Found Number: " + number.number);
      }

      // buy a number
      Number number = Number.buy(token, numbers[0].number);

      // Add a receiving number to the purchased number
      number.addReceivingNumber("+1xxxxxxxxxx");
