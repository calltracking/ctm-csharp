# Webhook Receiver Example with SHA1 Signature Verification

## Description

This project demonstrates a simple webhook receiver implemented in ASP.NET Core.
It showcases how to securely receive data by verifying an SHA1 HMAC signature,
ensuring that the incoming requests are authenticated and have not been tampered with during transit.
The project uses an `X-CTM-Signature` header to carry the signature and an `X-CTM-Time` header to include a timestamp,
which are used together with a secret key to compute and verify the request's integrity.

## Features

- ASP.NET Core minimal API for receiving webhook POST requests.
- SHA1 HMAC signature verification to ensure data integrity and source authenticity.
- Reading and using custom HTTP headers (`X-CTM-Time` and `X-CTM-Signature`).

## Prerequisites

- .NET 5.0 SDK or later.

## Building the Project


```
  dotnet build
```


## Running the Project

```
  dotnet run
```

## Testing 
  
  You can test the webhook by installing ngrok.  From there within CTM you can create a Webhook and use the test button to verify your webhook is working.

## Understanding SHA1 Signature Verification
The project verifies the authenticity and integrity of incoming webhook requests using an SHA1 HMAC signature. The signature is computed on the sending side by concatenating the X-CTM-Time header value and the request body, then using the sender's secret key (the auth_token) to generate an HMAC SHA1 hash. This hash is then base64-encoded and sent as the X-CTM-Signature header.

On the receiving side (this project), the process is reversed to compute the expected signature using the same method and secret key. If the computed signature matches the one received in the X-CTM-Signature header, the request is considered authentic and has not been altered.
