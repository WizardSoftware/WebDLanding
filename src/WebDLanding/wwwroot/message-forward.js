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