using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Models;

public partial class PetdbContext : DbContext
{
    public PetdbContext()
    {
    }

    public PetdbContext(DbContextOptions<PetdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdoptionRecords> AdoptionRecords { get; set; }

    public virtual DbSet<Article> Article { get; set; }

    public virtual DbSet<ArticleImage> ArticleImage { get; set; }

    public virtual DbSet<Bookmark> Bookmark { get; set; }

    public virtual DbSet<Category> Category { get; set; }

    public virtual DbSet<Chatrooms> Chatrooms { get; set; }

    public virtual DbSet<Comment> Comment { get; set; }

    public virtual DbSet<ENewsletter> ENewsletter { get; set; }

    public virtual DbSet<Faq> Faq { get; set; }

    public virtual DbSet<Following> Following { get; set; }

    public virtual DbSet<GameContent> GameContent { get; set; }

    public virtual DbSet<GameHistory> GameHistory { get; set; }

    public virtual DbSet<HealthPassports> HealthPassports { get; set; }

    public virtual DbSet<Inventory> Inventory { get; set; }

    public virtual DbSet<InventoryLog> InventoryLog { get; set; }

    public virtual DbSet<LineBot> LineBot { get; set; }

    public virtual DbSet<LoginActivityTable> LoginActivityTable { get; set; }

    public virtual DbSet<MedicalHistory> MedicalHistory { get; set; }

    public virtual DbSet<Messages> Messages { get; set; }

    public virtual DbSet<MissingReports> MissingReports { get; set; }

    public virtual DbSet<Notify> Notify { get; set; }

    public virtual DbSet<OauthTable> OauthTable { get; set; }

    public virtual DbSet<Pets> Pets { get; set; }

    public virtual DbSet<PlayerProfile> PlayerProfile { get; set; }

    public virtual DbSet<PointRecord> PointRecord { get; set; }

    public virtual DbSet<QARecord> QARecord { get; set; }

    public virtual DbSet<RoleTable> RoleTable { get; set; }

    public virtual DbSet<SkinShop> SkinShop { get; set; }

    public virtual DbSet<SystemTable> SystemTable { get; set; }

    public virtual DbSet<UserAuthTable> UserAuthTable { get; set; }

    public virtual DbSet<UserSystemRole> UserSystemRole { get; set; }

    public virtual DbSet<UserTable> UserTable { get; set; }

    public virtual DbSet<VaccinationStatus> VaccinationStatus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=petdb;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdoptionRecords>(entity =>
        {
            entity.HasKey(e => e.AdoptionId).HasName("PK__Adoption__38BABF2C3A551414");

            entity.Property(e => e.ReturnReason).HasMaxLength(50);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("PK__Article__9C6270C8F2F22431");

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
            entity.HasKey(e => e.ImageId).HasName("PK__ArticleI__7516F4EC33E8BBAB");

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
            entity.HasKey(e => e.BookmarkId).HasName("PK__Bookmark__541A3A91FC289266");

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
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B966B187B");

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

        modelBuilder.Entity<Chatrooms>(entity =>
        {
            entity.HasKey(e => e.ChatroomId).HasName("PK__Chatroom__B83BDF2880B61613");

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
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFAA29D5F8F8");

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
            entity.HasKey(e => e.NewsLetterId).HasName("PK__e-newsle__7F740FC22F54BBF0");

            entity.ToTable("e-newsletter");

            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.PublishDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.Summary).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Faqid).HasName("PK__FAQ__4B89D182DC97FEBF");

            entity.ToTable("FAQ");

            entity.Property(e => e.Faqid).HasColumnName("FAQId");
            entity.Property(e => e.Answer).HasMaxLength(500);
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsExist).HasDefaultValue(true);
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.Question).HasMaxLength(50);
            entity.Property(e => e.QuestionType).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.StoppedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Following>(entity =>
        {
            entity.HasKey(e => e.FollowingIdPk).HasName("PK__Followin__A05EF690AC98D0E5");

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
            entity.HasKey(e => e.GameId).HasName("PK__GameCont__2AB897FD08EBE8CA");

            entity.Property(e => e.GameId).ValueGeneratedNever();
            entity.Property(e => e.AnswersDetail).HasMaxLength(300);
            entity.Property(e => e.GameName).HasMaxLength(10);
            entity.Property(e => e.Questions).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<GameHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__GameHist__4D7B4ABDEE703FEF");

            entity.Property(e => e.HistoryId).ValueGeneratedNever();
            entity.Property(e => e.LastPlayedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<HealthPassports>(entity =>
        {
            entity.HasKey(e => e.PassportId).HasName("PK__HealthPa__185653D048D671F0");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6B386B7F360");

            entity.Property(e => e.InventoryId).ValueGeneratedNever();
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Inventor__5E54864820BB171A");

            entity.Property(e => e.LogId).ValueGeneratedNever();
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<LineBot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LINE Bot__3214EC07E7074582");

            entity.ToTable("LINE Bot");

            entity.Property(e => e.ChatContent).HasMaxLength(100);
            entity.Property(e => e.ChatDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChiefComplaint).HasMaxLength(50);
            entity.Property(e => e.QuestionType).HasMaxLength(20);
        });

        modelBuilder.Entity<LoginActivityTable>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LoginAct__5E5499A8E5C18ECD");

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
            entity.HasKey(e => e.MedicalDetailId).HasName("PK__MedicalH__DC48BDDE84D7D3CF");

            entity.Property(e => e.MedicalDetailId).HasColumnName("MedicalDetailID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Disease).HasMaxLength(20);
            entity.Property(e => e.DiseaseTreatment).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(20);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Messages>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C037CF13C968F");

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

        modelBuilder.Entity<MissingReports>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__MissingR__D5BD48056D59B634");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LastSeenDate).HasColumnType("datetime");
            entity.Property(e => e.LastSeenLat).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LastSeenLng).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LostLocation).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Notify>(entity =>
        {
            entity.HasKey(e => e.NotifyId).HasName("PK__Notify__AD54A2DCCACF6635");

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
            entity.HasKey(e => e.OauthId).HasName("PK__OAuthTab__BE2FE48D7C1D7D48");

            entity.ToTable("OAuthTable");

            entity.Property(e => e.AccessToken).HasMaxLength(255);
            entity.Property(e => e.AuthType).HasMaxLength(20);
            entity.Property(e => e.ProviderKey).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
        });

        modelBuilder.Entity<Pets>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pets__48E53862BA147928");

            entity.Property(e => e.BehavioralTraits).HasMaxLength(50);
            entity.Property(e => e.CoatColor).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.Photo).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<PlayerProfile>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("PK__PlayerPr__4A4E74C83183372B");

            entity.Property(e => e.PlayerId).ValueGeneratedNever();
        });

