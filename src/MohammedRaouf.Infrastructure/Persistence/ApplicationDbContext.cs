using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseSection> CourseSections => Set<CourseSection>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

    public DbSet<PurchaseRequestEvent> PurchaseRequestEvents => Set<PurchaseRequestEvent>();

    public DbSet<PurchaseRequestNumberCounter> PurchaseRequestNumberCounters => Set<PurchaseRequestNumberCounter>();

    public DbSet<ConsultationRequestNumberCounter> ConsultationRequestNumberCounters => Set<ConsultationRequestNumberCounter>();

    public DbSet<ActivationCode> ActivationCodes => Set<ActivationCode>();

    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();

    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();

    public DbSet<ConsultationRequest> ConsultationRequests => Set<ConsultationRequest>();

    public DbSet<Article> Articles => Set<Article>();

    public DbSet<FaqItem> FaqItems => Set<FaqItem>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ExpertiseItem> ExpertiseItems => Set<ExpertiseItem>();

    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    public DbSet<SiteStatistic> SiteStatistics => Set<SiteStatistic>();

    public DbSet<VideoTranscodeJob> VideoTranscodeJobs => Set<VideoTranscodeJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
