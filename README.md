

# RolyApps Auth
A dead simple template for a serverless AWS-based identity service, compromising of:

- A Cognito UserPool and Client.
- .Net Core lambda function serving as an API.
- API Gateway to route requests to the function.

If you're looking for a really simple authentication service to begin messing around with your own applications feel free to grab this project template and re-use as you see fit.


**Prereqs for quick start:**
- AWS CLI installed and configured  (check https://aws.amazon.com/cli/ for install guide)
- AWS CDK (check https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html for install guide)
- .Net Lambda tools (check https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-cli.html for install guide)

**Quick start:**

- Clone the repo.
- Run `BuildAndDeploy.cmd` located in the root of the solution.
- You may need to follow y/n prompts when first deploying, or changing any infrastructure in the CDK stack

Once the stack is deployed into your AWS environment, you should be able to log in to console and see the url for the API Gateway which sits in front of the application so you can start testing requests, something like this:  
![enter image description here](https://i.imgur.com/lQn0Ul9.png)

**Provided Functions:**  
*Create a user account*  
POST to `account/register`   
`{ "email":  "jane.citizen@email.io", "password":  "someP@ssw0rdHere!" }`
> Would be great to incorporate user verification via an email link or similar, if you've got the spare time you're welcome to fork this repo and add such functionality ;)

*Login with Username and Password*  
POST to `account/login`

`{ "email":  "jane.citizen@email.io", "password":  "someP@ssw0rdHere!" }*`
The IdToken property returned in response to this is the bearer token to be used with subsequent requests to authorized endpoints.*

*Refresh Session with LoginToken*  
POST to `/account/login/token`  
`{ "refreshToken":  "eyJd...", }`

Initiate OTP Login (request email code)
POST to `account/login/requestOTP`
`{"email": "jane.citizen@email.io`}
Response to this call contains a session token which must be included in the OTP submission which follows below

Submit OTP for Login (email code)
POST to `account/login/submitEmailOtp`
`{"email": "jane.citizen@email.io, "code": "12345678", "sessionToken": "Ayabezek..."}`
This returns a response with IdToken (similar to username + pw login above which can be used for subsequent Bearer-auth-based calls)

*Begin Password Reset*  
POST to `account/forgotPassword`

`{ "email":  "jane.citizen@email.io" }`
> This endpoint will cause a very simple email to be sent to the provided address with a reset code to enter with their new password, detailed below.

*Complete Password Reset*  
POST to `/account/resetPassword`

{ "email":  "jane.citizen@email.io", "confirmationCode":  "391111", "newPassword":  "someN3wP@ssw0rdHere!" }  
In my use case, I'm tinkering with this as a central auth portal to other apps I'm going to play around with in my spare time, so I've included an authenticated endpoint to return a list of applications with a GET to `/apps`

This endpoint is protected by Cognito and can only be accessed by applying a valid Bearer token received after logging in. You can check out how a protected endpoint is configured in CDK by taking a look at the `cognitoAuthorizer` construct `within RolyAuthStack.cs`

That's about it at the moment. I hope to have a bit more time to tinker with this template in the future to add a little but more funk to it, hopefully it'll save you a buttload of time if you have a similar use case :)

**Need to nuke it?**  
Run `cdk destroy` at the solution root folder, follow the in-cmd prompts and your stack will be taken down and destroyed.  
**Note**: The CognitoPool in this CDK stack will not be retained if you pull the pin, this means all your user data will be deleted when you destroy the stack.


## Bonus: Verifying a JWT token from your Cognito Auth application in a .Net Core API

This was surprisingly simple! We just need to configure Token validation in the application startup and away you go.

So, in your `Startup.cs` file:
  ```
 public void ConfigureServices(IServiceCollection services) {            ...  
 // I'm not sure if order matters but I did this after .AddControllers  
 services.AddAuthentication() .AddJwtBearer(options => { options.TokenValidationParameters = GetCognitoTokenValidationParams(configRoot); });  
 }     private TokenValidationParameters GetCognitoTokenValidationParams(IConfiguration configuration)  
 { var cognitoIssuer = configuration["Cognito:IssuerUrl"]; var jwtKeySetUrl = $"{cognitoIssuer}/.well-known/jwks.json"; var cognitoAudience = configuration["Cognito:ClientId"];  
 return new TokenValidationParameters { IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) => { // get JsonWebKeySet from AWS                    var json = new WebClient().DownloadString(jwtKeySetUrl);  
  
 // serialize the result                    var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;  
  
 // cast the result to be the type expected by IssuerSigningKeyResolver                    return (IEnumerable<SecurityKey>)keys;  
 }, ValidIssuer = cognitoIssuer, ValidateIssuerSigningKey = true, ValidateIssuer = true, ValidateLifetime = true, ValidAudience = cognitoAudience }; }  
  ```
and then your `appsettings.json` file should look something like:

` { ... "Cognito": { "ClientId": "{your-client-id-here}", "IssuerUrl": "https://cognito-idp.{your-aws-region-here}.amazonaws.com/{your-aws-region-here}_{your-cognito-pool-id-here}" } } `
Once these are set up, you can just use the `[Authorize]` attribute on your controllers / endpoints to protect them by only allowing requests with valid Bearer (IdToken) tokens through.