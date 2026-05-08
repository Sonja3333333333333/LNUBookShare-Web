document.addEventListener("DOMContentLoaded", function () {

    const alerts = document.querySelectorAll('.alert-success, .alert-danger');

    alerts.forEach(function (alert) {

        if (!alert.classList.contains('alert-dismissible')) {
            alert.classList.add('alert-dismissible', 'fade', 'show');
        }

        if (!alert.querySelector('.btn-close')) {
            const closeBtn = document.createElement('button');
            closeBtn.type = 'button';
            closeBtn.className = 'btn-close';
            closeBtn.setAttribute('data-bs-dismiss', 'alert');
            closeBtn.setAttribute('aria-label', 'Close');
            alert.appendChild(closeBtn);
        }

        const isError = alert.classList.contains('alert-danger');
        const displayTime = isError ? 7000 : 1000;

        setTimeout(function () {

            alert.style.transition = "opacity 0.5s ease";
            alert.style.opacity = "0";

            setTimeout(function () {
                alert.remove();
            }, 500);

        }, displayTime);
    });

});