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


namespace OnlineLearningPlatform.RazorPages
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";
            });

            // Services
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
            // Dashboard service: tổng hợp stat cho Admin Dashboard
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            
            // Payment, Order, Wallet services
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IWalletService, WalletService>();

            // Repositories
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
            // Dashboard repository: query tổng hợp cho Dashboard, không dùng UserManager
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            
            // Payment, Order, Wallet repositories
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
                options.AddPolicy("Student", policy => policy.RequireRole("Student"));
            });

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "Admin");
                options.Conventions.AuthorizeAreaFolder("Teacher", "/", "Teacher");
                options.Conventions.AuthorizeAreaFolder("Student", "/", "Student");
                options.Conventions.AllowAnonymousToFolder("/Auth");
            });

            // ================= SIGNALR =================
            builder.Services.AddSignalR();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedData.InitializeAsync(services);
            }

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

            app.MapGet("/", () => Results.Redirect("/Auth/Login"));
            app.MapRazorPages();
            app.MapHub<ProgressHub>("/hubs/progress");

            app.Run();
        }
    }
}
