document.addEventListener("DOMContentLoaded", function () {

    // --- ПРЕВ'Ю АВАТАРКИ ---
    const avatarInput = document.getElementById('AvatarFile');
    if (avatarInput) {
        avatarInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (e) {
                let preview = document.getElementById('avatarPreview');
                const placeholder = document.getElementById('avatarPlaceholder');

                if (!preview) {
                    preview = document.createElement('img');
                    preview.id = 'avatarPreview';
                    preview.className = 'rounded-circle border shadow-sm';
                    preview.style.width = '120px';
                    preview.style.height = '120px';
                    preview.style.objectFit = 'cover';

                    if (placeholder) {
                        placeholder.parentNode.replaceChild(preview, placeholder);
                    }
                }
                preview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        });
    }

    // --- ЗАПОВНЕННЯ МОДАЛКИ РЕДАГУВАННЯ КНИГИ ---
    const editBookButtons = document.querySelectorAll('.edit-book-btn');
    if (editBookButtons.length > 0) {
        editBookButtons.forEach(button => {
            button.addEventListener('click', function () {
                const id = this.dataset.id;

                // Заповнюємо інпути даними з data-атрибутів кнопки
                document.getElementById('edit-id').value = id;
                document.getElementById('edit-title').value = this.dataset.title || '';
                document.getElementById('edit-author').value = this.dataset.author || '';
                document.getElementById('edit-publisher').value = this.dataset.publisher || '';
                document.getElementById('edit-isbn').value = this.dataset.isbn || '';
                document.getElementById('edit-year').value = this.dataset.year || '';
                document.getElementById('edit-language').value = this.dataset.language || '';

                // Заповнюємо селект категорії (він автоматично вибере потрібний <option>)
                const categorySelect = document.getElementById('edit-category');
                if (categorySelect && this.dataset.category) {
                    categorySelect.value = this.dataset.category;
                }

                // Змінюємо action форми
                const editForm = document.getElementById('editBookForm');
                if (editForm) {
                    editForm.action = '/Profile/EditBook/' + id;
                }
            });
        });
    }

    // --- ПІДГОТОВКА МОДАЛКИ ВИДАЛЕННЯ ---
    const deleteBookButtons = document.querySelectorAll('.delete-book-btn');
    if (deleteBookButtons.length > 0) {
        deleteBookButtons.forEach(button => {
            button.addEventListener('click', function () {
                const id = this.dataset.id;

                // Підставляємо ID в прихований інпут
                document.getElementById('delete-id').value = id;

                // Змінюємо action форми
                const deleteForm = document.getElementById('deleteBookForm');
                if (deleteForm) {
                    deleteForm.action = '/Profile/DeleteBook/' + id;
                }
            });
        });
    }
});