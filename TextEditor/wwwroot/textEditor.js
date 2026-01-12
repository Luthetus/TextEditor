// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.ideTextEditor = {
    mouseMoveLastCall: 0,
    //mouseMoveSkippedCount: 0,
    //mouseMoveDidCount: 0,
    thinksLeftMouseButtonIsDown: false,
    mouseStopTimer: null,
    stopDelay: 300,
    setFocus: function () {
        let textEditorElement = document.getElementById("te_component-id");
        if (textEditorElement) {
            textEditorElement.focus();
        }
    },
    initializeAndGetMeasurements: function (dotNetHelper) {
    
        let contentElement = document.getElementById("te_component-id");
        if (!contentElement) {
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
        }
        
        contentElement.addEventListener('mousemove', (event) => {
            if ((event.buttons & 1) === 0) {
                this.thinksLeftMouseButtonIsDown = false;
            }
            if (this.thinksLeftMouseButtonIsDown) {

                if (this.cursorIsBlinking) {
                    cursorElement.className = "ide_te_text-editor-cursor ide_te_cursor-beam";
                    this.cursorIsBlinking = false;
                    clearTimeout(this.cursorBlinkingStopTimer); // Reset timer on every move
                    this.cursorBlinkingStopTimer = setTimeout(() => {
                        if (!this.cursorIsBlinking) {
                            cursorElement.className = "ide_te_text-editor-cursor ide_te_blink ide_te_cursor-beam";
                            this.cursorIsBlinking = true;
                        }
                    }, this.cursorBlinkingStopDelay);
                }

                const now = new Date().getTime();
                // Check if enough time has passed since the last execution
                // 157 did, 1000 skipped, at 15ms throttle
                // TODO: Fine tune this more
                // 155 did, 1000 skipped, at 16ms throttle
                // 141 did, 1000 skipped, at 17ms throttle
                // 138 did, 1000 skipped, at 18ms throttle
                // 124 did, 1000 skipped, at 19ms throttle
                // 123 did, 1000 skipped, at 20ms throttle
                // 119 did, 1000 skipped, at 20ms throttle
                // 103 did, 1000 skipped, at 23ms throttle
                // 101 did, 1000 skipped, at 24ms throttle
                // 95 did, 1000 skipped, at 25ms throttle
                // 83 did, 1000 skipped, at 30ms throttle
                if (now - ideTextEditor.mouseMoveLastCall >= 25) {
                    //this.mouseMoveDidCount++;
                    ideTextEditor.mouseMoveLastCall = now;
                    dotNetHelper.invokeMethodAsync(
                        "ReceiveContentOnMouseMove",
                        event.buttons,
                        event.clientX,
                        event.clientY,
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
                            event.clientY);
                    }
                }, this.stopDelay);
            }
        });
        
        return this.getTextEditorMeasurements();
    },
    getTextEditorMeasurements: function () {
		let textEditorElement = document.getElementById("te_component-id");

        if (!textEditorElement) {
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
        }
        
        let boundingClientRect = textEditorElement.getBoundingClientRect();
        
        let measureTextElement = document.createElement("div");
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
        root.style.setProperty('--te_line-height', lineHeight + "px");

        return {
            CharacterWidth: characterWidth,
            LineHeight: lineHeight,
            EditorWidth: textEditorElement.offsetWidth,
            EditorHeight: textEditorElement.offsetHeight,
            EditorLeft: boundingClientRect.left,
			EditorTop: boundingClientRect.top,
			ScrollbarLiteralWidth: scrollbarLiteralWidth,
			ScrollbarLiteralHeight: scrollbarLiteralHeight,
        }
    },
}