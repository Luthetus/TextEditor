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
                CharacterWidth: 5,
                LineHeight: 5,
                EditorWidth: 0,
                EditorHeight: 0,
				BoundingClientRectLeft: 0,
				BoundingClientRectTop: 0,
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
        measureScrollbarLiteralElement.setAttribute('style', "width:0; height:0; overflow:scroll;");
        let scrollbarLiteralWidth = Math.ceil(elementReference.offsetWidth);
        let scrollbarLiteralHeight = Math.ceil(elementReference.offsetHeight),
        textEditorElement.removeChild(measureScrollbarLiteralElement);

        return {
            CharacterWidth: characterWidth,
            LineHeight: lineHeight,
            EditorWidth: Math.ceil(textEditorElement.offsetWidth),
            EditorHeight: Math.ceil(textEditorElement.offsetHeight),
            EditorLeft: boundingClientRect.left,
			EditorTop: boundingClientRect.top,
			ScrollbarLiteralWidth: scrollbarLiteralWidth,
			ScrollbarLiteralHeight: scrollbarLiteralHeight,
        }
    },
}