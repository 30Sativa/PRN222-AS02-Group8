using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Models.Migrations.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ===== SEED ROLES =====
            string[] roles = { "Admin", "Student", "Teacher" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        throw new Exception($"Failed to create role {role}");
                }
            }

            // ===== SEED ADMIN =====
            await CreateUserIfNotExists(userManager,
                "admin@gmail.com",
                "Admin@123", "Admin",
                "Admin");

            // ===== SEED TEACHER =====
            var teacherUser = await CreateUserIfNotExists(userManager,
                "teacher@gmail.com",
                "Teacher@123", "Teacher",
                "Teacher");

            // ===== SEED STUDENT =====
            var studentUser = await CreateUserIfNotExists(userManager,
                "student@gmail.com",
                "Student@123", "Student",
                "Student");

            // ===== SEED CATEGORIES AND COURSES =====
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await CreateSampleCourses(dbContext, teacherUser);
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName =  fullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new Exception($"Failed to create user {email}");

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        private static async Task CreateSampleCourses(ApplicationDbContext db, ApplicationUser teacher)
        {
            if (db.Courses.Any(c => c.CourseCode == "WEB101")) return; // Đã seed

            var catWebFront = db.Categories.FirstOrDefault(c => c.CategoryName == "Lập trình Web Frontend");
            if (catWebFront == null)
            {
                var newCats = new List<Category>
                {
                    new Category { CategoryName = "Lập trình Web Frontend" },
                    new Category { CategoryName = "Lập trình Web Backend" },
                    new Category { CategoryName = "Lập trình Di động (Mobile)" },
                    new Category { CategoryName = "Khoa học Dữ liệu (Data Science)" },
                    new Category { CategoryName = "Trí tuệ nhân tạo (AI)" },
                    new Category { CategoryName = "DevOps & Cloud" },
                    new Category { CategoryName = "UI/UX Design" },
                    new Category { CategoryName = "Cơ sở dữ liệu (Database)" },
                    new Category { CategoryName = "Lập trình Game" },
                    new Category { CategoryName = "Kiểm thử phần mềm (QA/QC)" }
                };
                db.Categories.AddRange(newCats);
                await db.SaveChangesAsync();
            }

            var c0 = db.Categories.First(c => c.CategoryName == "Lập trình Web Frontend").CategoryId;
            var c1 = db.Categories.First(c => c.CategoryName == "Lập trình Web Backend").CategoryId;
            var c2 = db.Categories.First(c => c.CategoryName == "Lập trình Di động (Mobile)").CategoryId;
            var c3 = db.Categories.First(c => c.CategoryName == "Khoa học Dữ liệu (Data Science)").CategoryId;
            var c4 = db.Categories.First(c => c.CategoryName == "Trí tuệ nhân tạo (AI)").CategoryId;
            var c5 = db.Categories.First(c => c.CategoryName == "DevOps & Cloud").CategoryId;
            var c6 = db.Categories.First(c => c.CategoryName == "UI/UX Design").CategoryId;
            var c7 = db.Categories.First(c => c.CategoryName == "Cơ sở dữ liệu (Database)").CategoryId;
            var c8 = db.Categories.First(c => c.CategoryName == "Lập trình Game").CategoryId;
            var c9 = db.Categories.First(c => c.CategoryName == "Kiểm thử phần mềm (QA/QC)").CategoryId;

            var courses = new List<Course>
            {
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "WEB101", Title = "HTML & CSS Căn Bản Đến Thực Chiến", Slug = "html-css-can-ban-den-thuc-chien",
                    Description = "Khoá học nền tảng nhất cho người mới bắt đầu lập trình web. Sẽ hướng dẫn từ thẻ HTML cơ bản nhất đến làm Layout linh hoạt bằng Flexbox và Grid.",
                    TeacherId = teacher.Id, CategoryId = c0, Price = 0, DiscountPrice = 0,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/6/61/HTML5_logo_and_wordmark.svg", Level = CourseLevel.Beginner, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "JS201", Title = "JavaScript Cơ Bản & Nâng Cao", Slug = "javascript-co-ban-nang-cao",
                    Description = "Thành thạo JavaScript - ngôn ngữ không thể thiếu của lập trình viên Frontend hiện đại. Tìm hiểu ES6+, Async/Await, DOM manipulation.",
                    TeacherId = teacher.Id, CategoryId = c0, Price = 500000, DiscountPrice = 300000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/9/99/Unofficial_JavaScript_logo_2.svg", Level = CourseLevel.Intermediate, Status = CourseStatus.Published, IsFeatured = true
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "REACT301", Title = "Xây dựng SPA với ReactJS", Slug = "xay-dung-spa-voi-reactjs",
                    Description = "Khoá đào tạo ReactJS toàn diện, bao gồm Context API, Redux Toolkit, React Router và cách Optimize Performance.",
                    TeacherId = teacher.Id, CategoryId = c0, Price = 900000, DiscountPrice = 699000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a7/React-icon.svg", Level = CourseLevel.Advanced, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "NET801", Title = "Lập trình Backend với ASP.NET Core 8", Slug = "lap-trinh-backend-aspnetcore-8",
                    Description = "Khoá học thực chiến làm web API, clean architecture, EF Core và SQL Server. Có Authentication bằng JWT và Identity.",
                    TeacherId = teacher.Id, CategoryId = c1, Price = 1200000, DiscountPrice = 850000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ee/.NET_Core_Logo.svg", Level = CourseLevel.Intermediate, Status = CourseStatus.Published, IsFeatured = true
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "NODE201", Title = "Backend REST API với Node.js & Express", Slug = "backend-rest-api-nodejs-express",
                    Description = "Làm quen với Node.js, Express, xử lý bất đồng bộ, MongoDB, Mongoose, Socket.io cơ bản.",
                    TeacherId = teacher.Id, CategoryId = c1, Price = 600000, DiscountPrice = 400000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d9/Node.js_logo.svg", Level = CourseLevel.Intermediate, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "FLUTTER101", Title = "Lập trình Di động Đa nền tảng với Flutter", Slug = "lap-trinh-di-dong-flutter",
                    Description = "Nắm vững ngôn ngữ Dart và framework Flutter để build ứng dụng cho cả iOS và Android chỉ với một codebase.",
                    TeacherId = teacher.Id, CategoryId = c2, Price = 900000, DiscountPrice = 750000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/1/17/Google-flutter-logo.png", Level = CourseLevel.Beginner, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "PYTHON101", Title = "Python Cơ bản & Phân tích dữ liệu", Slug = "python-co-ban-phan-tich-du-lieu",
                    Description = "Học lập trình Python từ con số 0. Làm quen với các thư viện Pandas, NumPy, Matplotlib để vẽ biểu đồ và xử lý data.",
                    TeacherId = teacher.Id, CategoryId = c3, Price = 450000, DiscountPrice = 299000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c3/Python-logo-notext.svg", Level = CourseLevel.Beginner, Status = CourseStatus.Published, IsFeatured = true
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "ML201", Title = "Machine Learning Nền Tảng", Slug = "machine-learning-nen-tang",
                    Description = "Áp dụng thuật toán học máy giải quyết các bài toán phân loại và dự đoán, sử dụng Scikit-Learn.",
                    TeacherId = teacher.Id, CategoryId = c4, Price = 1500000, DiscountPrice = 1200000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/0/05/Scikit_learn_logo_small.svg", Level = CourseLevel.Intermediate, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "DOCKER101", Title = "Docker & CI/CD Dành Cho Dev", Slug = "docker-ci-cd-danh-cho-dev",
                    Description = "Đóng gói ứng dụng với Docker, viết Dockerfile, chạy container, cơ bản Kubernetes và setup pipelines với GitHub Actions.",
                    TeacherId = teacher.Id, CategoryId = c5, Price = 700000, DiscountPrice = 550000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/7/79/Docker_%28container_engine%29_logo.png", Level = CourseLevel.Intermediate, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "AWS202", Title = "Amazon Web Services Thực Chiến", Slug = "amazon-web-services-thuc-chien",
                    Description = "Deploy dự án lên AWS, học cách dùng EC2, S3, RDS, và VPC cơ bản. Tổng quan về serverless với Lambda.",
                    TeacherId = teacher.Id, CategoryId = c5, Price = 2000000, DiscountPrice = 1500000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Amazon_Web_Services_Logo.svg", Level = CourseLevel.Advanced, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "FIGMA101", Title = "Thiết kế UI/UX với Figma", Slug = "thiet-ke-ui-ux-voi-figma",
                    Description = "Làm quen với Figma, nguyên lý thiết kế đồ họa, auto-layout, component và quy trình tạo ra một wireframe lên mockup hoàn chỉnh.",
                    TeacherId = teacher.Id, CategoryId = c6, Price = 0, DiscountPrice = 0,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/3/33/Figma-logo.svg", Level = CourseLevel.Beginner, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "SQL101", Title = "SQL Căn Bản - Chinh phục cơ sở dữ liệu", Slug = "sql-can-ban-chinh-phuc-database",
                    Description = "Học cách viết truy vấn SQL hiệu quả, đánh index, join, subquery, và các khái niệm về Transaction hay Normalization.",
                    TeacherId = teacher.Id, CategoryId = c7, Price = 300000, DiscountPrice = 199000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/8/87/Sql_data_base_with_logo.png", Level = CourseLevel.Beginner, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "UNITY101", Title = "Làm Game 2D với Unity", Slug = "lam-game-2d-voi-unity",
                    Description = "Làm game đầu tiên của bạn với engine phổ biến nhất thế giới: Unity và ngôn ngữ C#.",
                    TeacherId = teacher.Id, CategoryId = c8, Price = 850000, DiscountPrice = 650000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/1/19/Unity_Technologies_logo.svg", Level = CourseLevel.Intermediate, Status = CourseStatus.Published
                },
                new Course
                {
                    CourseId = Guid.NewGuid(), CourseCode = "TEST101", Title = "Automation Testing Với Selenium", Slug = "automation-testing-voi-selenium",
                    Description = "Kiểm thử ứng dụng web tự động. Viết kịch bản test case và chạy tự động với trình duyệt ảo.",
                    TeacherId = teacher.Id, CategoryId = c9, Price = 650000, DiscountPrice = 450000,
                    ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d5/Selenium_Logo.png", Level = CourseLevel.Intermediate, Status = CourseStatus.Published
                }
            };
            db.Courses.AddRange(courses);
            await db.SaveChangesAsync();

            // Seed nhiều Section & Lesson cho mỗi khóa học
            foreach (var course in courses)
            {
                for (int i = 1; i <= 3; i++) // Mỗi khóa học có 3 phần
                {
                    var section = new Section
                    {
                        CourseId = course.CourseId,
                        Title = $"Phần {i}: Kiến thức trọng tâm {(i == 1 ? "mở đầu" : (i == 2 ? "cốt lõi" : "nâng cao"))}",
                        OrderIndex = i
                    };
                    db.Sections.Add(section);
                    await db.SaveChangesAsync();

                    for (int j = 1; j <= 4; j++) // Mỗi phần có 4 bài học
                    {
                        var lesson = new Lesson
                        {
                            SectionId = section.SectionId,
                            Title = $"Bài {j}: Nội dung bài học số {j} của khóa",
                            LessonType = (j % 2 == 0) ? LessonType.Reading : LessonType.Video,
                            OrderIndex = j,
                            IsPreview = (i == 1 && j == 1), // Chỉ cho học thử bài đầu tiên phần 1
                            Content = (j % 2 == 0) ? $"<h2>Nội dung bài học {j}</h2><p>Đây là nội dung lý thuyết chi tiết hướng dẫn các bạn làm chủ kĩ năng của bài học này.</p>" : null,
                            // Mô phỏng video (sẽ không chạy được thực tế nếu ko có file, nhưng lưu đủ các trường)
                            VideoOriginalFileName = (j % 2 != 0) ? "video_demo.mp4" : null,
                            VideoStatus = (j % 2 != 0) ? VideoStatus.Ready : null,
                            VideoDurationSeconds = (j % 2 != 0) ? 360 : null 
                        };
                        db.Lessons.Add(lesson);
                    }
                }
            }
            await db.SaveChangesAsync();
        }
    }
}