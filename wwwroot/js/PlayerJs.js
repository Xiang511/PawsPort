    // 編輯按鈕點擊事件
    document.addEventListener('click', function(e) {
        if (e.target.closest('.edit-btn')) {
            const btn = e.target.closest('.edit-btn');
    const playerId = btn.dataset.id;

    // 獲取玩家詳細信息
    fetch(`/Player/GetPlayerDetails?playerId=${playerId}`)
                .then(response => response.json())
                .then(data => {
        document.getElementById('EditPlayerIdDisplay').textContent = data.playerId;
    document.getElementById('EditPlayerId').value = data.playerId;
    document.getElementById('EditPoint').value = data.point;
                    document.getElementById('EditGameProgress').value = data.maxGameId > 0 ? `第 ${data.maxGameId} 關` : '未開始';
    document.getElementById('EditLastPlayedDate').value = data.lastPlayedDate;

    // 生成造型列表
    const skinsList = document.getElementById('SkinsList');
    skinsList.innerHTML = '';

    if (data.skins.length === 0) {
        skinsList.innerHTML = '<p style="margin: 0; color: #999;">該玩家沒有持有任何造型</p>';
                    } else {
        data.skins.forEach(skin => {
            const skinItem = document.createElement('div');
            skinItem.style.cssText = 'display: flex; justify-content: space-between; align-items: center; padding: 8px; border-bottom: 1px solid #eee;';
            skinItem.innerHTML = `
            <div style="display: flex; align-items: center; gap: 10px;">
                ${skin.skinImage ? `<img src="${skin.skinImage}" alt="${skin.skinName}" style="width: 50px; height: 50px; border: 1px solid #ddd; border-radius: 4px; object-fit: cover;" />` : '<span style="width: 50px; height: 50px; background-color: #f0f0f0; border: 1px solid #ddd; border-radius: 4px; display: flex; align-items: center; justify-content: center; color: #999;">無圖片</span>'}
                <div>
                    <div><strong>${skin.skinName}</strong></div>
                    <div style="font-size: 12px; color: #666;">造型 ID: ${skin.skinId}</div>
                </div>
            </div>
            <div class="custom-control custom-checkbox">
                <input type="checkbox" class="custom-control-input skin-checkbox" name="EnabledSkinIds" value="${skin.inventoryId}" id="skin_${skin.inventoryId}" ${skin.enable ? 'checked' : ''} />
                <label class="custom-control-label" for="skin_${skin.inventoryId}">
                    啟用
                </label>
            </div>
        `;
            skinsList.appendChild(skinItem);
        });
                    }
                })
                .catch(error => console.error('Error:', error));
        }
    });

    function deletePlayer(playerId) {
        document.getElementById('DeletePlayerId').textContent = playerId;
    document.getElementById('DeletePlayerIdInput').value = playerId;
    $('#DeleteBn').modal('show');
    }

    function viewUserProfile(playerId) {
        $.ajax({
            url: '/Player/GetUserProfile',
            type: 'GET',
            data: { userId: playerId },  // 直接用 playerId 當作 userId
            success: function (response) {
                if (response.success) {
                    const user = response.data;

                    // 填充會員資訊
                    document.getElementById('ProfileUserId').textContent = user.userId;
                    document.getElementById('ProfileName').textContent = user.name || '未設定';
                    document.getElementById('ProfileJob').textContent = user.job || '未設定';
                    document.getElementById('ProfilePhone').textContent = user.phone || '未設定';
                    document.getElementById('ProfileBirthday').textContent = user.birthday ? new Date(user.birthday).toLocaleDateString('zh-TW') : '未設定';
                    document.getElementById('ProfileCity').textContent = user.city || '未設定';
                    // document.getElementById('ProfilePoint').textContent = user.point || 0;
                    document.getElementById('ProfileNote').textContent = user.note || '無';
                    document.getElementById('ProfileHasPriorExp').textContent = user.hasPriorExp ? '是' : '否';
                    document.getElementById('ProfileStatus').textContent = user.status || '未設定';
                    document.getElementById('ProfileIsSubscribe').textContent = user.isSubscribe ? '是' : '否';
                    document.getElementById('ProfileIsVerify').textContent = user.isVerify ? '是' : '否';
                    document.getElementById('ProfileCreatedAt').textContent = user.createdAt ? new Date(user.createdAt).toLocaleString('zh-TW') : '未設定';
                    document.getElementById('ProfileUpdatedAt').textContent = user.updatedAt ? new Date(user.updatedAt).toLocaleString('zh-TW') : '未設定';
                    // document.getElementById('ProfileDeleteDay').textContent = user.deleteDay ? new Date(user.deleteDay).toLocaleDateString('zh-TW') : '未設定';

                    // 設置照片
                    const photoImg = document.getElementById('ProfilePhoto');
                    const photoPlaceholder = document.getElementById('PhotoPlaceholder');

                    if (user.photo) {
                        photoImg.src = user.photo;
                        photoImg.style.display = 'block';
                        photoPlaceholder.style.display = 'none';
                    } else {
                        photoImg.style.display = 'none';
                        photoPlaceholder.style.display = 'block';
                    }

                    // 打開 Modal
                    $('#UserProfileModal').modal('show');
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('無法載入會員資料');
            }
        });
    }