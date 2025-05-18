using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class ApplicationDbContext : IdentityDbContext<Models.User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Models.Conversation> Conversations { get; set; } = default!;
        public DbSet<Models.ConversationParticipant> ConversationParticipants { get; set; } = default!;
        public DbSet<Models.Message> Messages { get; set; } = default!;
        public DbSet<Models.EmailVerification> EmailVerifications { get; set; } = default!;
        public DbSet<Models.RefreshToken> RefreshTokens { get; set; } = default!;



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Correctly rename tables
            builder.Entity<Models.User>(b => b.ToTable("Users"));
            builder.Entity<IdentityRole<Guid>>(b => b.ToTable("Roles"));
            builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));
            builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
            builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
            builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
            builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));


            // Custom primary keys or additional configuration
            builder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            builder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.HasKey(r => new { r.UserId, r.RoleId });
            });

            builder.Entity<IdentityUserToken<Guid>>(b =>
            {
                b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });

            // Domain: Conversation → Message (LastMessage)
            builder.Entity<Models.Conversation>()
                .HasOne(e => e.LastMessage)
                .WithOne(e => e.Conversation)
                .HasForeignKey<Models.Conversation>(e => e.LastMessageId)
                .IsRequired(false);

            // Domain: ConversationParticipants
            builder.Entity<Models.ConversationParticipant>()
                .HasOne(e => e.Conversation)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.ConversationId)
                .IsRequired(true);

            builder.Entity<Models.ConversationParticipant>()
                .HasOne(e => e.Participant)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.ParticipantId)
                .IsRequired(true);

            builder.Entity<Models.ConversationParticipant>()
                .HasOne(e => e.LastSeenMessage);

            // Domain: Messages
            builder.Entity<Models.Message>()
                .HasOne(e => e.Sender)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.SenderId)
                .IsRequired(true);
        }

    }
}
