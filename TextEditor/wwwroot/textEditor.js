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
                LineHeight: 5
            }
        }
        
        let child = document.createElement("div");
        textEditorElement.appendChild(child);
        
        let elevenTimesAlphabetAndDigits = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
        child.innerText = elevenTimesAlphabetAndDigits;
        
        let characterWidth = child.offsetWidth / elevenTimesAlphabetAndDigits.length;
        let lineHeight = child.offsetHeight;
        
        textEditorElement.removeChild(child);

        return {
            CharacterWidth: characterWidth,
            LineHeight: lineHeight
        }
    },
}