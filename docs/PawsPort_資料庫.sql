-- MSSQL 資料表建立腳本
-- 遊戲系統
-- 1. PlayerProfile 資料表
CREATE TABLE PlayerProfile (
    PlayerId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    --FOREIGN KEY (UserId) REFERENCES User(UserId),
    Point INT NULL
);

-- 2. GameContent 資料表
CREATE TABLE GameContent (
    GameId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    GameName NVARCHAR(10) NOT NULL,
    Questions NVARCHAR(50) NOT NULL,
    AnswersDetail NVARCHAR(300) NOT NULL,
    Answers BIT NOT NULL,
    IsActive BIT NOT NULL,
    Rewards INT NOT NULL, -- 關卡獎勵
    Type NVARCHAR(50) NULL -- 假設為 NVARCHAR(50)，若有具體長度請調整
);

-- 3. SkinShop 資料表
CREATE TABLE SkinShop (
    SkinId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    SkinName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(50) NOT NULL,
    Price INT NOT NULL,
    SkinImage VARCHAR(100) NOT NULL,
    IsAvailable BIT NOT NULL,
    IsDEL BIT NULL
);

-- 4. Inventory 資料表
CREATE TABLE Inventory (
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,
    PlayerId INT NOT NULL,
    SkinId INT NOT NULL,
    Enable BIT NOT NULL,
    CreateTime DATETIME NULL,
    --FOREIGN KEY (PlayerId) REFERENCES PlayerProfile(PlayerId),
    --FOREIGN KEY (SkinId) REFERENCES SkinShop(SkinId)
);

-- 5. GameHistory 資料表
CREATE TABLE GameHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    GameId INT NOT NULL,
    StageClear BIT NULL, -- 是否通關狀態，假設為 BIT
    LastPlayedDate DATETIME NULL, -- 最後遊玩日期
    PlayerId INT NOT NULL,
    --FOREIGN KEY (GameId) REFERENCES GameContent(GameId),
    --FOREIGN KEY (PlayerId) REFERENCES PlayerProfile(PlayerId)
);

-- 6. PointRecord 資料表
CREATE TABLE PointRecord (
    PointDetailId INT IDENTITY(1,1) PRIMARY KEY,
    Point INT NULL,
    Upstream NVARCHAR(50) NULL, -- 假設為 NVARCHAR(50)，若有具體 enum 值請調整為 CHECK 約束
    CreateTime DATETIME NULL,
    PlayerId INT NOT NULL,
    GameId INT NULL, -- 根據 ER 圖，GameId 似乎不是 NOT NULL
    --FOREIGN KEY (PlayerId) REFERENCES PlayerProfile(PlayerId),
    --FOREIGN KEY (GameId) REFERENCES GameContent(GameId)
);

-- 7. InventoryLog 資料表
CREATE TABLE InventoryLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    PlayerId INT NOT NULL,
    SkinId INT NULL, -- 根據 ER 圖，SkinID 似乎不是 NOT NULL
    CreateTime DATETIME NULL,
    Price INT NOT NULL,
    Point INT NOT NULL,
    --FOREIGN KEY (PlayerId) REFERENCES PlayerProfile(PlayerId),
    --FOREIGN KEY (SkinId) REFERENCES SkinShop(SkinId)
);

