// StayFinder - Otel detay scripti
// Fotoğraf slider'ı: önceki / sonraki butonları ile yatay kaydırma.
// (Favori butonu ortak site.js içinde.)
(function () {
    'use strict';

    var track = document.getElementById('photoTrack');
    if (!track) return;

    document.querySelectorAll('.slider-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var item = track.querySelector('.photo-item');
            var gap = parseFloat(getComputedStyle(track).columnGap) || 0;
            var step = item ? item.offsetWidth + gap : track.clientWidth;
            track.scrollBy({
                left: step * parseInt(btn.dataset.dir, 10),
                behavior: 'smooth'
            });
        });
    });
})();
