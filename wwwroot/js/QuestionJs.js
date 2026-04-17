
    function deleteQuestion(gameId, gameName) {
        // 填充刪除確認 Modal
        document.getElementById('DeleteGameId').value = gameId;
    document.getElementById('DeleteGameName').textContent = gameName;

    // 打開 Modal
    $('#DeleteBn').modal('show');
}
    // 編輯按鈕點擊事件
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const gameId = this.dataset.id;
            const gameName = this.dataset.name;
            const questions = this.dataset.questions;
            const answers = this.dataset.answers;
            const answersDetail = this.dataset.detail;
            const rewards = this.dataset.rewards;
            const type = this.dataset.type;
            const isActive = this.dataset.active;

            // 填充 Modal 欄位
            document.getElementById('EditGameId').value = gameId;
            document.getElementById('EditGameName').value = gameName;
            document.getElementById('EditQuestions').value = questions;
            document.getElementById('EditAnswers').value = (answers === 'True' || answers === true) ? 'true' : 'false';
            document.getElementById('EditAnswersDetail').value = answersDetail;
            document.getElementById('EditRewards').value = rewards;
            document.getElementById('EditType').value = type;
            document.getElementById('EditIsActive').checked = (isActive === 'True');
        });
    });
