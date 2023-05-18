# WebDLanding

The main solution provides a landing and routing page for Web-Direct deployed solutions hosted across multiple domains.

If your needs are simple: one solution with all hosting on a single domain, you can use the [version located in `/src/SingleOriginSolution`](https://github.com/WizardSoftware/WebDLanding/tree/main/src/SingleOriginSolution).

## Features

- Warn users before leaving the page that they may lose their work.
- Suppress browser back button exiting the application.
  - Support for integrating browser back button into a solution to call app-specific "back button" functionality.
- Single server and routed mode.
  - Land multiple interfaces on a single site across different FileMaker Servers.

Provide a best effort to warn users that refreshing the page, or otherwise navigating away will lose their place in the web-direct application. Note: support varies by browser.

Avoid browser back button taking the user out of the solution. Provide support for solutions to listen to the native browser back button and react accordingly.

WebDLanding can act as a sort of reverse proxy for Web-Direct. You can create user-friendly DNS records pointed at WebDLanding and then route them, behind the scenes to the correct FileMaker Server.

## Getting Started: Single Server Mode

You must install this project inside the FileMaker Server created website, be default located at `/FileMaker Server/HTTPServer/conf`.

### Deploy to FileMaker Server Website

1. Install ASP.NET Core Hosting Bundle.
2. Run Windows update, it might contain updates for ASP.NET.

Read about the [hosting bundle here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/hosting-bundle?view=aspnetcore-6.0) and [download it here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.8-windows-hosting-bundle-installer).

2. Copy the release files to the FMWebSite website located at `/FileMaker Server/HTTPServer/conf`. DO NOT copy web.config file. Keep existing file and modify accordingly per Step 6.

Copy the binaries from the release and deploy those to the directory.

3. Create user-friendly DNS records for your Web-Direct solution pointed at the server.

This must be configured in your DNS host.

4. Ensure that the FMWebSite bindings are setup to handle this DNS record.

This could be with a wildcard binding, or specific bindings. The key is that all DNS entries used are the same for both the landing (entry) and the web direct host.

5. Update `appsettings.json` to include your site(s) in the AppModels section and specify server mode (Single in this case).

```json
  "WebDLandingConfig":{
    "SingleServerMode": true
  },
  "AppModels": [
    {
      "EntryHostName": "webdirect.example.com",
      "DatabaseName": "FileMakerFile",
      "WebDirectHostName": "webdirect.example.com"
    }
  ],
```

6. Modify the `web.config` file to include the ASP.NET Core hosting modules:

```xml
<system.webServer>
  <handlers>
    <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
  </handlers>
  <aspNetCore processPath="dotnet" arguments=".\WebDLanding.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
</system.webServer>
```

## Getting Started: Pseudo Reverse Proxy Routed Mode

You must create and install this project as a separate website on a web server. It can be your FileMaker Server or a separate stand alone server. The main thing is separate DNS records.

### Install and configure WebDLanding

1. Setup the website on your web server, and copy the release files on to the website root.
2. Create a canonical WebDLanding DNS entry for hosting the application.
3. Create user-friendly DNS records for your Web-Direct solutions pointed at this site.
4. Update the configuration (TODO: document options) for each site.

### Allow WebDLanding to iframe your Web-Direct Solutions

Update web.config in the FileMaker Server root site `/FileMaker Server/HTTPServer/conf`:

```xml
<add name="Content-Security-Policy" value="frame-ancestors 'self' https://www.example.com" />
```

Replacing www.example.com with your cononical WebDLanding site from step #2 above. This allows WebDLanding to iframe in the content of the web direct session through it.

### Add HomeURL Support to enable iframe inception escape

Since the landing page uses an iframe, the native homeurl logout option will just create a loop of putting another iframe inside another iframe. This could pose challenges if a user logs in and out repeatedly. To avoid this, we use a `top.location.href` in our custom HomeURL logout page to escape to the top level.

#### Enable Home URL Support for WebDLanding

Update the `jwpc_prefs.xml` file located in the FileMaker Server install directory: `/FileMaker Server/Web Publishing/conf/jwpc_prefs.xml`

```xml
<parameter name="homeurlenabled">yes</parameter>
```

Add `https://webdlanding.example.com` as an allowed home url

```xml
<parameter name="customhomeurl">https://webdlanding.example.com</parameter>
```

Finally restart FileMaker Server services

🪟+ R => services.msc => FileMaker Server => Restart

### FileMaker Server Configuration For Back Button Support

Support for the back button requires some further configuration on the FileMaker Server.

WebDLanding captures the Browser Back Button being pressed. When that occurrs, WebDLanding uses the `postMessage` api to signal that the back button was pressed. Web Viewers inside the solution can be configured to receive this event and react accordingly.

#### Web Viewer content for each layout

In order to receive this event, a web viewer must exist on the layout with the following content to receive the event posted from the parent window.

A sample of the web viewer content that must be provided on each layout:

```html
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1" />
</head>

<body></body>

<script>
    window.addEventListener("message", (d) => {
        if (d.data === "BackButtonPressed") {
            FileMaker.PerformScript("back_button", "");
        }
    });
</script>

</html>
```

#### Add postMessage forwarding script to out going FileMaker Web-Direct requests

Because the WebDLanding uses an iframe hosted (possibly) on a different site the intermediate Web-Direct responses must be amended to include the following script to forward events from its parent, down into its children.

Add the following snippet as a file named `message-forward.js` to FMServer Root Site `/HTTPServer/conf`:

```js
window.addEventListener("message", d => {
    // find all frames inside the webdirect session
    let frames = Array.from(document.getElementsByTagName("iframe"));
    if (frames.length) {
        // fore each frame found, relay the event by re-posting it down to the child frame
        frames.forEach(frame => {
            frame.contentWindow.postMessage(d.data, "*")
        });
    }
});
```

#### Web.Config Modifications

We need to modify the web.config in FMServer Root Site `/FileMaker Server/HTTPServer/conf` to add an extra script tag that will forward messages sent from the parent page down into the web viewers.

Update the existing inbound rule: `FMWebPublishing` adding two http server variables:

```xml
<set name="HTTP_X_ORIGINAL_ACCEPT_ENCODING" value="{HTTP_ACCEPT_ENCODING}" />
<set name="HTTP_ACCEPT_ENCODING" value="" />
```

Essentially what this does, is store the original accept encoding and clear the actual accept encoding. This is necessary because the server behind the IIS Reverse Proxy uses gzip, which means we can't easily insert our script snipet.

Add two new Outbound Rules

For requests that had their accept encoding removed, restore it on the way back out.

```xml
<rule name="RestoreAcceptEncoding" preCondition="NeedsRestoringAcceptEncoding">
<match serverVariable="HTTP_ACCEPT_ENCODING" pattern="^(.*)" />
  <action type="Rewrite" value="{HTTP_X_ORIGINAL_ACCEPT_ENCODING}" />
</rule>
```

For requests with a `</body>` tag, add our script tag to it.

```xml
<rule name="AddPostMessageForwarding" preCondition="IsHTML" patternSyntax="ExactMatch">
  <match filterByTags="None" pattern="&lt;/body>" />
  <action type="Rewrite" value="&lt;script type='text/javascript' src='/message-forward.js'>&lt;/script>&lt;/body>" />
</rule>
```

Add two PreConditions which are used by both outgoing rules.

```xml
<preConditions>
  <preCondition name="IsHTML">
    <add input="{RESPONSE_CONTENT_TYPE}" pattern="^text/html" />
    <add input="{URL}" pattern="^/fmi/(.*)" />
  </preCondition>
  <preCondition name="NeedsRestoringAcceptEncoding">
    <add input="{HTTP_X_ORIGINAL_ACCEPT_ENCODING}" pattern=".+" />
  </preCondition>
</preConditions>
```

Finally add both of the above variables to the allowed variables list:

```xml
<allowedServerVariables>
  <add name="HTTP_SEC_WEBSOCKET_EXTENSIONS" />
  <add name="HTTP_X_ORIGINAL_ACCEPT_ENCODING" />
  <add name="HTTP_ACCEPT_ENCODING" />
</allowedServerVariables>
```
