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

function setHeaderMessage() {
  let msg = document
    .getElementById("webdirect-frame")
    ?.contentDocument?.getElementById('login_header_msg');

  if (msg && loginHeaderMessage) {
    // load message from AppModel.
    msg.innerHTML = loginHeaderMessage;
  } else {
    setTimeout(setHeaderMessage, 200);
  }
}

iframe = document.getElementById("webdirect-frame");
iframe.addEventListener("load", function () {
  history.pushState(null, null, "");
  setHeaderMessage();
});

window.addEventListener('load', function () {
  // use uri variable, which is set in global scope from set-vars.js
  let url = new URL(uri);
  let parentSearchParams = new URLSearchParams(window.location.search);

  if (parentSearchParams.has('script')) {
    url.searchParams.append('script', parentSearchParams.get('script'));
    
    if (parentSearchParams.has('param')) {
      url.searchParams.append('param', parentSearchParams.get('param'));
    }
  }
  if (homeUri) {
    url.searchParams.append('homeurl', `${homeUri}/logoff.html`);
  }
  
  document.getElementById('webdirect-frame').src = url.href;
});

let allowExit = false;

window.addEventListener("message", (d) => {
  if (d.data == 'allow-exit') {
    allowExit = true;
    if (homeUri) {
      uri = uri + "?homeurl=" + homeUri + "/logoff.html";
    }
    document.getElementById("webdirect-frame").src = uri;
  }
});

window.addEventListener("beforeunload", function (e) {
  if (allowExit) {
    return;
  } else {
    e.preventDefault();
    return (e.returnValue = "Are you sure you want to exit?");
  }
});
