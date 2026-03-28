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

    public virtual DbSet<ENewsletter> ENewsletters { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<GameContent> GameContents { get; set; }

    public virtual DbSet<GameHistory> GameHistories { get; set; }

    public virtual DbSet<HealthPassport> HealthPassports { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryLog> InventoryLogs { get; set; }

    public virtual DbSet<LineBot> LineBots { get; set; }

    public virtual DbSet<LoginActivity> LoginActivities { get; set; }

    public virtual DbSet<MedicalHistory> MedicalHistories { get; set; }

    public virtual DbSet<MissingReport> MissingReports { get; set; }

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
            entity.HasKey(e => e.AdoptionId).HasName("PK__Adoption__38BABF2C819F666F");

            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.ReturnReason).HasMaxLength(1);
        });

        modelBuilder.Entity<ENewsletter>(entity =>
        {
            entity.HasKey(e => e.NewsLetterId).HasName("PK__E-Newsle__7F740FC2A551C4A1");

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
            entity.HasKey(e => e.Faqid).HasName("PK__FAQ__4B89D182FAF343EC");

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

        modelBuilder.Entity<GameContent>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__GameCont__2AB897FDC2CE58DA");

            entity.ToTable("GameContent");

            entity.Property(e => e.AnswersDetail).HasMaxLength(300);
            entity.Property(e => e.GameName).HasMaxLength(10);
            entity.Property(e => e.Questions).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<GameHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__GameHist__4D7B4ABDE31D5EB9");

            entity.ToTable("GameHistory");

            entity.Property(e => e.LastPlayedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<HealthPassport>(entity =>
        {
            entity.HasKey(e => e.PassportId).HasName("PK__HealthPa__185653D029827285");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6B31E15E807");

            entity.ToTable("Inventory");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Inventor__5E5486480537AC15");

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
            entity.HasKey(e => e.LogId).HasName("PK__LoginAct__5E5499A8EAE84392");

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
            entity.HasKey(e => e.MedicalDetailId).HasName("PK__MedicalH__DC48BDDE84434851");

            entity.ToTable("MedicalHistory");

            entity.Property(e => e.MedicalDetailId).HasColumnName("MedicalDetailID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Disease).HasMaxLength(1);
            entity.Property(e => e.DiseaseTreatment).HasMaxLength(1);
            entity.Property(e => e.Location).HasMaxLength(1);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MissingReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__MissingR__D5BD48058F7342C6");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.LastSeenDate).HasColumnType("datetime");
            entity.Property(e => e.LastSeenLat).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LastSeenLng).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LostLocation).HasMaxLength(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<OauthTable>(entity =>
        {
            entity.HasKey(e => e.OauthId).HasName("PK__OAuthTab__BE2FE48DE1032343");

            entity.ToTable("OAuthTable");

            entity.Property(e => e.AccessToken).HasMaxLength(255);
            entity.Property(e => e.AuthType).HasMaxLength(20);
            entity.Property(e => e.ProviderKey).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pets__48E5386281EC6FD7");

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
            entity.HasKey(e => e.PlayerId).HasName("PK__PlayerPr__4A4E74C812534064");

            entity.ToTable("PlayerProfile");
        });

        modelBuilder.Entity<PointRecord>(entity =>
        {
            entity.HasKey(e => e.PointDetailId).HasName("PK__PointRec__C990F497A7936515");

            entity.ToTable("PointRecord");

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.Upstream).HasMaxLength(50);
        });

        modelBuilder.Entity<QARecord>(entity =>
        {
            entity.HasKey(e => e.Qaid).HasName("PK__Q&A Reco__DFA593A07C45FA2E");

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
            entity.HasKey(e => e.RoleId).HasName("PK__RoleTabl__8AFACE1A11AAC5BF");

            entity.ToTable("RoleTable");

            entity.Property(e => e.RoleName).HasMaxLength(20);
        });

        modelBuilder.Entity<SkinShop>(entity =>
        {
            entity.HasKey(e => e.SkinId).HasName("PK__SkinShop__9A70C30BEAAD2D08");

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
            entity.HasKey(e => e.SystemId).HasName("PK__SystemTa__9394F68A2D42481E");

            entity.ToTable("SystemTable");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SystemName).HasMaxLength(20);
        });

        modelBuilder.Entity<UserAuthTable>(entity =>
        {
            entity.HasKey(e => e.AuthId).HasName("PK__UserAuth__12C15DD376AE85C9");

            entity.ToTable("UserAuthTable");

            entity.HasIndex(e => e.UserId, "UQ__UserAuth__1788CC4DD30DD3DE").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserAuth__A9D10534B618E24B").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PasswordResetExpires).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
        });

        modelBuilder.Entity<UserSystemRole>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK__UserSyst__8B5781BD0236294C");

            entity.ToTable("UserSystemRole");

            entity.Property(e => e.MappingId).HasColumnName("MappingID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserTabl__1788CC4C68A3A205");

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
            entity.HasKey(e => e.HistoryId).HasName("PK__Vaccinat__4D7B4ABDFABACAAE");

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
