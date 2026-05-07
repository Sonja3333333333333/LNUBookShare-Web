// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.




//spiner
function setupButtonSpinner(formId, btnId) {
    const form = document.getElementById(formId);
    const btn = document.getElementById(btnId);

    if (!form || !btn) return;

    form.addEventListener('submit', function (e) {
        if (typeof $(form).valid === 'function' && !$(form).valid()) {
            return;
        }

        const textSpan = btn.querySelector('.btn-text');
        const spinner = btn.querySelector('.spinner-border');

        btn.disabled = true;
        if (textSpan) textSpan.textContent = 'Обробка...';
        if (spinner) spinner.classList.remove('d-none');
    });
}