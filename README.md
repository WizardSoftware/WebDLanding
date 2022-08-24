# WebDLanding

Web-Direct Landing page for Wizard Web-Direct deployed solutions.

## Features

- `onbeforeunload` support.

Best effort to warn users that refreshing the page, or otherwise navigating away will lose their place in the system. Support varies by browser.

- Back button suppression / integration.

Avoid back button taking the user out of the solution, and provide support for solutions to listen to the native browser back button and react accordingly.

## Back Button Integration

WebDLanding provides an opportunity for back button support to be added, by posting a message to child iframes that the back button was pressed.

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

## FileMaker Server Configuration for basic support

Update web.config in the http server directory

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

🪟➕R => services.msc => FileMaker Server => Restart

### Add postMessage forwarding script as referenced in <https://github.com/WizardSoftware/WebDLanding/issues/6>

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

#### Web.Config Mods

Rewrite Section

Add two new Outbound Rules

```xml
<rule name="RestoreAcceptEncoding" preCondition="NeedsRestoringAcceptEncoding">
<match serverVariable="HTTP_ACCEPT_ENCODING" pattern="^(.*)" />
  <action type="Rewrite" value="{HTTP_X_ORIGINAL_ACCEPT_ENCODING}" />
</rule>
<rule name="AddPostMessageForwarding" preCondition="IsHTML" patternSyntax="ExactMatch">
  <match filterByTags="None" pattern="&lt;/body>" />
  <action type="Rewrite" value="&lt;script type='text/javascript' src='/message-forward.js'>&lt;/script>&lt;/body>" />
</rule>
```

Add two PreConditions

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

Update the existing inbound rule: `FMWebPublishing` adding two http server variables:

```xml
<set name="HTTP_X_ORIGINAL_ACCEPT_ENCODING" value="{HTTP_ACCEPT_ENCODING}" />
<set name="HTTP_ACCEPT_ENCODING" value="" />
```

Add both of the above variables to the allowed variables list:

```xml
<allowedServerVariables>
  <add name="HTTP_SEC_WEBSOCKET_EXTENSIONS" />
  <add name="HTTP_X_ORIGINAL_ACCEPT_ENCODING" />
  <add name="HTTP_ACCEPT_ENCODING" />
</allowedServerVariables>
```
