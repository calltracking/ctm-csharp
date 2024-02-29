# CTM Phone Embed Demo

Example C# Application to connect CallTrackingMetrics embeddable softphone into your application.

  * You can either use our component's UI or build your own
  * Authentication is handled automatically via backend connection

To run the example web application you will need to ensure the following environment variables are set

```
CTM_TOKEN: Your API key
CTM_SECRET: Your API secret key
CTM_ACCOUNT_ID: Your CTM Account ID
CTM_HOST: app.calltrackingmetrics.com
```

In bash you can do this with:
```
export CTM_SECRET=your_actual_auth_token_here
```
windows command prompt:
```
set CTM_SECRET=your_actual_auth_token_here
```
and windows powershell:
```
$env:CTM_SECRET="your_actual_auth_token_here"
```


# Building

```
dotnet build
```


# Running

```
dotnet run
```
