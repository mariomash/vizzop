using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace vizzopWeb.Models
{
    public class vizzopContext : DbContext
    {
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Converser> Conversers { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Ubication> Ubications { get; set; }
        public DbSet<CommSession> CommSessions { get; set; }
        public DbSet<TextString> TextStrings { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<WebLocation> WebLocations { get; set; }
        public DbSet<WebLocation_History> WebLocations_History { get; set; }
        public DbSet<ScreenCapture> ScreenCaptures { get; set; }
        public DbSet<ScreenMovie> ScreenMovies { get; set; }
        public DbSet<ZenSession> ZenSessions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<LoginAction> LoginActions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<MeetingSession> MeetingSessions { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<FaqDetails> FaqDetails { get; set; }
        public DbSet<Isocode> Isocodes { get; set; }
        public DbSet<MessageAudit> MessageAudits { get; set; }
        public DbSet<BrowserFeature> BrowserFeatures { get; set; }
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure Code First to ignore PluralizingTableName convention
            // If you keep this convention then the generated tables will have pluralized names.
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

    }
}