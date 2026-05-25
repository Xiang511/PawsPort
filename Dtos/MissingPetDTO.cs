using System;

namespace PawsPort.Dtos
{
    // 供 MissingPetView.vue 列表頁使用
    public class MissingPetListDTO
    {
        public int Id { get; set; } // 對應 ReportId
        public string Breed { get; set; } // 前端名為 breed，對應 Name
        public string Gender { get; set; }
        public string City { get; set; } // 對應 LostCity
        public string District { get; set; } // 對應 LostDistrict
        public string LostTime { get; set; } // 對應 LastSeenDate
        public string LostPlace { get; set; } // 對應 LostLocation
        public string ChipId { get; set; } // 解析自 Note
        public string Feature { get; set; } // 對應 Features
        public string Photo { get; set; }
        public string Color { get; set; } // 解析自 Note
    }

    // 供 MissingPetDetailView.vue 詳細頁使用
    public class MissingPetDetailDTO : MissingPetListDTO
    {
        public string Species { get; set; } // 前端 ANIM，對應 Species
        public string ReporterName { get; set; } // 前端 FDRNAME
        public string ContactPhone { get; set; } // 前端 L_HTEL
        public string ContactEmail { get; set; } // 前端 L_EMAIL
        public string AddDate { get; set; } // 前端 AddDate，對應 CreatedAt
    }

    // 供 PostMissingPetView.vue 刊登使用
    public class CreateMissingPetDTO
    {
        public int UserId { get; set; } // 必填：前端傳遞的發布者 UserId
        public string PetName { get; set; } // 必填：寵物名稱 (對應 Name)
        public string PetType { get; set; } // 必填：動物類別 (對應 Species)
        public string Gender { get; set; } // 選填：性別
        public string LostDate { get; set; } // 必填：遺失日期 (yyyy-MM-dd)
        public string City { get; set; } // 必填：縣市
        public string District { get; set; } // 必填：地區
        public string Address { get; set; } // 必填：遺失地點
        public string HasTag { get; set; } // 選填：晶片號碼 (存入 Note)
        public string FurColor { get; set; } // 選填：毛色 (存入 Note)
        public string Features { get; set; } // 選填：特徵描述
        public string ContactPhone { get; set; } // 必填：聯絡電話
        public string ContactEmail { get; set; } // 必填：聯絡信箱
        public string Photo { get; set; } // 選填：照片 (Base64)
    }
}
