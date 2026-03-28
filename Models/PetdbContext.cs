using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Models;

public partial class PetDbContext : DbContext
{
    public PetDbContext()
    {
    }

    public PetDbContext(DbContextOptions<PetDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdoptionRecord> AdoptionRecords { get; set; }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleImage> ArticleImages { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Chatroom> Chatrooms { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<ENewsletter> ENewsletters { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Following> Followings { get; set; }

    public virtual DbSet<GameContent> GameContents { get; set; }

    public virtual DbSet<GameHistory> GameHistories { get; set; }

    public virtual DbSet<HealthPassport> HealthPassports { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryLog> InventoryLogs { get; set; }

    public virtual DbSet<LineBot> LineBots { get; set; }

    public virtual DbSet<LoginActivity> LoginActivities { get; set; }

    public virtual DbSet<MedicalHistory> MedicalHistories { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MissingReport> MissingReports { get; set; }

    public virtual DbSet<Notify> Notifies { get; set; }

    public virtual DbSet<OauthTable> OauthTables { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PlayerProfile> PlayerProfiles { get; set; }

    public virtual DbSet<PointRecord> PointRecords { get; set; }

    public virtual DbSet<QARecord> QARecords { get; set; }

    public virtual DbSet<RoleTable> RoleTables { get; set; }

    public virtual DbSet<SkinShop> SkinShops { get; set; }

    public virtual DbSet<SystemTable> SystemTables { get; set; }

    public virtual DbSet<UserAuthTable> UserAuthTables { get; set; }

    public virtual DbSet<UserSystemRole> UserSystemRoles { get; set; }

    public virtual DbSet<UserTable> UserTables { get; set; }

    public virtual DbSet<VaccinationStatus> VaccinationStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=PetDB;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdoptionRecord>(entity =>
        {
            entity.HasKey(e => e.AdoptionId).HasName("PK__Adoption__38BABF2C94EB29B7");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.ReturnReason).HasMaxLength(1);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("PK__Article__9C6270C88372D714");

            entity.ToTable("Article");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.EventEndDate).HasColumnType("datetime");
            entity.Property(e => e.EventLocation).HasMaxLength(1000);
            entity.Property(e => e.EventStartDate).HasColumnType("datetime");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.LastEditTime).HasColumnType("datetime");
            entity.Property(e => e.LastReported).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ArticleImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ArticleI__7516F4EC03D58C31");

            entity.ToTable("ArticleImage");

            entity.HasIndex(e => new { e.ArticleId, e.SortOrder }, "IX_Unique_ArticleImage_Sort")
                .IsUnique()
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.LastEditTime).HasColumnType("datetime");
            entity.Property(e => e.SortOrder).HasDefaultValue(1);
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.BookmarkId).HasName("PK__Bookmark__541A3A916E4B2579");

            entity.ToTable("Bookmark");

            entity.HasIndex(e => new { e.UserId, e.ArticleId }, "IX_Unique_Bookmark_Active")
                .IsUnique()
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.BookmarkId).HasColumnName("BookmarkID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2BE0F9A212");

            entity.ToTable("Category");

            entity.HasIndex(e => new { e.ParentId, e.CategoryName }, "IX_Unique_CategoryName_Per_Parent")
                .IsUnique()
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryDescription).HasMaxLength(1000);
            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.LastEditTime).HasColumnType("datetime");
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
        });

        modelBuilder.Entity<Chatroom>(entity =>
        {
            entity.HasKey(e => e.ChatroomId).HasName("PK__Chatroom__B83BDF2870C30B0E");

            entity.HasIndex(e => new { e.UserId1, e.UserId2 }, "IX_Unique_Chatroom_Active")
                .IsUnique()
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.ChatroomId).HasColumnName("ChatroomID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.UserId1).HasColumnName("UserID_1");
            entity.Property(e => e.UserId2).HasColumnName("UserID_2");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFAADA18E9A9");

            entity.ToTable("Comment");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.LastEditTime).HasColumnType("datetime");
            entity.Property(e => e.LastReported).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ENewsletter>(entity =>
        {
            entity.HasKey(e => e.NewsLetterId).HasName("PK__E-Newsle__7F740FC2AA96F7DD");

            entity.ToTable("E-Newsletter");

            entity.Property(e => e.Category).HasMaxLength(1);
            entity.Property(e => e.Content).HasMaxLength(1);
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.PublishDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(1);
            entity.Property(e => e.Summary).HasMaxLength(1);
            entity.Property(e => e.Title).HasMaxLength(1);
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Faqid).HasName("PK__FAQ__4B89D182E3EFD9F0");

            entity.ToTable("FAQ");

            entity.Property(e => e.Faqid).HasColumnName("FAQId");
            entity.Property(e => e.Answer).HasMaxLength(1);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.Question).HasMaxLength(1);
            entity.Property(e => e.QuestionType).HasMaxLength(1);
            entity.Property(e => e.Status).HasMaxLength(1);
            entity.Property(e => e.StoppedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Following>(entity =>
        {
            entity.HasKey(e => e.FollowingIdPk).HasName("PK__Followin__A05EF6903774B0B9");

            entity.ToTable("Following");

            entity.HasIndex(e => new { e.UserId, e.FollowingId }, "IX_Unique_Following_Active")
                .IsUnique()
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.FollowingIdPk).HasColumnName("FollowingID_PK");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.FollowingId).HasColumnName("FollowingID");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<GameContent>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__GameCont__2AB897FD1E1F2433");

            entity.ToTable("GameContent");

            entity.Property(e => e.AnswersDetail).HasMaxLength(300);
            entity.Property(e => e.GameName).HasMaxLength(10);
            entity.Property(e => e.Questions).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<GameHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__GameHist__4D7B4ABD9CDEC84D");

            entity.ToTable("GameHistory");

            entity.Property(e => e.LastPlayedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<HealthPassport>(entity =>
        {
            entity.HasKey(e => e.PassportId).HasName("PK__HealthPa__185653D01B5F7BCE");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6B39123707F");

            entity.ToTable("Inventory");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Inventor__5E5486486E0F346A");

            entity.ToTable("InventoryLog");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<LineBot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LINE Bot__3214EC0789C6A1C6");

            entity.ToTable("LINE Bot");

            entity.Property(e => e.ChatContent).HasMaxLength(1);
            entity.Property(e => e.ChatDate)
                .HasDefaultValueSql("(getdate())", "DF__LINE Bot__ChatDa__5070F446")
                .HasColumnType("datetime");
            entity.Property(e => e.ChiefComplaint).HasMaxLength(1);
            entity.Property(e => e.QuestionType).HasMaxLength(1);
        });

        modelBuilder.Entity<LoginActivity>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LoginAct__5E5499A88BCAA291");

            entity.ToTable("LoginActivity");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.AuthType).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LoginTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
        });

        modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.HasKey(e => e.MedicalDetailId).HasName("PK__MedicalH__DC48BDDE47CCC4BD");

            entity.ToTable("MedicalHistory");

            entity.Property(e => e.MedicalDetailId).HasColumnName("MedicalDetailID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Disease).HasMaxLength(1);
            entity.Property(e => e.DiseaseTreatment).HasMaxLength(1);
            entity.Property(e => e.Location).HasMaxLength(1);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C037CE4C2BAF7");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.ChatroomId).HasColumnName("ChatroomID");
            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
        });

        modelBuilder.Entity<MissingReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__MissingR__D5BD48058624F120");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastSeenDate).HasColumnType("datetime");
            entity.Property(e => e.LastSeenLat).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LastSeenLng).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LostLocation).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Notify>(entity =>
        {
            entity.HasKey(e => e.NotifyId).HasName("PK__Notify__AD54A2DC4F8BB824");

            entity.ToTable("Notify");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreateAt }, "IX_Notification_User_Read")
                .IsDescending(false, false, true)
                .HasFilter("([IsExist]=(1))");

            entity.Property(e => e.NotifyId).HasColumnName("NotifyID");
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_at");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<OauthTable>(entity =>
        {
            entity.HasKey(e => e.OauthId).HasName("PK__OAuthTab__BE2FE48D6424D4CC");

            entity.ToTable("OAuthTable");

            entity.Property(e => e.AccessToken).HasMaxLength(255);
            entity.Property(e => e.AuthType).HasMaxLength(20);
            entity.Property(e => e.ProviderKey).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pets__48E538624A948680");

            entity.Property(e => e.BehavioralTraits).HasMaxLength(1);
            entity.Property(e => e.CoatColor).HasMaxLength(1);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(1);
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.Photo).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<PlayerProfile>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("PK__PlayerPr__4A4E74C8A2F5E505");

            entity.ToTable("PlayerProfile");
        });

        modelBuilder.Entity<PointRecord>(entity =>
        {
            entity.HasKey(e => e.PointDetailId).HasName("PK__PointRec__C990F497C1815603");

            entity.ToTable("PointRecord");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.Upstream).HasMaxLength(50);
        });

        modelBuilder.Entity<QARecord>(entity =>
        {
            entity.HasKey(e => e.Qaid).HasName("PK__Q&A Reco__DFA593A04B46895D");

            entity.ToTable("Q&A Record");

            entity.Property(e => e.Qaid).HasColumnName("QAId");
            entity.Property(e => e.ChatContent).HasMaxLength(1);
            entity.Property(e => e.ChiefComplaint).HasMaxLength(1);
            entity.Property(e => e.Csname)
                .HasMaxLength(1)
                .HasColumnName("CSName");
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.QuestionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionType).HasMaxLength(1);
            entity.Property(e => e.ReplyContent).HasMaxLength(1);
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<RoleTable>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__RoleTabl__8AFACE1AAB2C433F");

            entity.ToTable("RoleTable");

            entity.Property(e => e.RoleName).HasMaxLength(20);
        });

        modelBuilder.Entity<SkinShop>(entity =>
        {
            entity.HasKey(e => e.SkinId).HasName("PK__SkinShop__9A70C30B0A8A8A23");

            entity.ToTable("SkinShop");

            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.IsDel).HasColumnName("IsDEL");
            entity.Property(e => e.SkinImage)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SkinName).HasMaxLength(50);
        });

        modelBuilder.Entity<SystemTable>(entity =>
        {
            entity.HasKey(e => e.SystemId).HasName("PK__SystemTa__9394F68A8215729F");

            entity.ToTable("SystemTable");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SystemName).HasMaxLength(20);
        });

        modelBuilder.Entity<UserAuthTable>(entity =>
        {
            entity.HasKey(e => e.AuthId).HasName("PK__UserAuth__12C15DD37DEB1BB1");

            entity.ToTable("UserAuthTable");

            entity.HasIndex(e => e.UserId, "UQ__UserAuth__1788CC4DD2603A09").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserAuth__A9D105348970C216").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PasswordResetExpires).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
        });

        modelBuilder.Entity<UserSystemRole>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK__UserSyst__8B5781BD54CE4666");

            entity.ToTable("UserSystemRole");

            entity.Property(e => e.MappingId).HasColumnName("MappingID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserTabl__1788CC4CBEA8B341");

            entity.ToTable("UserTable");

            entity.Property(e => e.City).HasMaxLength(10);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeleteDay).HasColumnType("datetime");
            entity.Property(e => e.HasPriorExp).HasDefaultValue(false);
            entity.Property(e => e.IsSubscribe).HasDefaultValue(true);
            entity.Property(e => e.IsVerify).HasDefaultValue(false);
            entity.Property(e => e.Job).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(10);
            entity.Property(e => e.Point).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<VaccinationStatus>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Vaccinat__4D7B4ABD09FE6BC4");

            entity.ToTable("VaccinationStatus");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(1);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
