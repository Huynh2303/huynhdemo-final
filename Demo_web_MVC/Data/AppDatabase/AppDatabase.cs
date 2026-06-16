using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Data.AppDatabase
{
    public class AppDatabase : DbContext
    {

        public AppDatabase(DbContextOptions<AppDatabase> options) : base(options) { }
        
        public DbSet<Demo_web_MVC.Models.User> Users { get; set; }
        public DbSet<Demo_web_MVC.Models.Contact> Contacts { get; set; }
        public DbSet<Demo_web_MVC.Models.UserToken> userTokens { get; set; }
        public DbSet<Demo_web_MVC.Models.Role> Roles { get; set; }
        public DbSet<Demo_web_MVC.Models.UserRole> UserRoles { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }

        public virtual DbSet<Cart> Carts { get; set; }

        public virtual DbSet<CartItem> CartItems { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<FraudAnalysis> FraudAnalyses { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<OrderLog> OrderLogs { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<ProductVariant> ProductVariants { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<ProductVariantImage > ProductVariantImages { get; set; }
        public virtual DbSet<UserImage> UserImages  { get; set; }
        public virtual DbSet<Conversation> Conversations { get; set; }
        public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<MessageAttachment> MessageAttachments { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<UserToken>()

                .HasOne(t => t.User)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(t => t.UserId);
                
            modelBuilder.Entity<UserToken>()
                .HasIndex(t => t.Token)
                .IsUnique();
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Code = "USER", Name = "Người dùng" },
                new Role { Id = 2, Code = "ADMIN", Name = "Quản trị" },
                new Role { Id = 3, Code = "STAFF", Name = "Nhân viên" }
            );
            // Cấu hình các entity khác
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Addresse__3214EC0758FDAC5E");

                entity.HasIndex(e => e.UserId, "idx_addresses_userid");
                entity.Property(e => e.AddressLine)
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(e => e.City)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.Country)
                    .HasMaxLength(100)
                    .IsUnicode(true);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_addresses_user");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Carts__3214EC073F6079E0");

                entity.HasIndex(e => e.UserId, "idx_carts_userid");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(true)
                    .HasDefaultValue("active");

                entity.HasOne(d => d.User).WithMany(p => p.Carts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_carts_user");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC0736AE55CA");

                entity.HasIndex(e => e.CartId, "idx_cartitems_cartid");

                entity.HasIndex(e => e.VariantId, "idx_cartitems_variantid");

                entity.HasIndex(e => new { e.CartId, e.VariantId }, "uq_cart_variant").IsUnique();

                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.CartId)
                    .HasConstraintName("fk_cartitems_cart");

                entity.HasOne(d => d.Variant).WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cartitems_variant");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC077F70D310");

                entity.HasIndex(e => e.ParentId, "idx_categories_parentid");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("fk_categories_parent");
            });

            modelBuilder.Entity<FraudAnalysis>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__FraudAna__3214EC07D1803ABB");

                entity.ToTable("FraudAnalysis");

                entity.HasIndex(e => e.OrderId, "IX_FraudAnalysis_OrderId")
                    .IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.Property(e => e.ModelName)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.RiskScore)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RiskLevel)
                    .HasMaxLength(20)
                    .IsUnicode(true);

                entity.Property(e => e.RiskReasons)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.InputSnapshot)
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.Order)
                    .WithOne(p => p.FraudAnalysis)
                    .HasForeignKey<FraudAnalysis>(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_fraudanalysis_order");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC079A4B06C9");

                entity.HasIndex(e => e.UserId, "idx_orders_userid");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status)
                    .HasConversion<int>()          // 🔥 QUAN TRỌNG
                    .HasDefaultValue(OrderStatus.Pending);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.User).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orders_user");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC0725749632");

                entity.HasIndex(e => e.OrderId, "idx_orderitems_orderid");

                entity.HasIndex(e => e.VariantId, "idx_orderitems_variantid");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("fk_orderitems_order");

                entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderitems_variant");
            });

            modelBuilder.Entity<OrderLog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__OrderLog__3214EC071A7FFF54");

                entity.HasIndex(e => e.OrderId, "idx_orderlogs_orderid");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(true);

                entity.HasOne(d => d.Order).WithMany(p => p.OrderLogs)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("fk_orderlogs_order");
            });

            //modelBuilder.Entity<Payment>(entity =>
            //{
            //    entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07AE94A3CB");

            //    entity.HasIndex(e => e.OrderId, "idx_payments_orderid");

            //    entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            //    entity.Property(e => e.CreatedAt)
            //        .HasDefaultValueSql("(getdate())")
            //        .HasColumnType("datetime");
            //    entity.Property(e => e.PaymentMethod)
            //        .HasMaxLength(50)
            //        .IsUnicode(true);
            //    entity.Property(e => e.Status)
            //        .HasMaxLength(20)
            //        .IsUnicode(true)
            //        .HasDefaultValue("pending");

            //    entity.HasOne(d => d.Order).WithMany(p => p.Payments)
            //        .HasForeignKey(d => d.OrderId)
            //        .HasConstraintName("fk_payments_order");
            //});

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Products__3214EC07816071E2");

                entity.HasIndex(e => e.CategoryId, "idx_products_categoryid");

                entity.Property(e => e.Brand)
                    .HasMaxLength(100)
                    .IsUnicode(true);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Description).IsUnicode(true);
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(true);
                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);
                entity.HasOne(d => d.Category).WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_products_category");
                // Quan hệ seller - product
                entity.HasOne(d => d.Seller)
                    .WithMany(u => u.SellerProducts)
                    .HasForeignKey(d => d.SellerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_products_seller");
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ProductV__3214EC078FDE3CF6");

                entity.HasIndex(e => e.ProductId, "idx_variants_productid");

                entity.Property(e => e.Color)
                    .HasMaxLength(50)
                    .IsUnicode(true);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Size)
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("fk_variants_product");
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ProductId, "idx_productimages_productid");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SortOrder)
                    .HasDefaultValue(0);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_productimages_product");
            });

            modelBuilder.Entity<ProductVariantImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.VariantId, "idx_variantimages_variantid");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SortOrder)
                    .HasDefaultValue(0);

                entity.HasOne(d => d.Variant)
                    .WithMany(v => v.ProductVariantImages)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_variantimages_variant");
            });

            modelBuilder.Entity<UserImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId, "uq_userimages_userid")
                    .IsUnique();

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(u => u.UserImage)
                    .HasForeignKey<UserImage>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_userimages_user");
            });
            // tạo chat
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(x => x.LastMessage)
                    .HasMaxLength(500)
                    .IsUnicode(true);

                entity.Property(x => x.CreatedAt)
                    .HasColumnType("datetime");

                entity.Property(x => x.LastMessageAt)
                    .HasColumnType("datetime");

                entity.HasOne(x => x.Order)
                    .WithMany()
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Seller)
                    .WithMany()
                    .HasForeignKey(x => x.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.RoleInConversation)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(x => x.LastReadAt)
                    .HasColumnType("datetime");

                entity.Property(x => x.JoinedAt)
                    .HasColumnType("datetime");

                entity.HasOne(x => x.Conversation)
                    .WithMany(x => x.Participants)
                    .HasForeignKey(x => x.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ConversationId, x.UserId })
                    .IsUnique();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MessageType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(x => x.Content)
                    .HasColumnType("nvarchar(max)");

                entity.Property(x => x.CreatedAt)
                    .HasColumnType("datetime");

                entity.HasOne(x => x.Conversation)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Sender)
                    .WithMany()
                    .HasForeignKey(x => x.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MessageAttachment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FileName)
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(x => x.FileUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(x => x.ContentType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(x => x.CreatedAt)
                    .HasColumnType("datetime");

                entity.HasOne(x => x.Message)
                    .WithMany(x => x.Attachments)
                    .HasForeignKey(x => x.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            //thông báo
            modelBuilder.Entity<Notification>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
