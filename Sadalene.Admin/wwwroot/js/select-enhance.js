(function () {
    'use strict';

    function enhance(el) {
        if (!el || el.tomselect || el.multiple) return;
        new TomSelect(el, {
            create: false,
            allowEmptyOption: true,
            maxOptions: null
        });
    }

    function enhanceAll(root) {
        root.querySelectorAll('select.form-select').forEach(enhance);
    }

    function wireModal(modalEl) {
        modalEl.addEventListener('shown.bs.modal', function () {
            enhanceAll(modalEl);
        });
    }

    function enhanceModalsIn(root) {
        root.querySelectorAll('.modal').forEach(wireModal);
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('select.form-select').forEach(function (el) {
            if (el.closest('.modal')) return;
            enhance(el);
        });
        enhanceModalsIn(document);
    });

    window.SadaleneSelects = { enhance: enhance, enhanceModalsIn: enhanceModalsIn };
})();