-- 客服系統
-- 1. FAQ 資料表
CREATE TABLE [FAQ] (
    [FAQId] INT IDENTITY(1,1) PRIMARY KEY,
    [QuestionType] NVARCHAR(20) NOT NULL,
    [Question] NVARCHAR(50) NOT NULL,
    [Answer] NVARCHAR(500) NOT NULL,
    [Note] NVARCHAR(100),
    [CreateAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [StoppedDate] DATETIME NULL,           -- 修正：初期可能沒停用日期，建議允許 NULL
    [Status] NVARCHAR(20) NOT NULL,        -- 發送狀態
    [IsExist] BIT NOT NULL DEFAULT 1       -- 布林值使用 BIT
);

-- 2. e-newsletter 資料表
CREATE TABLE [e-newsletter] (
    [NewsLetterId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(50) NOT NULL,
    [Summary] NVARCHAR(50) NOT NULL,
    [Content] NVARCHAR(500) NOT NULL,
    [PublishDate] DATETIME NULL,           -- 建議允許 NULL，發佈時再填入
    [Category] NVARCHAR(20),
    [Status] NVARCHAR(100),
    [Note] NVARCHAR(100),
    [UserId] INT NOT NULL
);

-- 3. LINE Bot(非必要) 資料表
CREATE TABLE [LINE Bot] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [ChatDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [QuestionType] NVARCHAR(20) NOT NULL,
    [ChiefComplaint] NVARCHAR(50) NOT NULL,
    [ChatContent] NVARCHAR(100) NOT NULL
);

-- 4. Q&A Record 資料表
CREATE TABLE [Q&A Record] (
    [QAId] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,                 -- 會員 ID
    [QuestionDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [QuestionType] NVARCHAR(20) NOT NULL,
    [ChiefComplaint] NVARCHAR(50) NOT NULL,
    [ChatContent] NVARCHAR(2000) NOT NULL,
    [CSName] NVARCHAR(20) NOT NULL,        -- 客服姓名
    [ReplyContent] NVARCHAR(100) NOT NULL,
    [Note] NVARCHAR(100),
    [ReplyDate] DATETIME NOT NULL,
    [Score] INT NULL                       -- 評分初期可能為空，建議允許 NULL
);

-- ==========================================
-- 1. 會員系統 (Core System)
-- ==========================================

-- 1.1 基礎使用者表 (UserTable)
CREATE TABLE [UserTable] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(20) NOT NULL,
    Photo NVARCHAR(MAX) NULL, -- base64
    Job NVARCHAR(20),
    Phone NVARCHAR(10),
    Birthday DATE,
    City NVARCHAR(10),
    Note NVARCHAR(50) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    DeleteDay DATETIME NULL,
    Status BIT NOT NULL DEFAULT 1,
    HasPriorExp BIT DEFAULT 0,
    Point INT DEFAULT 0,
    IsSubscribe BIT DEFAULT 1,
    IsVerify BIT DEFAULT 0
);
GO

-- 1.2 傳統帳號密碼認證 (UserAuth Table)
CREATE TABLE [UserAuthTable] (
    AuthId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    PasswordResetExpires DATETIME NULL,
    PasswordResetToken NVARCHAR(255) NULL,
    UserId INT NOT NULL UNIQUE,
    -- CONSTRAINT FK_UserAuth_User FOREIGN KEY (UserId) REFERENCES [UserTable](UserId) ON DELETE CASCADE
);
GO

-- 1.3 第三方 OAuth 認證 (OAuth Table)
CREATE TABLE [OAuthTable] (
    OauthId INT IDENTITY(1,1) PRIMARY KEY,
    AuthType NVARCHAR(20) NOT NULL, -- Google, FB, Apple...
    ProviderKey NVARCHAR(255) NOT NULL,
    RefreshToken NVARCHAR(255) NULL,
    AccessToken NVARCHAR(255) NULL,
    UserId INT NOT NULL,
    -- CONSTRAINT FK_OAuth_User FOREIGN KEY (UserId) REFERENCES [UserTable](UserId) ON DELETE CASCADE,
    -- CONSTRAINT UC_OAuth_Provider UNIQUE (AuthType, ProviderKey)
);
GO

-- 1.4 系統與角色定義
CREATE TABLE [SystemTable] (
    SystemId INT IDENTITY(1,1) PRIMARY KEY,
    SystemName NVARCHAR(20) NOT NULL,
    Description NVARCHAR(100),
    IsActive BIT DEFAULT 1
);
GO

CREATE TABLE [RoleTable] (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL
);
GO

-- 1.5 使用者系統角色對應表
CREATE TABLE [UserSystemRole] (
    MappingID INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    SystemId INT NOT NULL,
    RoleId INT NOT NULL,
    UpdatedAt DATETIME DEFAULT GETDATE(),
    -- CONSTRAINT FK_Mapping_User FOREIGN KEY (UserId) REFERENCES [UserTable](UserId),
    -- CONSTRAINT FK_Mapping_System FOREIGN KEY (SystemId) REFERENCES [SystemTable](SystemId),
    -- CONSTRAINT FK_Mapping_Role FOREIGN KEY (RoleId) REFERENCES [RoleTable](RoleId)
);
GO

-- 1.6 登入活動記錄
CREATE TABLE [LoginActivityTable] (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    LoginTime DATETIME DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    DeviceInfo NVARCHAR(MAX),
    AuthType NVARCHAR(20),
    Status BIT,
    Country NVARCHAR(50),
    City NVARCHAR(50),
    Latitude DECIMAL(9,6),
    Longitude DECIMAL(9,6),
    UserId INT,
    -- CONSTRAINT FK_Login_User FOREIGN KEY (UserId) REFERENCES [UserTable](UserId)
);
GO
-- 寵物系統
CREATE TABLE Pets (
    PetId INT IDENTITY(1,1) PRIMARY KEY,
    SpeciesId INT,      -- 原 ERD 為 Enum
    Name NVARCHAR(50),
    Gender INT,          -- 原 ERD 為 Enum
    Size INT,            -- 原 ERD 為 Enum
    CoatColor NVARCHAR(20),
    BirthDate DATE,
    Photo NVARCHAR(100) NULL,
    CurrentStatus INT,  -- 原 ERD 為 Enum
    BehavioralTraits NVARCHAR(50),
    IsHighMaintenance BIT,      -- 原 ERD 為 Boolean
    Note NVARCHAR(50),
    CreatedAt DATETIME,
    IsDesex BIT,                 -- 原 ERD 為 Boolean
    UpdatedAt DATETIME,
    DeletedAt DATETIME NULL       -- 建議未來可修正拼字為 deleted_at
);
GO
CREATE TABLE HealthPassports (
    PassportId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT,
    RecordDate DATE,
    Weight DECIMAL(5,2),          -- 預設體重精度
    Note NVARCHAR(50),
    RecordType INT,     -- 原 ERD 為 enum
    UpdatedAt DATETIME,
    CreatedAt DATETIME,
    --CONSTRAINT FK_HealthPassportsPets FOREIGN KEY (PetId) REFERENCES Pets(PetId)
);
GO
CREATE TABLE MedicalHistory (
    MedicalDetailID INT IDENTITY(1,1) PRIMARY KEY,
    Location NVARCHAR(20),        -- 已修正拼寫 loacation -> location
    Disease NVARCHAR(20),      -- 中文欄位名使用中括號包覆
    DiseaseTreatment NVARCHAR(50),      -- 已修正拼寫 navrchar50 -> NVARCHAR(50)
    Time DATETIME,
    PassportId INT,
    UpdatedAt DATETIME,
    CreatedAt DATETIME,
    --CONSTRAINT FK_MedicalHistoryPassports FOREIGN KEY (PassportId) REFERENCES HealthPassports(PassportId)
);
GO
CREATE TABLE VaccinationStatus (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    Type NVARCHAR(10),
    Location NVARCHAR(20),
    Time DATETIME,
    Forecast DATE,
    PassportId INT,
    UpdatedAt DATETIME,
    CreatedAt DATETIME,
    --CONSTRAINT FK_VaccinationStatusPassports FOREIGN KEY (PassportId) REFERENCES HealthPassports(PassportId)
);
GO
CREATE TABLE MissingReports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT,
    LastSeenDate DATETIME,      -- 原 ERD 為 Timestamp
    IsActive BIT,                -- 原 ERD 為 Boolean
    LastSeenLat DECIMAL(9,6),   -- 預設經緯度精度
    LastSeenLng DECIMAL(9,6),   -- 預設經緯度精度
    LostLocation NVARCHAR(50),  -- ERD 漏掉型態，預設給 NVARCHAR(255)
    UserId INT,                   -- ERD 漏掉型態，預設給 INT
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    --CONSTRAINT FK_MissingReportsPets FOREIGN KEY (PetId) REFERENCES Pets(PetId)
    -- 如果有 Users 表，請解除下方註解：
    -- CONSTRAINT FK_MissingReports_Users FOREIGN KEY (userid) REFERENCES Users(user_id) 
);
GO
CREATE TABLE AdoptionRecords (
    AdoptionId INT IDENTITY(1,1) PRIMARY KEY,
    PetId INT,
    UserId INT,
    ApplyDate DATE,
    AdoptDate DATE,
    ReturnDate DATE,
    ReturnReason NVARCHAR(50),
    FollowUpDeadline DATE,
    Status INT,          -- 原 ERD 為 Enum
    --CONSTRAINT FK_AdoptionRecordsPets FOREIGN KEY (PetId) REFERENCES Pets(PetId)
    -- 如果有 Users 表，請解除下方註解：
    -- CONSTRAINT FK_AdoptionRecords_Users FOREIGN KEY (userid) REFERENCES Users(user_id)
);
GO
-- 社群系統
-- 1. 分類表 (Category)
CREATE TABLE [Category] (
    [CategoryID] INT PRIMARY KEY IDENTITY(1,1),
    [CategoryName] NVARCHAR(200) NOT NULL,
    [CategoryDescription] NVARCHAR(1000) NULL,
    [ParentID] INT NULL, 
    [Level] INT NOT NULL DEFAULT (1),
    [SortOrder] INT NOT NULL DEFAULT (0),
    [IsExist] BIT NOT NULL DEFAULT (1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [LastEditTime] DATETIME NULL,
    --CONSTRAINT [FK_Category_Self] FOREIGN KEY ([ParentID]) REFERENCES [Category]([CategoryID])
);
CREATE UNIQUE NONCLUSTERED INDEX IX_Unique_CategoryName_Per_Parent ON [Category] ([ParentID], [CategoryName]) WHERE [IsExist] = 1;
GO

-- 2. 貼文表 (Article)
CREATE TABLE [Article] (
    [ArticleID] INT PRIMARY KEY IDENTITY(1,1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [LastEditTime] DATETIME NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [Status] INT NOT NULL DEFAULT (0), 
    [ViewCount] INT NOT NULL DEFAULT (0),
    [ReportedCount] INT NOT NULL DEFAULT (0),
    [LastReported] DATETIME NULL,
    [EventStartDate] DATETIME NULL,
    [EventEndDate] DATETIME NULL,
    [EventLocation] NVARCHAR(1000) NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    [UserID] INT NOT NULL, 
    [CategoryID] INT NOT NULL,
    CONSTRAINT [CK_Article_Date] CHECK ([EventEndDate] >= [EventStartDate]),
    CONSTRAINT [CK_Article_Status] CHECK ([Status] BETWEEN 0 AND 3),
    --CONSTRAINT [FK_Article_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Article_Category] FOREIGN KEY ([CategoryID]) REFERENCES [Category]([CategoryID])
);
GO

-- 3. 貼文圖片表 (ArticleImage)
CREATE TABLE [ArticleImage] (
    [ImageID] INT PRIMARY KEY IDENTITY(1,1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [LastEditTime] DATETIME NULL,
    [Image] NVARCHAR(500) NOT NULL,
    [SortOrder] INT NOT NULL DEFAULT (1), 
    [ArticleID] INT NOT NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    --CONSTRAINT [FK_ArticleImage_Article] FOREIGN KEY ([ArticleID]) REFERENCES [Article]([ArticleID])
);
CREATE UNIQUE NONCLUSTERED INDEX IX_Unique_ArticleImage_Sort ON [ArticleImage] ([ArticleID], [SortOrder]) WHERE [IsExist] = 1;
GO

-- 4. 追蹤表 (Following)
CREATE TABLE [Following] (
    [FollowingID_PK] INT PRIMARY KEY IDENTITY(1,1),
    [UserID] INT NOT NULL,
    [FollowingID] INT NOT NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [CK_Follow_Self] CHECK ([UserID] <> [FollowingID]),
    --CONSTRAINT [FK_Follow_User_Source] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Follow_User_Target] FOREIGN KEY ([FollowingID]) REFERENCES [Users]([UserID])
);
CREATE UNIQUE NONCLUSTERED INDEX IX_Unique_Following_Active ON [Following] ([UserID], [FollowingID]) WHERE [IsExist] = 1;
GO

-- 5. 聊天室表 (Chatrooms)
CREATE TABLE [Chatrooms] (
    [ChatroomID] INT PRIMARY KEY IDENTITY(1,1),
    [UserID_1] INT NOT NULL,
    [UserID_2] INT NOT NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [CK_Chatroom_Order] CHECK ([UserID_1] < [UserID_2]),
    --CONSTRAINT [FK_Chatroom_User1] FOREIGN KEY ([UserID_1]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Chatroom_User2] FOREIGN KEY ([UserID_2]) REFERENCES [Users]([UserID])
);
CREATE UNIQUE NONCLUSTERED INDEX IX_Unique_Chatroom_Active ON [Chatrooms] ([UserID_1], [UserID_2]) WHERE [IsExist] = 1;
GO

-- 6. 訊息表 (Messages)
CREATE TABLE [Messages] (
    [MessageID] BIGINT PRIMARY KEY IDENTITY(1,1),
    [Content] NVARCHAR(4000) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT (0),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [Image] NVARCHAR(500) NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    [SenderID] INT NOT NULL,
    [ChatroomID] INT NOT NULL,
    --CONSTRAINT [FK_Messages_Chatroom] FOREIGN KEY ([ChatroomID]) REFERENCES [Chatrooms]([ChatroomID]),
    --CONSTRAINT [FK_Messages_Sender] FOREIGN KEY ([SenderID]) REFERENCES [Users]([UserID])
);
GO

-- 7. 收藏文章表 (Bookmark)
CREATE TABLE [Bookmark] (
    [BookmarkID] INT PRIMARY KEY IDENTITY(1,1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [UserID] INT NOT NULL,
    [ArticleID] INT NOT NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    --CONSTRAINT [FK_Bookmark_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Bookmark_Article] FOREIGN KEY ([ArticleID]) REFERENCES [Article]([ArticleID])
);
CREATE UNIQUE NONCLUSTERED INDEX IX_Unique_Bookmark_Active ON [Bookmark] ([UserID], [ArticleID]) WHERE [IsExist] = 1;
GO

-- 8. 通知表 (Notify)
CREATE TABLE [Notify] (
    [NotifyID] BIGINT PRIMARY KEY IDENTITY(1,1),
    [UserID] INT NOT NULL, 
    [SenderID] INT NOT NULL, 
    [Type] INT NOT NULL, 
    [TargetID] INT NULL, 
    [Content] NVARCHAR(500) NULL,
    [IsRead] BIT NOT NULL DEFAULT (0),
    [IsExist] BIT NOT NULL DEFAULT (1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    --CONSTRAINT [FK_Notify_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Notify_Sender] FOREIGN KEY ([SenderID]) REFERENCES [Users]([UserID]),
    CONSTRAINT [CK_Notify_Type] CHECK ([Type] BETWEEN 0 AND 4)
);
CREATE NONCLUSTERED INDEX IX_Notification_User_Read ON [Notify] ([UserID], [IsRead], [Create_at] DESC) WHERE [IsExist] = 1;
GO

-- 9. 留言表 (Comment)
CREATE TABLE [Comment] (
    [CommentID] INT PRIMARY KEY IDENTITY(1,1),
    [Create_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [LastEditTime] DATETIME NULL,
    [Content] NVARCHAR(4000) NOT NULL,
    [Status] INT NOT NULL DEFAULT (1),
    [Image] NVARCHAR(500) NULL,
    [ReportedCount] INT NOT NULL DEFAULT(0),
    [LastReported] DATETIME NULL,
    [IsExist] BIT NOT NULL DEFAULT (1),
    [UserID] INT NOT NULL, 
    [ArticleID] INT NOT NULL,
    CONSTRAINT [CK_Comment_Status] CHECK ([Status] BETWEEN 0 AND 2),
    --CONSTRAINT [FK_Comment_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID]),
    --CONSTRAINT [FK_Comment_Article] FOREIGN KEY ([ArticleID]) REFERENCES [Article]([ArticleID])
);
GO


USE [PetDB]
GO
/****** Object:  Table [dbo].[E-Newsletter]    Script Date: 2026/3/28 下午 08:02:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[E-Newsletter](
	[NewsLetterId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Summary] [nvarchar](50) NOT NULL,
	[Content] [nvarchar](500) NOT NULL,
	[PublishDate] [datetime] NULL,
	[Category] [nvarchar](20) NULL,
	[Status] [nvarchar](100) NULL,
	[Note] [nvarchar](100) NULL,
	[UserId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NewsLetterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FAQ]    Script Date: 2026/3/28 下午 08:02:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FAQ](
	[FAQId] [int] IDENTITY(1,1) NOT NULL,
	[QuestionType] [nvarchar](20) NOT NULL,
	[Question] [nvarchar](50) NOT NULL,
	[Answer] [nvarchar](500) NOT NULL,
	[Note] [nvarchar](100) NULL,
	[CreateAt] [datetime] NOT NULL,
	[StoppedDate] [datetime] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[IsExist] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FAQId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LINE Bot]    Script Date: 2026/3/28 下午 08:02:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LINE Bot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ChatDate] [datetime] NOT NULL,
	[QuestionType] [nvarchar](20) NOT NULL,
	[ChiefComplaint] [nvarchar](50) NOT NULL,
	[ChatContent] [nvarchar](1000) NOT NULL,
 CONSTRAINT [PK__LINE Bot__3214EC0789C6A1C6] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Q&A Record]    Script Date: 2026/3/28 下午 08:02:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Q&A Record](
	[QAId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[QuestionDate] [datetime] NOT NULL,
	[QuestionType] [nvarchar](20) NOT NULL,
	[ChiefComplaint] [nvarchar](50) NOT NULL,
	[ChatContent] [nvarchar](2000) NOT NULL,
	[CSName] [nvarchar](20) NOT NULL,
	[ReplyContent] [nvarchar](100) NOT NULL,
	[Note] [nvarchar](100) NULL,
	[ReplyDate] [datetime] NOT NULL,
	[Score] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[QAId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[FAQ] ADD  DEFAULT (getdate()) FOR [CreateAt]
GO
ALTER TABLE [dbo].[FAQ] ADD  DEFAULT ((1)) FOR [IsExist]
GO
ALTER TABLE [dbo].[LINE Bot] ADD  CONSTRAINT [DF__LINE Bot__ChatDa__5070F446]  DEFAULT (getdate()) FOR [ChatDate]
GO
ALTER TABLE [dbo].[Q&A Record] ADD  DEFAULT (getdate()) FOR [QuestionDate]
GO
