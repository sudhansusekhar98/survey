(function($) {
            $(document).ready(function() {
                // ---------- QUANTITY + / - for inputs (uses data-target -> input id) ----------
                $(document).on('click', '.qty-increment, .qty-decrement', function(e) {
                    e.preventDefault();
                    var $btn = $(this);
                    var targetId = $btn.attr('data-target');
                    if (targetId) {
                        var $input = $('#' + targetId);
                        if ($input.length) {
                            var val = parseInt($input.val() || '0', 10);
                            if ($btn.hasClass('qty-increment')) val = val + 1;
                            else if (val > 0) val = val - 1;
                            $input.val(val);
                        }
                        return;
                    }

                    // if button has data-qty-input-name (used inside cantilever blocks)
                    var qtyName = $btn.attr('data-qty-input-name');
                    if (qtyName) {
                        var $cantBlock = $btn.closest('.cantilever-block');
                        var $input = $cantBlock.find('input[name="' + qtyName + '"]');
                        if ($input.length) {
                            var val = parseInt($input.val() || '0', 10);
                            if ($btn.hasClass('qty-increment')) val = val + 1;
                            else if (val > 1) val = val - 1;
                            $input.val(val);
                        }
                    }
                });

                // ---------- Add / Remove pole blocks (Surveillance/ALPR/Traffic) ----------
                function updateTotals() {
                    $('#SurveillanceTotal').text(('0' + $('.pole-block[data-type="Surveillance"]').length).slice(-2));
                    $('#ALPRTotal').text(('0' + $('.pole-block[data-type="ALPR"]').length).slice(-2));
                    $('#TrafficTotal').text(('0' + $('.pole-block[data-type="Traffic"]').length).slice(-2));
                }

                // Add Surveillance pole
                $('#addSurveillancePole').on('click', function() {
                    var idx = $('#SurveillancePolesContainer .pole-block').length;
                    var template = `
                        <div class="card mb-2 pole-block" data-type="Surveillance" data-index="${idx}">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap">
                                    <strong>Pole #${idx + 1}</strong>
                                    <button type="button" class="remove-pole btn btn-link text-danger">Remove Pole</button>
                                </div>
                                <div class="cantilevers-container mb-2">
                                    ${buildCantileverHtml('SurveillancePoles', idx)}
                                </div>
                                <div>
                                    <button type="button" class="add-cantilever btn btn-outline-primary btn-sm">Add Cantilever</button>
                                </div>
                            </div>
                        </div>`;
                    $('#SurveillancePolesContainer').append(template);
                    updateTotals();
                });

                // Add ALPR pole
                $('#addALPRPole').on('click', function() {
                    var idx = $('#ALPRPolesContainer .pole-block').length;
                    var template = `
                        <div class="card mb-2 pole-block" data-type="ALPR" data-index="${idx}">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap">
                                    <strong>Pole #${idx + 1}</strong>
                                    <button type="button" class="remove-pole btn btn-link text-danger">Remove Pole</button>
                                </div>
                                <div class="cantilevers-container mb-2">
                                    ${buildCantileverHtml('ALPRPoles', idx)}
                                </div>
                                <div>
                                    <button type="button" class="add-cantilever btn btn-outline-primary btn-sm">Add Cantilever</button>
                                </div>
                            </div>
                        </div>`;
                    $('#ALPRPolesContainer').append(template);
                    updateTotals();
                });

                // Add Traffic pole
                $('#addTrafficPole').on('click', function() {
                    var idx = $('#TrafficPolesContainer .pole-block').length;
                    var template = `
                        <div class="card mb-2 pole-block" data-type="Traffic" data-index="${idx}">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap">
                                    <strong>Pole #${idx + 1}</strong>
                                    <button type="button" class="remove-pole btn btn-link text-danger">Remove Pole</button>
                                </div>
                                <div class="cantilevers-container mb-2">
                                    ${buildCantileverHtml('TrafficPoles', idx)}
                                </div>
                                <div>
                                    <button type="button" class="add-cantilever btn btn-outline-primary btn-sm">Add Cantilever</button>
                                </div>
                            </div>
                        </div>`;
                    $('#TrafficPolesContainer').append(template);
                    updateTotals();
                });

                // Remove pole
                $(document).on('click', '.remove-pole', function(e) {
                    e.preventDefault();
                    $(this).closest('.pole-block').remove();
                    $('.pole-block').each(function(i) {
                        $(this).find('strong').first().text('Pole #' + (i + 1));
                    });
                    updateTotals();
                });

                // Add cantilever inside a pole block
                $(document).on('click', '.add-cantilever', function(e) {
                    e.preventDefault();
                    var $pole = $( this).closest('.pole-block');
                    var type = $pole.data('type');
                    var idx = $pole.data('index') || $('.pole-block[data-type="' + type + '"]').index($pole);
                    var cCount = $pole.find('.cantilever-block').length;
                    $pole.find('.cantilevers-container').append(buildCantileverHtml((type + 'Poles'), idx, cCount));
                });

                // Remove cantilever
                $(document).on('click', '.remove-cantilever', function(e) {
                    e.preventDefault();
                    $(this).closest('.cantilever-block').remove();
                });

                // helper to build cantilever HTML
                function buildCantileverHtml(listName, poleIndex, cantIndex = 0) {
                    var lengthName = listName + '[' + poleIndex + '].Cantilevers[' + cantIndex + '].Length';
                    var qtyName = listName + '[' + poleIndex + '].Cantilevers[' + cantIndex + '].Quantity';
                    return `<div class="d-flex align-items-center mb-2 cantilever-block flex-wrap" data-cindex="${cantIndex}" style="flex-wrap:wrap;">
                                <select name="${lengthName}" class="form-select me-2 mb-2 mb-md-0" style="max-width:120px;">
                                    <option value="">Select Length</option>
                                    <option value="1 Meter">1 Meter</option>
                                    <option value="2 Meter">2 Meter</option>
                                    <option value="3 Meter">3 Meter</option>
                                    <option value="4 Meter">4 Meter</option>
                                </select>
                                <div class="d-flex align-items-center mb-2 mb-md-0">
                                    <button type="button" class="qty-decrement btn btn-danger btn-sm" data-qty-input-name="${qtyName}">-</button>
                                    <input type="number" name="${qtyName}" class="mx-2 form-control text-center" value="1" min="1" style="max-width:80px;" />
                                    <button type="button" class="qty-increment btn btn-success btn-sm" data-qty-input-name="${qtyName}">+</button>
                                </div>
                                <div class="w-100 d-block d-md-none"></div>
                                <button type="button" class="remove-cantilever btn btn-link text-danger ms-2 mt-2 mt-md-0">Remove</button>
                            </div>`;
                }

                // prevent form submission on Enter key in number fields
                $(document).on('keypress', 'input[type="number"]', function(e) {
                    if (e.which === 13) { e.preventDefault(); return false; }
                });
                    // ---------- Capture Image Button Handlers ----------
                    $('#captureExistingPoleImage').on('click', function(e) {
                        e.preventDefault();
                        $(this).prev('input[type="file"]').trigger('click');
                    });
                    $('#captureSurveillanceImage').on('click', function(e) {
                        e.preventDefault();
                        $(this).prev('input[type="file"]').trigger('click');
                    });
                    $('#captureALPRImage').on('click', function(e) {
                        e.preventDefault();
                        $(this).prev('input[type="file"]').trigger('click');
                    });
                    $('#captureTrafficImage').on('click', function(e) {
                        e.preventDefault();
                        $(this).prev('input[type="file"]').trigger('click');
                    });
                    $('#captureGantryImage').on('click', function(e) {
                        e.preventDefault();
                        $(this).prev('input[type="file"]').trigger('click');
                    });
            });
        })(jQuery);


      
                                document.addEventListener('click', function (e) {
                                    const preview = document.querySelector('.pole-preview');
                                    // Browse
                                    if (e.target.closest('.pole-browse-btn')) {
                                        const input = document.querySelector('.pole-upload-input');
                                        input.click();
                                        input.onchange = function () {
                                            handleFiles(input.files, preview);
                                        };
                                    }
                                    // Gallery
                                    if (e.target.closest('.pole-gallery-btn')) {
                                        const input = document.querySelector('.pole-upload-input');
                                        input.click();
                                        input.onchange = function () {
                                            handleFiles(input.files, preview);
                                        };
                                    }
                                    // Take Photo
                                    if (e.target.closest('.pole-take-btn')) {
                                        const modal = new bootstrap.Modal(document.getElementById('poleVideoModal'));
                                        const video = document.getElementById('poleCaptureVideo');
                                        const captureBtn = document.getElementById('poleCaptureBtn');
                                        let stream = null;
                                        video.srcObject = null;
                                        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                                            navigator.mediaDevices.getUserMedia({ video: true }).then(function (s) {
                                                stream = s;
                                                video.srcObject = stream;
                                                modal.show();
                                                captureBtn.onclick = function () {
                                                    let canvas = document.createElement('canvas');
                                                    canvas.width = video.videoWidth;
                                                    canvas.height = video.videoHeight;
                                                    canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
                                                    let wrapper = document.createElement('div');
                                                    wrapper.className = 'pole-preview-wrapper position-relative d-inline-block';
                                                    let img = document.createElement('img');
                                                    img.src = canvas.toDataURL('image/png');
                                                    img.className = 'pole-preview-img';
                                                    wrapper.appendChild(img);
                                                    let cancelBtn = document.createElement('button');
                                                    cancelBtn.className = 'btn btn-sm btn-danger position-absolute';
                                                    cancelBtn.innerHTML = '<i class="bi bi-x"></i>';
                                                    cancelBtn.onclick = function () { wrapper.remove(); };
                                                    wrapper.appendChild(cancelBtn);
                                                    preview.appendChild(wrapper);
                                                    modal.hide();
                                                    if (stream) stream.getTracks().forEach(track => track.stop());
                                                };
                                                document.getElementById('poleVideoModal').addEventListener('hidden.bs.modal', function () {
                                                    if (stream) stream.getTracks().forEach(track => track.stop());
                                                }, { once: true });
                                            }).catch(function () {
                                                alert('Camera access denied.');
                                            });
                                        } else {
                                            alert('Camera not supported.');
                                        }
                                    }
                                });
                                function handleFiles(files, preview) {
                                    Array.from(files).forEach(file => {
                                        if (file.type.startsWith('image/')) {
                                            let reader = new FileReader();
                                            reader.onload = function (e) {
                                                let wrapper = document.createElement('div');
                                                wrapper.className = 'pole-preview-wrapper position-relative d-inline-block';
                                                let img = document.createElement('img');
                                                img.src = e.target.result;
                                                img.className = 'pole-preview-img';
                                                wrapper.appendChild(img);
                                                let cancelBtn = document.createElement('button');
                                                cancelBtn.className = 'btn btn-sm btn-danger position-absolute';
                                                cancelBtn.innerHTML = '<i class="bi bi-x"></i>';
                                                cancelBtn.onclick = function () { wrapper.remove(); };
                                                wrapper.appendChild(cancelBtn);
                                                preview.appendChild(wrapper);
                                            };
                                            reader.readAsDataURL(file);
                                        }
                                    });
                                }
                                
                                    