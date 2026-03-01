using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Models.Migrations.Data;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Repository.Implement;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Implement;
using OnlineLearningPlatform.Services.Implementations;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.Settings;

namespace OnlineLearningPlatform.RazorPages
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================= DATABASE =================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ================= IDENTITY =================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ================= SETTINGS =================
            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));

            builder.Services.Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));

            // ================= COOKIE CONFIG =================
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";
            });

            // ================= SERVICES =================
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IProgressService, ProgressService>();
            builder.Services.AddScoped<ISectionService, SectionService>();
            builder.Services.AddScoped<ILessonService, LessonService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IDiscussionService, DiscussionService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddScoped<IEmailService, EmailService>();

            // ================= REPOSITORIES =================
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IQuizRepository, QuizRepository>();
            builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
            builder.Services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
            builder.Services.AddScoped<ISectionRepository, SectionRepository>();
            builder.Services.AddScoped<ILessonRepository, LessonRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IDiscussionRepository, DiscussionRepository>();
            builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // ================= EXTERNAL AUTH =================
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId =
                        builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
                    options.ClientSecret =
                        builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
                });

            // ================= AUTHORIZATION =================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin",
                    policy => policy.RequireRole("Admin"));
                options.AddPolicy("Teacher",
                    policy => policy.RequireRole("Teacher"));
                options.AddPolicy("Student",
                    policy => policy.RequireRole("Student"));
            });

            // ================= RAZOR PAGES =================
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "Admin");
                options.Conventions.AuthorizeAreaFolder("Teacher", "/", "Teacher");
                options.Conventions.AuthorizeAreaFolder("Student", "/", "Student");
                options.Conventions.AllowAnonymousToFolder("/Auth");
                options.Conventions.AllowAnonymousToFolder("/Landing");
            });

            builder.Services.AddControllers();

            // ================= SIGNALR =================
            builder.Services.AddSignalR();

            var app = builder.Build();

            // ================= SEED DATA =================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedData.InitializeAsync(services);
            }

            // ================= MIDDLEWARE =================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // ================= ROUTING =================
            app.MapGet("/", () => Results.Redirect("/Landing"));
            app.MapRazorPages();
            app.MapControllers();

            app.MapHub<ProgressHub>("/hubs/progress");
            app.MapHub<NotificationHub>("/hubs/notification");
            app.MapHub<DataHub>("/hubs/data");

            app.Run();
        }
    }
}