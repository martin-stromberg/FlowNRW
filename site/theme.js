(function () {
    "use strict";

    var STORAGE_KEY = "flownrw-theme";
    var root = document.documentElement;
    var media = window.matchMedia("(prefers-color-scheme: dark)");

    function systemTheme() {
        return media.matches ? "dark" : "light";
    }

    function storedTheme() {
        try {
            var stored = window.localStorage.getItem(STORAGE_KEY);
            return stored === "dark" || stored === "light" ? stored : null;
        } catch (e) {
            return null;
        }
    }

    function currentTheme() {
        var attr = root.getAttribute("data-theme");
        if (attr === "dark" || attr === "light") {
            return attr;
        }
        return systemTheme();
    }

    function applyStored() {
        var stored = storedTheme();
        if (stored) {
            root.setAttribute("data-theme", stored);
        } else {
            // Keine Nutzerwahl: Attribut freigeben, damit die
            // prefers-color-scheme-Media-Queries Systemwechsel live übernehmen.
            root.removeAttribute("data-theme");
        }
        updateToggle();
    }

    function updateToggle() {
        var button = document.getElementById("theme-toggle");
        if (!button) {
            return;
        }
        var dark = currentTheme() === "dark";
        button.setAttribute("aria-pressed", dark ? "true" : "false");
        var label = button.querySelector(".theme-toggle-label");
        if (label) {
            label.textContent = dark ? "Hell" : "Dunkel";
        }
    }

    function toggle() {
        var next = currentTheme() === "dark" ? "light" : "dark";
        root.setAttribute("data-theme", next);
        try {
            window.localStorage.setItem(STORAGE_KEY, next);
        } catch (e) {
            /* Persistenz optional */
        }
        updateToggle();
    }

    function onSystemChange() {
        if (!storedTheme()) {
            root.removeAttribute("data-theme");
            updateToggle();
        }
    }

    applyStored();

    document.addEventListener("DOMContentLoaded", function () {
        var button = document.getElementById("theme-toggle");
        if (button) {
            button.addEventListener("click", toggle);
            updateToggle();
            button.hidden = false;
        }
        if (typeof media.addEventListener === "function") {
            media.addEventListener("change", onSystemChange);
        } else if (typeof media.addListener === "function") {
            media.addListener(onSystemChange);
        }
    });
})();
