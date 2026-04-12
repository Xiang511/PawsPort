
    function deleteSkin(skinId, skinName) {
        // 填充刪除確認 Modal
        document.getElementById('DeleteSkinId').value = skinId;
    document.getElementById('DeleteSkinName').textContent = skinName;

    // 打開 Modal
    $('#DeleteBn').modal('show');
    }

    // 文件選擇器顯示文件名和預覽圖片
    function previewImage(event, previewId) {
        const file = event.target.files[0];
    if (file) {
            const reader = new FileReader();
    reader.onload = function(e) {
                const preview = document.getElementById(previewId);
    preview.src = e.target.result;
    preview.style.display = 'block';
            };
    reader.readAsDataURL(file);

    // 更新文件名標籤
    const label = event.target.nextElementSibling;
    label.textContent = file.name;
        }
    }

    // 圖片放大預覽
    function viewImage(imageSrc, imageName) {
        document.getElementById('ViewerImage').src = imageSrc;
    document.getElementById('imageViewerLabel').textContent = imageName + ' - 圖片預覽';
    $('#ImageViewerBn').modal('show');
    }

    // 編輯按鈕點擊事件
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const skinId = this.dataset.id;
            const skinName = this.dataset.name;
            const description = this.dataset.description;
            const price = this.dataset.price;
            const image = this.dataset.image;
            const isAvailable = this.dataset.available;

            // 填充 Modal 欄位
            document.getElementById('editModalLabel').textContent = `編輯造型 - ${skinName}`;
            document.getElementById('EditSkinId').value = skinId;
            document.getElementById('EditSkinName').value = skinName;
            document.getElementById('EditDescription').value = description;
            document.getElementById('EditPrice').value = price;
            document.getElementById('EditIsAvailable').value = isAvailable === 'True' ? 'true' : 'false';

            // 顯示目前圖片
            document.getElementById('EditCurrentImage').src = image;

            // 清除新圖片預覽
            document.getElementById('EditImagePreview').src = '';
            document.getElementById('EditImagePreview').style.display = 'none';
            document.getElementById('EditImageFile').value = '';
            document.querySelector('label[for="EditImageFile"]').textContent = '選擇圖片（不選則保持原圖片）...';
        });
    });
