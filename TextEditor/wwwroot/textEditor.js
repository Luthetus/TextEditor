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
               //(measurements.EditorLeft && measurements.EditorLeft == 0) &&
               //(measurements.EditorTop && measurements.EditorTop == 0) &&
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
                dotNetHelper.invokeMethodAsync(
                    "OnKeydown",
                    event.key,
                    event.shiftKey,
                    event.ctrlKey);
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
            //EditorLeft: boundingClientRect.left,
			//EditorTop: boundingClientRect.top,
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
}