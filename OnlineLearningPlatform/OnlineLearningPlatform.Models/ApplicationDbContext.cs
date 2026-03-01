using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        // ===== DbSet cho các entity nghiệp vụ =====

        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<Section> Sections { get; set; } = default!;
        public DbSet<Lesson> Lessons { get; set; } = default!;

        public DbSet<LearningPath> LearningPaths { get; set; } = default!;
        public DbSet<LearningPathCourse> LearningPathCourses { get; set; } = default!;

        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = default!;
        public DbSet<Enrollment> Enrollments { get; set; } = default!;

        public DbSet<Refund> Refunds { get; set; } = default!;
        public DbSet<Wallet> Wallets { get; set; } = default!;
        public DbSet<WalletTransaction> WalletTransactions { get; set; } = default!;

        public DbSet<LessonProgress> LessonProgresses { get; set; } = default!;
        public DbSet<Certificate> Certificates { get; set; } = default!;

        public DbSet<Quiz> Quizzes { get; set; } = default!;
        public DbSet<Question> Questions { get; set; } = default!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = default!;

        public DbSet<Assignment> Assignments { get; set; } = default!;
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; } = default!;

        public DbSet<DiscussionTopic> DiscussionTopics { get; set; } = default!;
        public DbSet<DiscussionReply> DiscussionReplies { get; set; } = default!;

        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<Announcement> Announcements { get; set; } = default!;

        public DbSet<AiConversation> AiConversations { get; set; } = default!;
        public DbSet<AiMessage> AiMessages { get; set; } = default!;
        public DbSet<AiGeneratedExercise> AiGeneratedExercises { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Chỉnh lại tên bảng cho Identity ko để tên gốc cho ae dễ hiểu
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Cấu hình thêm cho đúng nghiệp vụ nè

            // Unique index cho CourseCode
            builder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            // Unique index: mỗi user chỉ enroll 1 lần cho mỗi course
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            // Cấu hình precision cho các trường tiền tệ
            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Course>()
                .Property(c => c.DiscountPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.WalletUsed)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderDetail>()
                .Property(od => od.DiscountApplied)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Refund>()
                .Property(r => r.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");

            builder.Entity<WalletTransaction>()
                .Property(wt => wt.Amount)
                .HasColumnType("decimal(18,2)");

            // ===== Chặn cascade delete để tránh lỗi multiple cascade paths =====
            // Theo README_MODELS_v2: Course.Teacher, Enrollment.User, Order.User, OrderDetail.Order, OrderDetail.Course, Refund.Order

            // Course relationships
            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Course)
                .WithMany()
                .HasForeignKey(od => od.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrollment relationships
            builder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enrollment>()
                .HasOne(e => e.OrderDetail)
                .WithOne(od => od.Enrollment)
                .HasForeignKey<Enrollment>(e => e.OrderDetailId)
                .OnDelete(DeleteBehavior.SetNull);

            // Refund relationships
            builder.Entity<Refund>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Refunds)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Refund>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Refund>()
                .HasOne(r => r.ProcessedBy)
                .WithMany()
                .HasForeignKey(r => r.ProcessedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Certificate relationships - tránh multiple cascade paths từ User và Course
            builder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Certificates)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // DiscussionTopic relationships
            builder.Entity<DiscussionTopic>()
                .HasOne(dt => dt.Creator)
                .WithMany()
                .HasForeignKey(dt => dt.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DiscussionTopic>()
                .HasOne(dt => dt.Course)
                .WithMany(c => c.DiscussionTopics)
                .HasForeignKey(dt => dt.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // DiscussionReply relationships
            builder.Entity<DiscussionReply>()
                .HasOne(dr => dr.User)
                .WithMany()
                .HasForeignKey(dr => dr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DiscussionReply>()
                .HasOne(dr => dr.DiscussionTopic)
                .WithMany(dt => dt.DiscussionReplies)
                .HasForeignKey(dr => dr.TopicId)
                .OnDelete(DeleteBehavior.NoAction);

            // LearningPath relationships
            builder.Entity<LearningPath>()
                .HasOne(lp => lp.CreatedBy)
                .WithMany()
                .HasForeignKey(lp => lp.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LearningPathCourse>()
                .HasOne(lpc => lpc.LearningPath)
                .WithMany(lp => lp.LearningPathCourses)
                .HasForeignKey(lpc => lpc.LearningPathId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LearningPathCourse>()
                .HasOne(lpc => lpc.Course)
                .WithMany(c => c.LearningPathCourses)
                .HasForeignKey(lpc => lpc.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Wallet relationships
            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WalletId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Refund)
                .WithMany()
                .HasForeignKey(wt => wt.RefundId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Order)
                .WithMany()
                .HasForeignKey(wt => wt.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // LessonProgress relationships
            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.User)
                .WithMany()
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.NoAction);

            // Section relationships
            builder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Lesson relationships
            builder.Entity<Lesson>()
                .HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Quiz relationships
            builder.Entity<Quiz>()
                .HasOne(q => q.Lesson)
                .WithMany(l => l.Quizzes)
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(quiz => quiz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.QuizAttempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany()
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<QuizAnswer>()
                .HasOne(qa => qa.QuizAttempt)
                .WithMany(qa => qa.QuizAnswers)
                .HasForeignKey(qa => qa.AttemptId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<QuizAnswer>()
                .HasOne(qa => qa.Question)
                .WithMany(q => q.QuizAnswers)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Assignment relationships
            builder.Entity<Assignment>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.Assignments)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AssignmentSubmission>()
                .HasOne(asub => asub.Assignment)
                .WithMany(a => a.AssignmentSubmissions)
                .HasForeignKey(asub => asub.AssignmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AssignmentSubmission>()
                .HasOne(asub => asub.User)
                .WithMany()
                .HasForeignKey(asub => asub.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Announcement relationships
            builder.Entity<Announcement>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Announcements)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification relationships
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // AI relationships
            builder.Entity<AiConversation>()
                .HasOne(aic => aic.User)
                .WithMany()
                .HasForeignKey(aic => aic.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AiMessage>()
                .HasOne(aim => aim.AiConversation)
                .WithMany(aic => aic.AiMessages)
                .HasForeignKey(aim => aim.ConversationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AiGeneratedExercise>()
                .HasOne(aige => aige.User)
                .WithMany()
                .HasForeignKey(aige => aige.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

