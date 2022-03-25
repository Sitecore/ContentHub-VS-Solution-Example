# ContentHub-VS-Solution-Example
Visual Studio Solution Example to download and base any Content Hub Development on. It supports Intellisense, Sync of scripts, Debugging and Unit-Testing e.g. for Content Hub Scripts and Azure Functions. It also stores External Page Components. Overall it provides guidance on how to best structure a Visual Studio Solution for Content Hub Projects and therefore accelerates Customers and Partners.

## Getting Started

### Preconditions
Clone the Repository and get yourself a content hub environment.

### Nuget Package
Check the documentation for the lates nuget package source: https://docs.stylelabs.com/contenthub/4.1.x/content/integrations/web-sdk/get-started.html

### Creat a user for Script Synchronization and Unit Testing in Content Hub Environment
You need a User to connect with your Visual Studio solution to the Content Hub environment so you can sync your scripts or unit test against your environment
1. Open the Manage View.
2. Open the Users App
3. Add a User and give a name e.g.: "CommandlineUser"
4. Assign required (all) modules.
5. Switch to “User groups“ Tab and click "Add to user group"
6. Search for "Superusers", select  checkbox and confirm with "Select" Button
7. You might see the error message "Missing from Policy combination: Superusers". This can be ignored
8. Save Changes

#### Set Password of the User
You need to create a Token to authenticate when setting the password of the user via the API using e.g. Postman. 
1. Open Users App and click the Key Icon of  your user
2. Click the "New token" button
3. Copy the shown token.
4. Open Postman
5. Send the following Request
```
Request Url: [YOUR-CONTENT-HUB-URL]/api/[USER-ID]/setpasword
e.g. https://abc.stylelabs.com/api/account/12345/setpassword

Headers: 
x-auth-token 	[THE_TOKEN_CREATED]
Content-Type	application/json

Body:
{"password":"[YOUR-PASSWORD]"}
```
Note: The User ID can be retrieved from the URL, when editing the User.

### Create OAuth Client
The OAuth Client is required to connect with your Visual Studio solution to the Content Hub environment so you can sync your scripts or unit test against your environment.
1. Open the Mange View
2. Open the "OAuth clients" app
3. Create a new OAuth client
4. Fill in the mandatory fields

e.g.:
Name: SolutionUser
Client Id: SolutionUser
ClientSecret: SolutionUser
Redirect Url: [YOUR-CONTENT-HUB-URL]
Client Type (can stay blank)
Users: [SELECT-USER-CREATED-PREVIOUSLY]

5. Save the OAuth Client

### Setting user.properties to sync with Sandbox
Put either local.settings.json file into root of the following projects or handle via user secrets. Use the provided content and adjust user and client information accordingly. Ignore those files so they are not shared with Git Repo
```
Sitecore.CH.Implementation.Scripts.Tests
{
  "M": {
    "Host": "[YOUR-CONTENT-HUB-URL]",
    "ClientId": "[YOUR-CLIENT-ID]",
    "ClientSecret": "[YOUR-CLIENT-SECRET]",
    "UserName": "[YOUR-USER-NAME]",
    "Password": "[YOUR-PASSWORD]",
    "KnownSSoRedirects": []
  }
}
```
```
Sitecore.CH.Implementation.CommandLine
{
  "M": {
    "Host": "[YOUR-CONTENT-HUB-URL]",
    "ClientId": "[YOUR-CLIENT-ID]",
    "ClientSecret": "[YOUR-CLIENT-SECRET]",
    "UserName": "[YOUR-USER-NAME]",
    "Password": "[YOUR-PASSWORD]",
    "KnownSSoRedirects": []
  },
  "CommandLine": {
    "ScriptPush": {
      "_scriptDirectoryPath": "[YOUR-REPO-PATH]\\src\\Sitecore.CH.Implementation.Scripts",
      "scriptDirectoryPath": "[YOUR-REPO-PATH]\\src\\Sitecore.CH.Implementation.Scripts"
    }
  }
}
```
Note: Repository Path may look like this: "C:\\repos\\ContentHub\\SUGCON\\src\\Sitecore.CH.Implementation.Scripts

```
Sitecore.CH.Implementation.AzFunctions
{
 "M": {
    "Host": "[YOUR-CONTENT-HUB-URL]",
    "ClientId": "[YOUR-CLIENT-ID]",
    "ClientSecret": "[YOUR-CLIENT-SECRET]",
    "UserName": "[YOUR-USER-NAME]",
    "Password": "[YOUR-PASSWORD]",
    "KnownSSoRedirects": []
  }
}
```


## How to sync scripts
In your solution, perform a right mouse click on the Sitecore.CH.Implementation.CommandLine project.

Choose "Debug" --> "Start new Instance"

A command prompt will open that performs the magic.

If the build fails, you get an error message.

## How to sync Azure Functions
in work

## Debugging
in work

## Unit Testing
in work
