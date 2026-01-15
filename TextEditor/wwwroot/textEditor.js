// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.textEditor = {
    mouseMoveLastCall: 0,
    //mouseMoveSkippedCount: 0,
    //mouseMoveDidCount: 0,
    thinksLeftMouseButtonIsDown: false,
    mouseDownDetailsRank: 1,
    mouseStopTimer: null,
    mouseStopDelay: 300,
    keydownStopTimer: null,
    keydownStopDelay: 3000,
    cursorBlinkingStopTimer: null,
    cursorBlinkingStopDelay: 1000,
    cursorIsBlinking: true,
    editorLeft: 0,
    editorTop: 0,
    setFocus: function () {
        let textEditorElement = document.getElementById("te_component-id");
        if (textEditorElement) {
            textEditorElement.focus();
        }
    },
    constructDefaultMeasurements: function () {
        return {
            CharacterWidth: 0,
            LineHeight: 0,
            EditorWidth: 0,
            EditorHeight: 0,
            EditorLeft: 0,
			EditorTop: 0,
			ScrollbarLiteralWidth: 0,
			ScrollbarLiteralHeight: 0,
        };
    },
    isDefaultMeasurements: function (measurements) {
        return (measurements.CharacterWidth && measurements.CharacterWidth == 0) &&
               (measurements.LineHeight && measurements.LineHeight == 0) &&
               (measurements.EditorWidth && measurements.EditorWidth == 0) &&
               (measurements.EditorHeight && measurements.EditorHeight == 0) &&
               (measurements.ScrollbarLiteralWidth && measurements.ScrollbarLiteralWidth == 0) &&
               (measurements.ScrollbarLiteralHeight && measurements.ScrollbarLiteralHeight == 0);
    },
    initializeAndTakeMeasurements: function (dotNetHelper) {

        let cursorElement = document.getElementById("te_cursor-id");
        if (cursorElement) {
            cursorElement.className = "te_cursor-class ide_te_blink";
            this.cursorIsBlinking = true;
        }

        let contentElement = document.getElementById("te_component-id");
        if (!contentElement) {
            return this.constructDefaultMeasurements();
        }
        
        let measurements = this.takeMeasurements();
        if (!this.isDefaultMeasurements(measurements)) {
            // only add the listeners if the measurements were non-default
            
            contentElement.addEventListener('mousedown', (event) => {
                if ((event.buttons & 1) === 1) {
                    this.thinksLeftMouseButtonIsDown = true;
                    if (event.detail % 3 == 0) {
                        this.mouseDownDetailRank = 3;
                    }
                    else if (event.detail % 2 == 0) {
                        this.mouseDownDetailRank = 2;
                    }
                    else {
                        this.mouseDownDetailRank = 1;
                    }
                }

                if (this.cursorIsBlinking) this.stopCursorBlinking(cursorElement);

                let boundingClientRect = contentElement.getBoundingClientRect();
                this.editorLeft = boundingClientRect.left;
                this.editorTop = boundingClientRect.top;

                dotNetHelper.invokeMethodAsync(
                    "OnMouseDown",
                    event.buttons,
                    event.clientX + contentElement.scrollLeft - this.editorLeft,
                    event.clientY + contentElement.scrollTop - this.editorTop,
                    event.shiftKey,
                    this.mouseDownDetailRank);
            });
            
            contentElement.addEventListener('mousemove', (event) => {
                if ((event.buttons & 1) === 0) {
                    this.thinksLeftMouseButtonIsDown = false;
                }
                if (this.thinksLeftMouseButtonIsDown) {
        
                    if (this.cursorIsBlinking) this.stopCursorBlinking(cursorElement);
        
                    const now = new Date().getTime();
                    // 95 did, 1000 skipped, at 25ms throttle
                    if (now - textEditor.mouseMoveLastCall >= 25) {
                        //this.mouseMoveDidCount++;
                        textEditor.mouseMoveLastCall = now;
                        dotNetHelper.invokeMethodAsync(
                            "OnMouseMove",
                            event.buttons,
                            event.clientX + contentElement.scrollLeft - this.editorLeft,
                            event.clientY + contentElement.scrollTop - this.editorTop,
                            event.shiftKey);
                    }
                    /*else {
                        this.mouseMoveSkippedCount++;
        				
                        if (this.mouseMoveSkippedCount % 1000 == 0) {
        				    // Breakpoint here in the user agent debugger
                            this.mouseMoveSkippedCount = this.mouseMoveSkippedCount;
        				}
                    }*/
                }
                else {
                    clearTimeout(this.mouseStopTimer); // Reset timer on every move
                    this.mouseStopTimer = setTimeout(() => {
                        if (!this.thinksLeftMouseButtonIsDown) {
                            dotNetHelper.invokeMethodAsync(
                                "ReceiveTooltip",
                                event.clientX,
                                event.clientY,
                                event.clientX + contentElement.scrollLeft - this.editorLeft,
                                event.clientY + contentElement.scrollTop - this.editorTop);
                        }
                    }, this.mouseStopDelay);
                }
            });

            contentElement.addEventListener('mouseout', (event) => {
                clearTimeout(this.mouseStopTimer);
            });

            contentElement.addEventListener('keydown', (event) => {
                if (this.cursorIsBlinking) this.stopCursorBlinking(cursorElement);
                switch (event.key) {
                    case "ArrowLeft":
                        event.preventDefault();
                        break;
                    case "ArrowDown":
                        event.preventDefault();
                        break;
                    case "ArrowUp":
                        event.preventDefault();
                        break;
                    case "ArrowRight":
                        event.preventDefault();
                        break;
                    case "Tab":
                        event.preventDefault();
                        break;
                }
                if (event.ctrlKey) {
                    switch (event.key) {
                        case "v":
                            this.readClipboard().then((text) => {
                                dotNetHelper.invokeMethodAsync(
                                    "OnPaste",
                                    text);
                            });
                            break;
                        case "c":
                            dotNetHelper.invokeMethodAsync("OnCopy");
                            break;
                    }
                }
                else {
                    dotNetHelper.invokeMethodAsync(
                        "OnKeydown",
                        event.key,
                        event.shiftKey,
                        event.ctrlKey);
                }
                clearTimeout(this.keydownStopTimer); // Reset timer on every move
                    this.keydownStopTimer = setTimeout(() => {
                        dotNetHelper.invokeMethodAsync("ReceiveKeyboardDebounce");
                    }, this.keydownStopDelay);
            });
        }
        
        return measurements;
    },
    takeMeasurements: function () {
		let textEditorElement = document.getElementById("te_component-id");

        if (!textEditorElement) {
            return this.constructDefaultMeasurements();
        }
        
        let boundingClientRect = textEditorElement.getBoundingClientRect();
        this.editorLeft = boundingClientRect.left;
        this.editorTop = boundingClientRect.top;

        let measureTextElement = document.createElement("div");
        measureTextElement.className = "te_measure";
        textEditorElement.appendChild(measureTextElement);
        let elevenTimesAlphabetAndDigits = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
        measureTextElement.innerText = elevenTimesAlphabetAndDigits;
        let characterWidth = measureTextElement.offsetWidth / elevenTimesAlphabetAndDigits.length;
        let lineHeight = measureTextElement.offsetHeight;
        textEditorElement.removeChild(measureTextElement);
        
        // "literal" as opposed to the "scrollHeight", this is the amount of width the y-axis scrollbar measures
        // "literal" as opposed to the "scrollWidth", this is the amount of height the x-axis scrollbar measures
        let measureScrollbarLiteralElement = document.createElement("div");
        textEditorElement.appendChild(measureScrollbarLiteralElement);
        measureScrollbarLiteralElement.setAttribute('style', "width:100px; height:100px; overflow:scroll;");
        let scrollbarLiteralWidth = measureScrollbarLiteralElement.offsetWidth - measureScrollbarLiteralElement.clientWidth;
        let scrollbarLiteralHeight = measureScrollbarLiteralElement.offsetHeight - measureScrollbarLiteralElement.clientHeight;
        
        textEditorElement.removeChild(measureScrollbarLiteralElement);

        const root = document.documentElement;
        let lineHeightPropertyValue = lineHeight + "px";
        // would this avoid layout for no reason?
        if (root.style.getPropertyValue('--te_line-height') !== lineHeightPropertyValue)
            root.style.setProperty('--te_line-height', lineHeightPropertyValue);

        return {
            CharacterWidth: characterWidth,
            LineHeight: lineHeight,
            EditorWidth: textEditorElement.offsetWidth,
            EditorHeight: textEditorElement.offsetHeight,
			ScrollbarLiteralWidth: scrollbarLiteralWidth,
			ScrollbarLiteralHeight: scrollbarLiteralHeight,
        }
    },
    stopCursorBlinking: function (cursorElement) {
        if (cursorElement && this.cursorIsBlinking) {
            cursorElement.className = "te_cursor-class";
            this.cursorIsBlinking = false;
            clearTimeout(this.cursorBlinkingStopTimer); // Reset timer on every move
            this.cursorBlinkingStopTimer = setTimeout(() => {
                if (!this.cursorIsBlinking) {
                    cursorElement.className = "te_cursor-class ide_te_blink";
                    this.cursorIsBlinking = true;
                }
            }, this.cursorBlinkingStopDelay);
        }
    },
    readClipboard: async function () {
        // domexception-on-calling-navigator-clipboard-readtext
        // https://stackoverflow.com/q/56306153/14847452
        // ----------------------------------------------------
        // First, ask the Permissions API if we have some kind of access to
        // the "clipboard-read" feature.
        try {
            return await navigator.permissions.query({ name: "clipboard-read" }).then(async (result) => {
                // If permission to read the clipboard is granted or if the user will
                // be prompted to allow it, we proceed.

                if (result.state === "granted" || result.state === "prompt") {
                    return await navigator.clipboard.readText().then((data) => {
                        return data;
                    });
                } else {
                    return "";
                }
            });
        } catch (e) {
            // Debugging Linux-Ubuntu (2024-04-28)
            // -----------------------------------
            // Reading clipboard is not working.
            //
            // Fixed with the following inner-try/catch block.
            //
            // This fix upsets me. Seemingly the permission
            // "clipboard-read" doesn't exist for some user-agents
            // But so long as you don't check for permission it lets you read
            // the clipboard?
            try {
                return navigator.clipboard
                    .readText()
                    .then((clipText) => {
                        return clipText;
                    });
            } catch (innerException) {
                return "";
            }
        }
    },
    setClipboard: function (value) {
        // how-do-i-copy-to-the-clipboard-in-javascript:
        // https://stackoverflow.com/a/33928558/14847452
        // ---------------------------------------------
        // Copies a string to the clipboard. Must be called from within an
        // event handler such as click. May return false if it failed, but
        // this is not always possible. Browser support for Chrome 43+,
        // Firefox 42+, Safari 10+, Edge and Internet Explorer 10+.
        // Internet Explorer: The clipboard feature may be disabled by
        // an administrator. By default a prompt is shown the first
        // time the clipboard is used (per session).
        if (window.clipboardData && window.clipboardData.setData) {
            // Internet Explorer-specific code path to prevent textarea being shown while dialog is visible.
            return window.clipboardData.setData("Text", text);

        } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
            var textarea = document.createElement("textarea");
            textarea.textContent = value;
            textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in Microsoft Edge.
            document.body.appendChild(textarea);
            textarea.select();
            try {
                return document.execCommand("copy");  // Security exception may be thrown by some browsers.
            } catch (ex) {
                console.warn("Copy to clipboard failed.", ex);
                return false;
            } finally {
                document.body.removeChild(textarea);
                let textEditorElement = document.getElementById("te_component-id");
                if (textEditorElement) {
                    textEditorElement.focus();
                }
            }
        }
    },
}