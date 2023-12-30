using ChatCommon.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace ChatDB

{
    public class ChatContext : DbContext
    {
        
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ChatContext()
        {

        }

        public ChatContext(DbContextOptions dbc) : base(dbc) 
        {
            //Database.EnsureCreated(); // - создать дб, если её нет
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer
                (@"Server=localhost; Database=GB; Integrated Security=False;
                TrustServerCertificate=true; Trusted_Connection=True").UseLazyLoadingProxies();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id).HasName("user_pk");
                entity.ToTable("users");
                entity.HasIndex(x => x.Fullname).IsUnique();
                entity.Property(e => e.Fullname).HasColumnName("Fullname").HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(x => x.MessageId).HasName("mesage_pk");
                entity.ToTable("messages");

                entity.Property(e => e.MessageId).HasColumnName("id");
                entity.Property(e => e.Text).HasColumnName("message_text");
                entity.Property(e => e.DateSend).HasColumnName("message_data");
                entity.Property(e => e.IsSend).HasColumnName("is_send");

                entity.HasOne(x => x.UserTo).WithMany(m => m.MessagesTo).
                HasForeignKey(x => x.UserToId).HasConstraintName("message_to_user_fk");

                entity.HasOne(x => x.UserFrom).WithMany(m => m.MessagesFrom).
                HasForeignKey(x => x.UserFromId).HasConstraintName("message_from_user_fk");
            });
            //base.OnModelCreating(modelBuilder);
        }


    }
}
