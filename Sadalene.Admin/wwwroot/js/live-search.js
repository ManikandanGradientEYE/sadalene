(function () {
    'use strict';

    // Keyed by the region's target selector, so pages can trigger a reload (e.g. from a custom
    // filter dropdown) without duplicating the fetch/history/page-link wiring logic below.
    var regions = {};

    function wireRegion(box) {
        var target = document.querySelector(box.dataset.target);
        if (!target) return;

        function load(url) {
            fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (r) { return r.text(); })
                .then(function (html) {
                    target.innerHTML = html;
                    history.replaceState(null, '', url);
                    wirePageLinks();
                    if (window.SadaleneSelects) window.SadaleneSelects.enhanceModalsIn(target);
                });
        }

        function wirePageLinks() {
            target.querySelectorAll('a.page-link[href]').forEach(function (a) {
                a.addEventListener('click', function (e) {
                    e.preventDefault();
                    load(a.getAttribute('href'));
                });
            });
        }

        var input = box.querySelector('[data-search-input]');
        if (input) {
            var timer;
            input.addEventListener('input', function () {
                clearTimeout(timer);
                timer = setTimeout(function () {
                    var url = new URL(window.location.href);
                    url.searchParams.set('search', input.value);
                    url.searchParams.set('pageNumber', '1');
                    load(url.toString());
                }, 300);
            });
        }

        wirePageLinks();

        regions[box.dataset.target] = load;
    }

    document.querySelectorAll('[data-live-search]').forEach(wireRegion);

    window.SadaleneLiveSearch = {
        reload: function (targetSelector, overrides) {
            var load = regions[targetSelector];
            if (!load) return;
            var url = new URL(window.location.href);
            Object.keys(overrides || {}).forEach(function (key) {
                if (overrides[key]) url.searchParams.set(key, overrides[key]);
                else url.searchParams.delete(key);
            });
            url.searchParams.set('pageNumber', '1');
            load(url.toString());
        }
    };
})();
