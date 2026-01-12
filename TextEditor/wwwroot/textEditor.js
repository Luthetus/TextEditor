// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.ideTextEditor = {
    setFocus: function () {
        let element = document.getElementById("te_component-id");
        if (element) {
            element.focus();
        }
    },
    getTextEditorMeasurements: function () {
        let element = document.getElementById("te_component-id");

        if (!element) {
            return {
                CharacterWidth: 5,
                LineHeight: 5
            }
        }
        
        /*<div class="@_wrapperCssClass"
        	 style="@_wrapperCssStyle">
        	<div class="ide_te_measure-charWidth-lineHeight-wrap ide_te_row">
        	    <div class="ide_te_measure-charWidth-lineHeight ide_te_row"
        	         id="@_measureCharacterWidthAndLineHeightElementId">
        	    </div>
        	</div>
        </div>*/
        
        let child = document.createElement("div");
        element.appendChild(child);
        
        let elevenTimesAlphabetAndDigits = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
        child.innerText = elevenTimesAlphabetAndDigits;
        
        let fontWidth = child.offsetWidth / elevenTimesAlphabetAndDigits.length;
        let lineHeight = child.offsetHeight;
        
        element.removeChild(child);

        return {
            CharacterWidth: fontWidth,
            LineHeight: lineHeight
        }
    },
}