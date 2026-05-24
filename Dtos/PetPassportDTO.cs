using System;
using System.Collections.Generic;

namespace PawsPort.Dtos
{
    // === 任務 1：毛孩護照主頁顯示 DTO ===
    public class PetPassportDisplayDto
    {
        public int Id { get; set; } // 對應 PassportId，避開敏感字眼
        public string Name { get; set; } // 來自 Pet
        public string Age { get; set; } // 後端依 BirthDate 計算
        public string Photo { get; set; } // 來自 HealthPassport
        public int? Gender { get; set; } // 來自 Pet
        public bool? IsDesex { get; set; } // 來自 Pet
        public decimal? Weight { get; set; } // 目前護照體重

        public List<MedicalRecordDto> MedicalRecords { get; set; } = new();
        public List<VaccinationDto> Vaccinations { get; set; } = new();
        public List<WeightRecordDto> WeightRecords { get; set; } = new();
    }

    public class MedicalRecordDto
    {
        public string Disease { get; set; }
        public string DiseaseTreatment { get; set; }
        public string Location { get; set; }
        public string Time { get; set; } // 轉格式為 yyyy-MM-dd
    }

    public class VaccinationDto
    {
        public string Type { get; set; }
        public string Location { get; set; }
        public string Time { get; set; } // 轉格式為 yyyy-MM-dd
        public string Forecast { get; set; } // 轉格式為 yyyy-MM-dd
    }

    public class WeightRecordDto
    {
        public int Id { get; set; } // 順序或虛擬識別碼
        public string Date { get; set; }
        public decimal Weight { get; set; }
    }

    // === 任務 2 & 3：新增與編輯通用 DTO ===
    public class PetPassportDetailDto
    {
        public int Id { get; set; } // PassportId
        public string? Name { get; set; } // 唯讀或同步顯示
        public int? Gender { get; set; }
        public DateOnly? BirthDate { get; set; } // yyyy-MM-dd
        public bool IsDesex { get; set; }
        public string? RecordDate { get; set; } // yyyy-MM-dd
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public string? Photo { get; set; }
    }

    public class PetPassportUpsertDto
    {
        public int? PetId { get; set; } // 新增時綁定特定的毛孩
        public string? Name { get; set; } // 寵物姓名
        public DateOnly? BirthDate { get; set; } // 出生日期
        public DateOnly? RecordDate { get; set; } // yyyy-MM-dd
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public string? Photo { get; set; }
        // 允許同步更新 Pet 的部分基本健康狀態
        public int? Gender { get; set; }
        public bool IsDesex { get; set; }
    }
}