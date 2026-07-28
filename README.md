# Petmily 寵物社群平台

![GitHub last commit](https://img.shields.io/github/last-commit/Xiang511/Petmily?display_timestamp=committer&style=flat-square) ![GitHub commit activity](https://img.shields.io/github/commit-activity/y/Xiang511/Petmily?style=flat-square) ![GitHub Created At](https://img.shields.io/github/created-at/Xiang511/Petmily?style=flat-square) ![GitHub License](https://img.shields.io/github/license/Xiang511/Petmily?style=flat-square)

Petmily 是一個全功能寵物社群與領養平台，提供寵物管理、社群互動、遊戲化系統及客服支援功能。


## Tech stack

| 層級 | 技術 |
|------|------|
| **後端框架** | ASP.NET Core 10.0 |
| **ORM** | Entity Framework Core 10.0 |
| **資料庫** | SQL Server |
| **即時通訊** | SignalR |
| **身份驗證** | JWT Bearer Token + Google OAuth 2.0 |
| **AI 客服** | Google Gemini API |
| **郵件服務** | MailKit / MimeKit |
| **日誌系統** | Serilog（File + Seq） |
| **API 文件** | OpenAPI / Scalar |
| **第三方整合** | LINE Bot、IpStack |

## Project Structure

```
PawsPort/
├── Controllers/        # API 控制器（21 個）
├── Services/           # 業務邏輯層（25 個服務）
├── Models/             # 資料模型（40+ 個）
├── Dtos/               # 資料傳輸物件（80+ 個）
├── Authorization/      # 權限與角色管理
├── Hubs/               # SignalR Hub（AI 聊天）
├── Middlewares/        # 例外處理中介層
├── Helpers/            # 圖片上傳工具
├── Enums/              # 系統與角色枚舉
├── Responses/          # 標準 API 回應格式
├── wwwroot/            # 靜態檔案與圖片儲存
├── logs/               # 應用程式日誌
└── Program.cs          # 應用程式啟動設定
```

## Roles & Permissions

系統採用 **5 系統 × 4 角色** 共 20 種授權政策：

| 角色 | 說明 |
|------|------|
| 系統管理員 | 最高管理權限 |
| 普通管理員 | 一般管理功能 |
| 一般成員 | 標準用戶功能 |
| 客服成員 | 客服專屬功能 |


## API Docs

啟動後可透過以下路徑瀏覽 API 文件：

| 類型 | 路徑 |
|------|------|
| Scalar UI | `https://localhost:<port>/scalar/v1` |
| OpenAPI JSON | `https://localhost:<port>/openapi/v1.json` |



## CI/CD

| Workflow | 說明 |
|----------|------|
| `Naming-Validation.yml` | 檔案命名規範驗證（Controller / Service / DTO / Middleware） |
| `Validate-Controller-Standards.yml` | 程式碼標準檢查（繼承、async、DI、回應格式） |
| `Nuclei Code Scan.yml` | 安全性掃描 |

## Contributors

<a href="https://github.com/xiang511/petmily-vue/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=xiang511/petmily-vue" />
</a>    


## Website

[前端專案連結](https://github.com/Xiang511/petmily-vue)


## License

本專案採用 AGPL-3.0 授權，詳見 [LICENSE](LICENSE) 文件。