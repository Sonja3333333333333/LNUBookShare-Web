document.addEventListener("DOMContentLoaded", function () {
    const chatWindow = document.getElementById("chatWindow");
    const chatForm = document.getElementById("chatForm");
    const messageInput = document.getElementById("messageContent");
    const statusBadge = document.getElementById("statusBadge");

    const chatContainer = document.querySelector(".chat-container");

    if (!chatContainer) return;

    const currentUserId = chatContainer.dataset.currentUserId;
    const selectedUserId = chatContainer.dataset.selectedUserId;

    const scrollToBottom = () => {
        if (chatWindow) chatWindow.scrollTop = chatWindow.scrollHeight;
    };
    scrollToBottom();

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.on("ReceiveMessage", function (senderId, content, time) {
        const isMine = senderId == currentUserId;
        const interlocutorId = isMine ? selectedUserId : senderId;
        if (senderId == selectedUserId || senderId == currentUserId) {
            const messageHtml = `
                    <div class="message-bubble ${isMine ? 'message-sent' : 'message-received'}">
                        ${content}
                        <span class="message-time">${time}</span>
                    </div>`;
            chatWindow.insertAdjacentHTML('beforeend', messageHtml);
            scrollToBottom();
        }

        const sidebarItem = document.querySelector(`.conversation-item[data-user-id="${interlocutorId}"]`);

        if (sidebarItem) {
            const lastMsgEl = sidebarItem.querySelector('p.text-muted');
            const timeEl = sidebarItem.querySelector('small.text-muted');

            if (lastMsgEl) lastMsgEl.innerText = content;
            if (timeEl) timeEl.innerText = time;

            const list = document.querySelector('.conversation-list');
            if (list) list.prepend(sidebarItem);
        } else {
            window.location.reload();
        }
    });

    connection.on("UserStatusChanged", function (userId, isOnline) {
        if (userId == selectedUserId) {
            updateStatusUI(isOnline);
        }
    });

    function updateStatusUI(isOnline) {
        if (!statusBadge) return;
        statusBadge.innerHTML = isOnline
            ? '<i class="bi bi-circle-fill" style="font-size: 0.5rem;"></i> онлайн'
            : '<i class="bi bi-circle-fill" style="font-size: 0.5rem;"></i> офлайн';
        statusBadge.className = isOnline ? "text-success" : "text-muted";
    }

    connection.start().then(() => {
        if (currentUserId) connection.invoke("JoinPrivateChat", currentUserId);
        if (selectedUserId) {
            connection.invoke("IsUserOnline", Number.parseInt(selectedUserId, 10)).then(updateStatusUI);
        }
    }).catch(function (err) {
        return console.error(err.toString());
    });

    if (chatForm) {
        chatForm.addEventListener("submit", function (e) {
            e.preventDefault();
            const content = messageInput.value.trim();
            if (!content) return;

            const formData = new FormData(chatForm);
            fetch(chatForm.action, {
                method: "POST",
                body: formData,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            }).then(response => {
                if (response.ok) {
                    messageInput.value = "";
                } else {
                    alert("Помилка відправки");
                }
            });
        });
    }

    let userToDelete = null;
    const deleteModalEl = document.getElementById('deleteChatModal');
    let deleteModal = null;
    if (deleteModalEl) {
        deleteModal = new bootstrap.Modal(deleteModalEl);
    }
    const confirmBtn = document.getElementById('confirmDeleteBtn');

    window.confirmDeleteFromSidebar = function (e, receiverId) {
        e.preventDefault();
        e.stopPropagation();
        userToDelete = receiverId;
        if (deleteModal) deleteModal.show();
    };

    if (confirmBtn) {
        confirmBtn.addEventListener('click', function () {
            if (!userToDelete) return;

            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenInput ? tokenInput.value : '';

            fetch(`/Chat/DeleteConversation?receiverId=${userToDelete}`, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': token
                }
            }).then(response => {
                if (response.ok) {
                    if (deleteModal) deleteModal.hide();

                    const sidebarItem = document.querySelector(`.conversation-item[data-user-id="${userToDelete}"]`);
                    if (sidebarItem) {
                        const wrapper = sidebarItem.closest('.conversation-wrapper');
                        if (wrapper) wrapper.remove();
                    }

                    if (window.location.search.includes(`receiverId=${userToDelete}`) || selectedUserId == userToDelete) {
                        window.location.href = '/Chat/Index';
                    }
                } else {
                    alert("Помилка при видаленні");
                }
            }).catch(err => console.error("Error:", err));
        });
    }
});