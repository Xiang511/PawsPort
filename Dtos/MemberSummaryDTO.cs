namespace PawsPort.Dtos
{

    public class MemberSummaryDTO
    {

        /// 總會員數（未刪除）
        public int MemberCount { get; set; }


        /// 本月新增會員數
        public int MemberMonthSignUp { get; set; }


        /// 已驗證會員數
        public int VerifiedMemberCount { get; set; }


        /// 驗證比例（百分比字串，例如："85.5"）
        public string VerifyPercentage { get; set; }


        /// 訂閱會員數
        public int SubscribedMemberCount { get; set; }
    }
}
