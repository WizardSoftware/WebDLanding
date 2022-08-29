history.pushState(null, null, "");

window.addEventListener("popstate", function () {
  // Prevent Back button click
  history.pushState(null, null, "");

  // select back button integration mode
  if (singleServerMode === false) {
    // setup postMessage push to iframe (to be picked up and pushed into listening web viewer)
    // via injected /message-forward.js
    document
      .getElementById("webdirect-frame")
      .contentWindow.postMessage(backEventName, "*");
  } else {
    // find the main iframe, then navigate through its children iframes and post the message
    // this only works on single origin solutions.
    let frames = Array.from(
      document
        .getElementById("webdirect-frame")
        .contentWindow.document.getElementsByTagName("iframe")
    );
    if (frames.length) {
      // fore each frame found, relay the event by re-posting it down to the child frame
      frames.forEach((frame) => {
        frame.contentWindow.postMessage(backEventName, "*");
      });
    }
  }
});

iframe = document.getElementById("webdirect-frame");
iframe.addEventListener("load", function () {
  history.pushState(null, null, "");
});

window.addEventListener("load", function () {
  if (homeUri) {
    uri = uri + "?homeurl=" + homeUri + "/logoff.html";
  }
  document.getElementById("webdirect-frame").src = uri;
});

window.addEventListener("beforeunload", function (e) {
  e.preventDefault();
  return (e.returnValue = "Are you sure you want to exit?");
});
