document.addEventListener("DOMContentLoaded", function () {
    const toastElement = document.getElementById('liveNotificationToast');
    const toastBody = document.getElementById('toastMessageBody');

    // Якщо елемент є на сторінці (юзер залогінений)
    if (toastElement && toastBody) {
        const notificationToast = new bootstrap.Toast(toastElement, { delay: 6000 });

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .build();

        connection.on("ReceiveNotification", function (message) {
            toastBody.innerText = message;
            notificationToast.show();
        });

        connection.start().catch(err => console.error(err.toString()));
    }
});