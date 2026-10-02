// StayFinder - Ana sayfa scripti
// Misafir sayaç butonları ve çocuk yaşı alanının gösterimi.
(function () {
    'use strict';

    document.querySelectorAll('.counter').forEach(function (counter) {
        var input = counter.querySelector('.counter-value');
        var min = parseInt(counter.dataset.min, 10);
        var max = parseInt(counter.dataset.max, 10);

        counter.addEventListener('click', function (e) {
            var btn = e.target.closest('.counter-btn');
            if (!btn) return;

            var value = parseInt(input.value, 10);
            value = btn.dataset.action === 'plus'
                ? Math.min(value + 1, max)
                : Math.max(value - 1, min);
            input.value = value;

            // Çocuk sayısı 0 ise yaş alanını gizle
            if (counter.id === 'childCounter') {
                var wrapper = document.getElementById('childAgeWrapper');
                if (wrapper) {
                    wrapper.classList.toggle('d-none', value === 0);
                }
            }
        });
    });
})();
