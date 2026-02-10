/**
 * ========================================
 * SETTINGS.JS - Advanced Settings Management
 * ========================================
 * File này chứa tất cả JavaScript cho module Settings
 */

var SettingsModule = (function () {
    'use strict';

    // ==================== PRIVATE VARIABLES ====================
    var _csrfToken = null;
    var _currentTheme = 'light';
    var _backupInProgress = false;

    // ==================== INITIALIZATION ====================
    function init() {
        _csrfToken = $('input[name="__RequestVerificationToken"]').val();
        _currentTheme = $('input[name="ThemeMode"]:checked').val() || 'light';
        
        initThemeSwitcher();
        initPasswordStrength();
        initPaymentToggles();
        initBackupFeatures();
        initFormValidation();
        
        console.log('Settings Module Initialized');
    }

    // ==================== THEME SWITCHER ====================
    function initThemeSwitcher() {
        // Apply current theme on page load
        applyTheme(_currentTheme);

        // Theme radio buttons
        $('input[name="ThemeMode"]').on('change', function () {
            _currentTheme = $(this).val();
            applyTheme(_currentTheme);
            showToast('Theme đã thay đổi thành ' + (_currentTheme === 'dark' ? 'Tối' : 'Sáng'), 'info');
        });

        // Quick theme toggle button (if exists)
        $('#quickThemeToggle').on('click', function () {
            _currentTheme = _currentTheme === 'light' ? 'dark' : 'light';
            $('input[name="ThemeMode"][value="' + _currentTheme + '"]').prop('checked', true);
            applyTheme(_currentTheme);
        });
    }

    function applyTheme(theme) {
        if (theme === 'dark') {
            $('body').addClass('dark-theme');
            $('body').removeClass('light-theme');
            
            // Update icons
            $('.theme-icon').removeClass('fa-moon-o').addClass('fa-sun-o');
        } else {
            $('body').addClass('light-theme');
            $('body').removeClass('dark-theme');
            
            // Update icons
            $('.theme-icon').removeClass('fa-sun-o').addClass('fa-moon-o');
        }

        // Save to localStorage for persistence
        localStorage.setItem('preferredTheme', theme);
    }

    // ==================== PASSWORD STRENGTH ====================
    function initPasswordStrength() {
        var $newPassword = $('#newPassword');
        var $confirmPassword = $('#confirmPassword');
        var $submitBtn = $('#submitBtn');
        var $strengthIndicator = $('#passwordStrength');

        if ($newPassword.length === 0) return;

        // Create strength indicator if not exists
        if ($strengthIndicator.length === 0) {
            $newPassword.after('<div id="passwordStrength" class="password-strength"></div>');
            $strengthIndicator = $('#passwordStrength');
        }

        $newPassword.on('keyup', function () {
            var password = $(this).val();
            var strength = calculatePasswordStrength(password);
            updateStrengthIndicator(strength, $strengthIndicator);
        });

        $confirmPassword.on('keyup', function () {
            var newPwd = $newPassword.val();
            var confirmPwd = $(this).val();
            
            if (confirmPwd.length > 0) {
                if (newPwd === confirmPwd) {
                    $(this).removeClass('error').addClass('success');
                    showPasswordMatch(true);
                    $submitBtn.prop('disabled', false);
                } else {
                    $(this).removeClass('success').addClass('error');
                    showPasswordMatch(false);
                    $submitBtn.prop('disabled', true);
                }
            }
        });
    }

    function calculatePasswordStrength(password) {
        var strength = 0;
        
        if (password.length === 0) return 0;
        if (password.length >= 8) strength += 1;
        if (password.length >= 12) strength += 1;
        if (/[a-z]/.test(password)) strength += 1;
        if (/[A-Z]/.test(password)) strength += 1;
        if (/[0-9]/.test(password)) strength += 1;
        if (/[^a-zA-Z0-9]/.test(password)) strength += 1;
        
        return Math.min(strength, 5);
    }

    function updateStrengthIndicator(strength, $indicator) {
        var colors = ['#d9534f', '#d9534f', '#f0ad4e', '#f0ad4e', '#5cb85c', '#5cb85c'];
        var texts = ['Rất yếu', 'Yếu', 'Trung bình', 'Khá tốt', 'Tốt', 'Rất tốt'];
        var widths = ['10%', '25%', '50%', '65%', '85%', '100%'];
        
        $indicator.css({
            'width': widths[strength],
            'background-color': colors[strength],
            'height': '5px',
            'border-radius': '3px',
            'transition': 'all 0.3s ease',
            'margin-top': '5px'
        });
        
        var $label = $indicator.next('.strength-label');
        if ($label.length === 0) {
            $indicator.after('<small class="strength-label" style="color: ' + colors[strength] + ';">' + texts[strength] + '</small>');
        } else {
            $label.text(texts[strength]).css('color', colors[strength]);
        }
    }

    function showPasswordMatch(isMatch) {
        var $message = $('#password-match-message');
        if ($message.length === 0) {
            $('#confirmPassword').after('<span id="password-match-message" class="help-block"></span>');
            $message = $('#password-match-message');
        }
        
        if (isMatch) {
            $message.html('<span class="text-success"><i class="fa fa-check"></i> Mật khẩu khớp</span>');
        } else {
            $message.html('<span class="text-danger"><i class="fa fa-times"></i> Mật khẩu không khớp</span>');
        }
    }

    // ==================== PAYMENT TOGGLES ====================
    function initPaymentToggles() {
        $('.enable-toggle').each(function () {
            var $toggle = $(this);
            var target = '.' + $toggle.data('target');
            
            togglePaymentSection($toggle, target);
        });

        $('.enable-toggle').on('change', function () {
            var $toggle = $(this);
            var target = '.' + $toggle.data('target');
            togglePaymentSection($toggle, target);
            
            var method = $toggle.closest('.panel').find('.panel-title').text().trim();
            showToast(method + ' đã được ' + ($toggle.is(':checked') ? 'bật' : 'tắt'), 'info');
        });
    }

    function togglePaymentSection($toggle, target) {
        var $target = $(target);
        
        if ($toggle.is(':checked')) {
            $target.find('input, select, textarea').prop('disabled', false);
            $target.css('opacity', '1');
            $target.removeClass('disabled');
        } else {
            $target.find('input, select, textarea').prop('disabled', true);
            $target.css('opacity', '0.5');
            $target.addClass('disabled');
        }
    }

    // ==================== BACKUP FEATURES ====================
    function initBackupFeatures() {
        // Select all checkbox
        $('#selectAll').on('change', function () {
            $('.backup-checkbox').prop('checked', $(this).is(':checked'));
            updateBulkActions();
        });

        $('.backup-checkbox').on('change', updateBulkActions);

        // Initialize DataTable if available
        if ($.fn.DataTable && $('#backupTable').length) {
            $('#backupTable').DataTable({
                "pageLength": 10,
                "order": [[1, "desc"]],
                "language": {
                    "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Vietnamese.json"
                },
                "columnDefs": [
                    { "orderable": false, "targets": [0, 4] }
                ]
            });
        }
    }

    function updateBulkActions() {
        var checkedCount = $('.backup-checkbox:checked').length;
        var $deleteBtn = $('#deleteSelectedBtn');
        
        $deleteBtn.prop('disabled', checkedCount === 0);
        
        if (checkedCount > 0) {
            $deleteBtn.html('<i class="fa fa-trash"></i> Xóa ' + checkedCount + ' mục');
        } else {
            $deleteBtn.html('<i class="fa fa-trash"></i> Xóa các mục đã chọn');
        }
    }

    // ==================== BACKUP OPERATIONS ====================
    function createBackup() {
        if (_backupInProgress) {
            showToast('Backup đang được thực hiện, vui lòng đợi...', 'warning');
            return;
        }

        if (!confirm('Bạn có chắc muốn tạo bản sao lưu mới?')) return;

        _backupInProgress = true;
        showLoading('Đang tạo bản sao lưu...');

        $.ajax({
            url: '/Adm/Settings/CreateBackup',
            type: 'POST',
            data: {
                __RequestVerificationToken: _csrfToken
            },
            success: function (response) {
                hideLoading();
                _backupInProgress = false;
                
                if (response.success) {
                    showToast(response.message, 'success');
                    setTimeout(function () {
                        location.reload();
                    }, 1500);
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function (xhr, status, error) {
                hideLoading();
                _backupInProgress = false;
                showToast('Có lỗi xảy ra: ' + error, 'error');
            }
        });
    }

    function restoreBackup(fileName) {
        if (!confirm('⚠️ CẢNH BÁO: Khôi phục dữ liệu sẽ ghi đè lên dữ liệu hiện tại!\n\nBạn có chắc muốn khôi phục từ bản sao lưu "' + fileName + '"?')) {
            return;
        }

        if (!confirm('Lần xác nhận cuối cùng! Bạn có chắc chắn không?')) {
            return;
        }

        showLoading('Đang khôi phục dữ liệu...');

        $.ajax({
            url: '/Adm/Settings/RestoreBackup',
            type: 'POST',
            data: {
                fileName: fileName,
                __RequestVerificationToken: _csrfToken
            },
            success: function (response) {
                hideLoading();
                
                if (response.success) {
                    showToast(response.message, 'success');
                    setTimeout(function () {
                        location.reload();
                    }, 2000);
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function () {
                hideLoading();
                showToast('Có lỗi xảy ra khi khôi phục dữ liệu!', 'error');
            }
        });
    }

    function deleteBackup(fileName) {
        if (!confirm('Bạn có chắc muốn xóa bản sao lưu "' + fileName + '"?')) return;

        $.ajax({
            url: '/Adm/Settings/DeleteBackup',
            type: 'POST',
            data: {
                fileName: fileName,
                __RequestVerificationToken: _csrfToken
            },
            success: function (response) {
                if (response.success) {
                    showToast(response.message, 'success');
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function () {
                showToast('Có lỗi xảy ra khi xóa bản sao lưu!', 'error');
            }
        });
    }

    function deleteSelectedBackups() {
        var selected = [];
        $('.backup-checkbox:checked').each(function () {
            selected.push($(this).val());
        });

        if (selected.length === 0) return;

        if (!confirm('Bạn có chắc muốn xóa ' + selected.length + ' bản sao lưu đã chọn?')) return;

        showLoading('Đang xóa ' + selected.length + ' bản sao lưu...');

        var deleted = 0;
        var errors = 0;

        selected.forEach(function (fileName) {
            $.ajax({
                url: '/Adm/Settings/DeleteBackup',
                type: 'POST',
                data: {
                    fileName: fileName,
                    __RequestVerificationToken: _csrfToken
                },
                success: function (response) {
                    deleted++;
                    if (!response.success) errors++;
                    
                    if (deleted === selected.length) {
                        hideLoading();
                        if (errors === 0) {
                            showToast('Đã xóa ' + deleted + ' bản sao lưu', 'success');
                        } else {
                            showToast('Đã xóa ' + (deleted - errors) + ' bản sao lưu, ' + errors + ' lỗi', 'warning');
                        }
                        setTimeout(function () {
                            location.reload();
                        }, 1500);
                    }
                },
                error: function () {
                    errors++;
                    deleted++;
                    
                    if (deleted === selected.length) {
                        hideLoading();
                        showToast('Đã xóa ' + (deleted - errors) + ' bản sao lưu, ' + errors + ' lỗi', 'warning');
                        setTimeout(function () {
                            location.reload();
                        }, 1500);
                    }
                }
            });
        });
    }

    function downloadBackup(fileName) {
        window.location.href = '/App_Data/Backups/' + fileName;
        showToast('Đang tải xuống file...', 'info');
    }

    function viewBackupInfo(fileName) {
        var content = `
            <div class="row">
                <div class="col-md-6">
                    <h4><i class="fa fa-file"></i> Thông tin file</h4>
                    <table class="table table-bordered">
                        <tr>
                            <th width="40%">Tên file:</th>
                            <td>${fileName}</td>
                        </tr>
                        <tr>
                            <th>Đường dẫn:</th>
                            <td><code>/App_Data/Backups/${fileName}</code></td>
                        </tr>
                        <tr>
                            <th>Format:</th>
                            <td>JSON</td>
                        </tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h4><i class="fa fa-database"></i> Nội dung sao lưu</h4>
                    <ul class="list-group">
                        <li class="list-group-item"><i class="fa fa-check text-success"></i> Danh mục sản phẩm</li>
                        <li class="list-group-item"><i class="fa fa-check text-success"></i> Sản phẩm</li>
                        <li class="list-group-item"><i class="fa fa-check text-success"></i> Đơn hàng</li>
                        <li class="list-group-item"><i class="fa fa-check text-success"></i> Người dùng</li>
                        <li class="list-group-item"><i class="fa fa-check text-success"></i> Cài đặt hệ thống</li>
                    </ul>
                </div>
            </div>
        `;
        
        $('#backupInfoContent').html(content);
        $('#backupInfoModal').modal('show');
    }

    // ==================== FORM VALIDATION ====================
    function initFormValidation() {
        $('form').on('submit', function (e) {
            var $form = $(this);
            var isValid = true;

            // Validate required fields
            $form.find('[required]').each(function () {
                if ($(this).val().trim() === '') {
                    $(this).addClass('error');
                    isValid = false;
                } else {
                    $(this).removeClass('error');
                }
            });

            // Validate email
            $form.find('[type="email"]').each(function () {
                var email = $(this).val();
                if (email && !isValidEmail(email)) {
                    $(this).addClass('error');
                    showToast('Email không hợp lệ', 'error');
                    isValid = false;
                }
            });

            if (!isValid) {
                e.preventDefault();
                showToast('Vui lòng điền đầy đủ thông tin!', 'error');
            }
        });
    }

    function isValidEmail(email) {
        var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    // ==================== UI HELPERS ====================
    function showLoading(message) {
        var html = `
            <div id="loading-overlay" style="
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.7);
                z-index: 9999;
                display: flex;
                align-items: center;
                justify-content: center;
            ">
                <div style="text-align: center; color: white;">
                    <i class="fa fa-spinner fa-spin fa-5x"></i>
                    <h3 style="margin-top: 20px;">${message}</h3>
                </div>
            </div>
        `;
        $('body').append(html);
    }

    function hideLoading() {
        $('#loading-overlay').remove();
    }

    function showToast(message, type) {
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
        } else {
            alert(message);
        }
    }

    // ==================== PUBLIC API ====================
    return {
        init: init,
        createBackup: createBackup,
        restoreBackup: restoreBackup,
        deleteBackup: deleteBackup,
        deleteSelectedBackups: deleteSelectedBackups,
        downloadBackup: downloadBackup,
        viewBackupInfo: viewBackupInfo,
        applyTheme: applyTheme
    };
})();

// Initialize when document is ready
$(document).ready(function () {
    SettingsModule.init();
});

// Expose to global scope for inline onclick handlers
window.createBackup = SettingsModule.createBackup;
window.restoreBackup = SettingsModule.restoreBackup;
window.deleteBackup = SettingsModule.deleteBackup;
window.deleteSelectedBackups = SettingsModule.deleteSelectedBackups;
window.downloadBackup = SettingsModule.downloadBackup;
window.viewBackupInfo = SettingsModule.viewBackupInfo;
