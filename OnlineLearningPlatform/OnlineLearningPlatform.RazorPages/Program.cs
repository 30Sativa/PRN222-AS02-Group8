using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Models.Migrations.Data;
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

            // ================= DB CONTEXT =================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ================= IDENTITY =================
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Password rule (tuỳ chỉnh nếu muốn)
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ================= COOKIE CONFIG =================
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";
            });
            //================== SERVICES =================

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IQuizRepository, QuizRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
            builder.Services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();

            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IStudentService, StudentService>();

            // ================= AUTHORIZATION POLICIES =================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("Teacher", policy =>
                    policy.RequireRole("Teacher"));

                options.AddPolicy("Student", policy =>
                    policy.RequireRole("Student"));
            });

            // ================= RAZOR PAGES =================
            builder.Services.AddRazorPages(options =>
            {
                // Admin Area
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "Admin");

                // Teacher Area
                options.Conventions.AuthorizeAreaFolder("Teacher", "/", "Teacher");

                // Student Area
                options.Conventions.AuthorizeAreaFolder("Student", "/", "Student");

                // Cho phép Login/Register không cần đăng nhập
                options.Conventions.AllowAnonymousToFolder("/Auth");
            });

            var app = builder.Build();

            // ================= SEED DATA =================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedData.InitializeAsync(services);
            }

            // ================= PIPELINE =================
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

            app.Run();
        }
    }
}