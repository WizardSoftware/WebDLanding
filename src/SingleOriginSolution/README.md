# Single Origin Use

This `index.html` works for a hard-coded web-direct solution hosted on the same origin as the web-direct engine.

## Getting Started

1. Update the variables in the script section of the `index.html` for your server name and solution file name.
2. Deploy the `index.html` and `logoff.html` file to your FMServer Root Site `/HTTPServer/conf` directory.
3. Enable "homeurl" support to your FileMaker Server
4. Add this web viewer your layout.

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

_Note: this works only if all content, these html files, and the web-direct session are hosted on the same domain name. If you need multiple domain names, checkout the main project which supports that._
