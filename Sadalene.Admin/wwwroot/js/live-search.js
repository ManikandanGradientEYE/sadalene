(function () {
    'use strict';

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
    }

    document.querySelectorAll('[data-live-search]').forEach(wireRegion);
})();
