// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.ideTextEditor = {
    setFocus: function () {
        let element = document.getElementById("te_component-id");
        if (element) {
            element.focus();
        }
    }
}