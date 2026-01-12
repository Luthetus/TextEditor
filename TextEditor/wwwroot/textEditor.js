// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.ideTextEditor = {
    setFocus: function () {
        let textEditorElement = document.getElementById("te_component-id");
        if (textEditorElement) {
            textEditorElement.focus();
        }
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
            }
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
        measureScrollbarLiteralElement.setAttribute('style', "width:200px; height:200px; overflow:scroll;");
        let scrollbarLiteralWidth = measureScrollbarLiteralElement.offsetWidth;
        let scrollbarLiteralHeight = measureScrollbarLiteralElement.offsetHeight;
        let aaa = measureScrollbarLiteralElement.getBoundingClientRect();
        textEditorElement.removeChild(measureScrollbarLiteralElement);

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