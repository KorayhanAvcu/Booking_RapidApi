// StayFinder - Ortak script (tüm sayfalarda yüklenir)
// Favori (kalp) butonu: otel listesi ve otel detayında aynı davranış.
(function () {
    'use strict';

    document.querySelectorAll('.fav-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var active = btn.classList.toggle('is-active');
            var icon = btn.querySelector('i');
            if (icon) {
                icon.className = active ? 'bi bi-heart-fill' : 'bi bi-heart';
            }
            btn.setAttribute('aria-label', active ? 'Favorilerden çıkar' : 'Favorilere ekle');
        });
    });
})();
