// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    // Select all success alerts and treat them as toasts
    const successAlerts = document.querySelectorAll(".alert.alert-success");
    successAlerts.forEach(function (alert) {
        // Automatically hide alert after 3 seconds
        setTimeout(function () {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-20px) scale(0.95)';
            setTimeout(function () {
                if (window.bootstrap && bootstrap.Alert) {
                    const bsAlert = bootstrap.Alert.getInstance(alert) || new bootstrap.Alert(alert);
                    if (bsAlert) {
                        bsAlert.close();
                        return;
                    }
                }
                alert.remove();
            }, 300); // Wait for the transition to complete
        }, 3000);
    });
});
