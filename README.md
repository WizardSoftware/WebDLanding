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

## FileMaker Server Configuration

1. Enable Home URL Support
2. Add `https://webdlanding.wizardsoftware.net` as an allowed home url
3. Restart FileMaker Services
4. Add postMessage forwarding script as referenced in https://github.com/WizardSoftware/WebDLanding/issues/6