        modelBuilder.Entity<PointRecord>(entity =>
        {
            entity.HasKey(e => e.PointDetailId).HasName("PK__PointRec__C990F497AA702E07");

            entity.Property(e => e.PointDetailId).ValueGeneratedNever();
            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.Upstream).HasMaxLength(50);
        });

        modelBuilder.Entity<QARecord>(entity =>
        {
            entity.HasKey(e => e.Qaid).HasName("PK__Q&A Reco__DFA593A0545DA5C1");

            entity.ToTable("Q&A Record");

            entity.Property(e => e.Qaid).HasColumnName("QAId");
            entity.Property(e => e.ChatContent).HasMaxLength(2000);
            entity.Property(e => e.ChiefComplaint).HasMaxLength(50);
            entity.Property(e => e.Csname)
                .HasMaxLength(20)
                .HasColumnName("CSName");
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.QuestionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionType).HasMaxLength(20);
            entity.Property(e => e.ReplyContent).HasMaxLength(100);
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<RoleTable>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__RoleTabl__8AFACE1AD2B14F8A");

            entity.Property(e => e.RoleName).HasMaxLength(20);
        });

        modelBuilder.Entity<SkinShop>(entity =>
        {
            entity.HasKey(e => e.SkinId).HasName("PK__SkinShop__9A70C30BF5BE056C");

            entity.Property(e => e.SkinId).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.IsDel).HasColumnName("IsDEL");
            entity.Property(e => e.SkinImage)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SkinName).HasMaxLength(50);
        });

        modelBuilder.Entity<SystemTable>(entity =>
        {
            entity.HasKey(e => e.SystemId).HasName("PK__SystemTa__9394F68AADCAA7FF");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SystemName).HasMaxLength(20);
        });

        modelBuilder.Entity<UserAuthTable>(entity =>
        {
            entity.HasKey(e => e.AuthId).HasName("PK__UserAuth__12C15DD3A6BC357E");

            entity.HasIndex(e => e.UserId, "UQ__UserAuth__1788CC4D7003F3E4").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserAuth__A9D105349545B5C8").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PasswordResetExpires).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
        });

        modelBuilder.Entity<UserSystemRole>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK__UserSyst__8B5781BD504E63CB");

            entity.Property(e => e.MappingId).HasColumnName("MappingID");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserTabl__1788CC4CD7D41605");

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
            entity.HasKey(e => e.HistoryId).HasName("PK__Vaccinat__4D7B4ABD11515C75");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(20);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(10);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
