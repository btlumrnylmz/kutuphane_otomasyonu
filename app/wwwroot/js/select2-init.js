// Select2 Initialization Script
// Tüm sayfalarda otomatik olarak çalışır

(function() {
    'use strict';
    
    // jQuery'nin yüklenmesini bekle
    function initSelect2WhenReady() {
        if (typeof jQuery === 'undefined' || typeof jQuery.fn.select2 === 'undefined') {
            // jQuery veya Select2 henüz yüklenmediyse, 100ms sonra tekrar dene
            setTimeout(initSelect2WhenReady, 100);
            return;
        }
        
        var $ = jQuery;
        
        // Select2'yi başlat
        function initSelect2() {
            // Tüm select elementlerine Select2 uygula (zaten Select2 uygulanmamış olanlar)
            $('select.form-select, select:not(.select2-hidden-accessible):not([data-select2-init])').each(function() {
                var $select = $(this);
                
                // Eğer zaten Select2 uygulanmışsa atla
                if ($select.hasClass('select2-hidden-accessible')) {
                    return;
                }
                
                // İşaretle ki tekrar işlenmesin
                $select.attr('data-select2-init', 'true');
                
                // Özel placeholder belirle
                var placeholder = 'Seçiniz...';
                var firstOption = $select.find('option:first');
                if (firstOption.length && (firstOption.val() === '' || firstOption.val() === null)) {
                    placeholder = firstOption.text() || 'Seçiniz...';
                }
                
                // Option sayısını kontrol et
                var optionCount = $select.find('option').length;
                
                // Select2 yapılandırması
                var config = {
                    theme: 'bootstrap-5',
                    language: 'tr',
                    width: '100%',
                    placeholder: placeholder,
                    allowClear: firstOption.length > 0 && (firstOption.val() === '' || firstOption.val() === null),
                    dropdownParent: $select.closest('.modal').length ? $select.closest('.modal') : $('body'),
                    // Arama özelliği (5'ten fazla seçenek varsa etkinleştir)
                    minimumResultsForSearch: optionCount > 5 ? 0 : Infinity
                };
                
                // Eğer çoklu seçim yapılabiliyorsa
                if ($select.attr('multiple')) {
                    config.closeOnSelect = false;
                }
                
                // Select2'yi başlat
                try {
                    $select.select2(config);
                    
                    // Form validasyonu ile entegrasyon
                    $select.on('select2:select select2:clear', function() {
                        if (this.setCustomValidity) {
                            this.setCustomValidity('');
                        }
                    });
                } catch (e) {
                    console.error('Select2 initialization error:', e);
                    // Hata durumunda işareti kaldır ki tekrar denenebilsin
                    $select.removeAttr('data-select2-init');
                }
            });
        }
        
        // Sayfa yüklendiğinde başlat
        $(document).ready(function() {
            initSelect2();
        });
        
        // AJAX ile içerik yüklendikten sonra da başlat
        $(document).ajaxComplete(function() {
            setTimeout(initSelect2, 50);
        });
        
        // Dinamik içerik için MutationObserver
        if (typeof MutationObserver !== 'undefined') {
            var observer = new MutationObserver(function(mutations) {
                var shouldInit = false;
                mutations.forEach(function(mutation) {
                    mutation.addedNodes.forEach(function(node) {
                        if (node.nodeType === 1 && 
                            (node.tagName === 'SELECT' || (typeof $ !== 'undefined' && $(node).find('select').length > 0))) {
                            shouldInit = true;
                        }
                    });
                });
                if (shouldInit) {
                    setTimeout(initSelect2, 100);
                }
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        }
        
        // Özel Select2 olayları
        $(document).on('select2:open', function(e) {
            // Dropdown açıldığında arama kutusuna odaklan
            setTimeout(function() {
                $('.select2-search__field').focus();
            }, 100);
        });
        
        // Form submit edildiğinde Select2 değerlerini koru (gerekirse)
        $(document).on('submit', 'form', function() {
            // Select2 değerleri otomatik olarak korunur, destroy etmeye gerek yok
        });
    }
    
    // DOM hazır olduğunda başlat
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSelect2WhenReady);
    } else {
        initSelect2WhenReady();
    }
})();

