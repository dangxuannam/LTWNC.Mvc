using CheapDeal.WebApp.Areas.Adm.Controllers;
using CheapDeal.WebApp.DAL;
using CheapDeal.WebApp.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Reflection;


namespace CheapDeal.WebApp.DAL
{
    public class ShopDbContext : IdentityDbContext<Account>
    {
        public ShopDbContext() : base("DefaultConnection")
        {
        }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductHistory> ProductHistories { get; set; }
        public DbSet<Picture> Picture { get; set; }


        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductProfile> ProductProfiles { get; set; }
        public DbSet<ShippingRate> ShippingRates { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AllModels.Notification> Notifications { get; set; }
        public DbSet<AllModels.UserActivity> UserActivities { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractStatus> ContractStatuses { get; set; }
        public DbSet<ContractPaymentSchedule> ContractPaymentSchedules { get; set; }
        public DbSet<ContractFileMetadata> ContractFileMetadatas { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<HangfireJobLog> HangfireJobLogs { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Supplier>()
            .Property(s => s.RowVersion)
            .IsConcurrencyToken()
            .IsRowVersion();

            modelBuilder.Entity<UserProfile>()
            .Property(p => p.Gender)
            .IsOptional();

            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserInRoles");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");

            modelBuilder.Entity<Category>()
                .HasMany(c => c.ChildCates)
                .WithOptional(c => c.Parent)
                .HasForeignKey(c => c.ParentID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithMany(p => p.Categories)
                .Map(m => m.MapLeftKey("CategoryId")
                            .MapRightKey("ProductId")
                            .ToTable("ProductCategory"));

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithRequired(d => d.Order)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Product>()
                .HasMany(p => p.OrderDetails)
                .WithRequired(d => d.Product)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Shipper>()
                 .HasMany(s => s.Orders)
                    .WithOptional(o => o.Shipper)
                    .HasForeignKey(o => o.ShipVia);

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.Products)
                .WithRequired(p => p.Supplier)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Account>()
                .HasMany(a => a.CustomerOrders)
                .WithRequired(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(a => a.HandleOrders)
                .WithOptional(o => o.Employee)
                .HasForeignKey(o => o.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(a => a.ProductHistories)
                .WithRequired(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Product>()
               .HasMany(p => p.Pictures)
               .WithRequired(d => d.Product)
               .WillCascadeOnDelete();


            modelBuilder.Entity<Account>()
                .HasMany(a => a.ProductHistories)
                .WithOptional(c => c.Account)
                .HasForeignKey(c => c.AccountId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Account>()
                .HasOptional(a => a.Profile)
                .WithRequired(u => u.Account);

            modelBuilder.Entity<UserActivityLog>()
                .HasRequired(a => a.Account)
                .WithMany()
                .HasForeignKey(a => a.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AllModels.Notification>()
                .HasRequired(n => n.Account)
                .WithMany()
                .HasForeignKey(n => n.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AllModels.UserActivity>()
                .HasRequired(a => a.Account)
                .WithMany()
                .HasForeignKey(a => a.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<Contract>()
                   .ToTable("Contracts");                  

            modelBuilder.Entity<ContractStatus>()
                        .ToTable("ContractStatus");             

            modelBuilder.Entity<ContractPaymentSchedule>()
                        .ToTable("ContractPaymentSchedule");    

            modelBuilder.Entity<ContractFileMetadata>()
                        .ToTable("ContractFileMetadata");       

            modelBuilder.Entity<Contract>()
                        .Property(c => c.RowVersion)
                        .IsRowVersion();

            modelBuilder.Entity<Contract>()
                        .HasRequired(c => c.Status)
                        .WithMany(s => s.Contracts)
                        .HasForeignKey(c => c.StatusId);

            modelBuilder.Entity<Contract>()
                        .HasRequired(c => c.Customer)
                        .WithMany()
                        .HasForeignKey(c => c.CustomerId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<ContractPaymentSchedule>()
                        .HasRequired(s => s.Contract)
                        .WithMany(c => c.Schedules)
                        .HasForeignKey(s => s.ContractId);

            modelBuilder.Entity<ContractFileMetadata>()
                        .HasRequired(f => f.Contract)
                        .WithMany()
                        .HasForeignKey(f => f.ContractId)
                        .WillCascadeOnDelete(true);

            modelBuilder.Entity<Reminder>()
            .ToTable("Reminders");

            modelBuilder.Entity<HangfireJobLog>()
                        .ToTable("HangfireJobLog");

            // Map quan hệ Reminder → ContractPaymentSchedule
            modelBuilder.Entity<Reminder>()
                        .HasRequired(r => r.Schedule)
                        .WithMany()
                        .HasForeignKey(r => r.ScheduleId)
                        .WillCascadeOnDelete(true);

            // Map quan hệ Reminder → Contract
            modelBuilder.Entity<Reminder>()
                        .HasRequired(r => r.Contract)
                        .WithMany()
                        .HasForeignKey(r => r.ContractId)
                        .WillCascadeOnDelete(false);

            // Không map DurationSeconds (computed property)
            modelBuilder.Entity<HangfireJobLog>()
                        .Ignore(j => j.DurationSeconds);
        }

        public static ShopDbContext Create()
        {
            return new ShopDbContext();
        }
    }
}