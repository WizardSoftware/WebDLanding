# WebDLanding

Web-Direct Landing page for Wizard Web-Direct deployed solutions.

## Features

- Land multiple interfaces on a single site across different FileMaker Servers.
- `onbeforeunload` support.
- Back button suppression / integration.

Use WebDLanding as a sort of reverse proxy for Web-Direct.

Provide a best effort to warn users that refreshing the page, or otherwise navigating away will lose their place in the web-direct application. Note: support varies by browser.

Avoid browser back button taking the user out of the solution. Provide support for solutions to listen to the native browser back button and react accordingly.

## FileMaker Server Setup

Update web.config in the FMServer Root Site `/HTTPServer/conf`:

```xml
<add name="Content-Security-Policy" value="frame-ancestors 'self' https://*.wizardsoftware.net" />
```

This allows WebDLanding to iframe in the content of the web direct session.

## FileMaker Server Configuration For Back Button Support

Support for the back button requires some further configuration on the FileMaker Server.

### Enable Home URL Support

Update the `jwpc_prefs.xml` file located in the `/Web Publishing/conf/jwpc_prefs.xml`

```xml
<parameter name="homeurlenabled">yes</parameter>
```

### Add `https://webdlanding.wizardsoftware.net` as an allowed home url

Update the `jwpc_prefs.xml` file located in the `/Web Publishing/conf/jwpc_prefs.xml`

```xml
<parameter name="customhomeurl">https://webdlanding.wizardsoftware.net</parameter>
```

### Restart FileMaker Services

🪟+ R => services.msc => FileMaker Server => Restart

## Back Button Integration

WebDLanding captures the Browser Back Button being pressed. When that occurrs, WebDLanding uses the `postMessage` api to signal that the back button was pressed. Web Viewers inside the solution can be configured to receive this event and react accordingly.

### Web Viewer content for each layout

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

### Add postMessage forwarding script to out going FileMaker Web-Direct requests

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

### Web.Config Mods

We need to modify the web.config in FMServer Root Site `/HTTPServer/conf` to add an extra script tag that will forward messages sent from the parent page down into the web viewers.

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
