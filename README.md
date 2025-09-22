

## Introduction
This repo presents a dead simple template for a centralised, serverless AWS-based identity service, compromising of:

- A Cognito UserPool and Client.
- .Net Core lambda function serving as an API.
- API Gateway to route requests to the function.
- DynamoDb table to store applications a user has access to

If you're looking for a really simple authentication service to begin messing around with your own applications feel free to grab this project template and re-use as you see fit.


### Prereqs for quick start:
- AWS CLI installed and configured  (check https://aws.amazon.com/cli/ for install guide)
- AWS CDK (check https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html for install guide)
- .Net Lambda tools (check https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-cli.html for install guide)

### Quick start:
- Clone the repo.
- Run `BuildAndDeploy.cmd` located in the root of the solution.
- You may need to follow y/n prompts when first deploying, or changing any infrastructure in the CDK stack

Once the stack is deployed into your AWS environment, you should be able to log in to console and see the url for the API Gateway which sits in front of the application so you can start testing requests, something like this:    
![enter image description here](https://i.imgur.com/lQn0Ul9.png)

## Provided Endpoints:

### Username & Password flows
The flows below dictate how to use username and password registration and login.

#### Create a user account
POST to `account/register`
`{ "email":  "jane.citizen@email.io", "password":  "someP@ssw0rdHere!", "name": "Jane" }`
> Would be great to incorporate user verification via an email link or similar, if you've got the spare time you're welcome to fork this repo and add such functionality ;)

#### Login with Username and Password
POST to `account/login`  
`{ "email":  "jane.citizen@email.io", "password":  "someP@ssw0rdHere!" }`
> The IdToken property returned in response to this is the bearer token to be used with subsequent requests to authorized endpoints.*

#### Request Password Reset
POST to `account/forgotPassword`  
`{ "email":  "jane.citizen@email.io" }`
> This endpoint will cause a very simple email to be sent to the provided address with a reset code to enter with their new password, detailed below.

#### Complete Password Reset*
POST to `/account/resetPassword`    
`{ "email":  "jane.citizen@email.io", "confirmationCode":  "123456", "newPassword":  "someN3wP@ssw0rdHere!" } `

### OTP-based flows
A modern login experience featuring OTP-based registration and login

#### Register Account using OTP
POST to `account/otpRegistration`
`{ "email":  "jane.citizen@email.io", "name":  "Jane" }`
> This creates a user account with an "unverified" email property. This endpoint will send the user an email and returns a session token to be used with the Submit OTP endpoint below which then confirms their email address and returns a token they can use for subsequent authenticated actions.

#### Request OTP Code for Login
POST to `account/login/requestOTP`  
`{"target": "jane.citizen@email.io", "otpMedium": "Email" }`
> Response to this call contains a session token which must be included in the OTP submission which follows below.

**Note:** Currently only a OtpMedium of Email is supported. Looking to add SMS Support at some point in the future.

#### Submit OTP for Login (email code)
POST to `account/login/submitEmailOtp`  
`{"email": "jane.citizen@email.io, "code": "12345678", "sessionToken": "Ayabezek..."}`
> This returns a response with IdToken (similar to username + pw login above which can be used for subsequent Bearer-auth-based calls)

### Other endpoints
#### Refresh Session with LoginToken
POST to `/account/login/token`
`{ "refreshToken":  "eyJd...", }`
> Returns a response identical to login endpoints with updated IdToken for ongoing access

#### Get Apps
GET to `/apps`
In my use case, I'm tinkering with this as a central auth portal to other apps I'm going to play around with in my spare time, so I've included an authenticated endpoint to return a list of applications with a GET to `/apps`

This endpoint is protected by Cognito and can only be accessed by applying a valid Bearer token received after logging in. You can check out how a protected endpoint is configured in CDK by taking a look at the `cognitoAuthorizer` construct `within RolyAuthStack.cs`

### Need to nuke your solution in AWS?
Run `cdk destroy` at the solution root folder, follow the in-cmd prompts and your stack will be taken down and destroyed.    
**Note**: The CognitoPool in this CDK stack will not be retained if you pull the pin, this means all your user data will be deleted when you destroy the stack.


## Verifying a JWT token from your Cognito Auth application in a .Net Core API

This was surprisingly simple! We just need to configure Token validation in the application startup and away you go.

So, in your `Startup.cs` file:
```
public void ConfigureServices(IServiceCollection services) {
	 ...
	 // I'm not sure if order matters but I did this after .AddControllers
	 services.AddAuthentication()
		 .AddJwtBearer(options => { options.TokenValidationParameters = GetCognitoTokenValidationParams(configRoot);});
 }

private TokenValidationParameters GetCognitoTokenValidationParams(IConfiguration configuration) { 
	var cognitoIssuer = configuration["Cognito:IssuerUrl"];
	var jwtKeySetUrl = $"{cognitoIssuer}/.well-known/jwks.json";
	var cognitoAudience = configuration["Cognito:ClientId"];
	return new TokenValidationParameters {
		IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>{
			// get JsonWebKeySet from AWS
			var json = new WebClient().DownloadString(jwtKeySetUrl);
			var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;    
         return (IEnumerable<SecurityKey>)keys;
     },
     ValidIssuer = cognitoIssuer, ValidateIssuerSigningKey = true, ValidateIssuer = true,
     ValidateLifetime = true, ValidAudience = cognitoAudience }; 
}
``` 

and then your `appsettings.json` file should look something like:
```
{ ... 
	"Cognito": { 
		"ClientId": "{your-client-id-here}",
		"IssuerUrl": "https://cognito-idp.{your-aws-region-here}.amazonaws.com/{your-aws-region-here}_{your-cognito-pool-id-here}"
	}
} 
```
Once these are set up, you can just use the `[Authorize]` attribute on your controllers / endpoints to protect them by only allowing requests with valid Bearer (IdToken) tokens through.