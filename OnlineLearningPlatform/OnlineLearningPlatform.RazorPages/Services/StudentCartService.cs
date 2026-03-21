using System.Text.Json;

namespace OnlineLearningPlatform.RazorPages.Services
{
    /// <summary>
    /// Giỏ khóa học trong session (chưa thanh toán).
    /// </summary>
    public class StudentCartService
    {
        private const string SessionKey = "EduLearnStudentCart";
        private readonly IHttpContextAccessor _http;

        public StudentCartService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ISession? Session => _http.HttpContext?.Session;

        public IReadOnlyList<Guid> GetCourseIds()
        {
            if (Session == null) return Array.Empty<Guid>();
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return Array.Empty<Guid>();
            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
            }
            catch
            {
                return Array.Empty<Guid>();
            }
        }

        private void Save(List<Guid> ids)
        {
            if (Session == null) return;
            var distinct = ids.Distinct().Take(30).ToList();
            Session.SetString(SessionKey, JsonSerializer.Serialize(distinct));
        }

        public int Count => GetCourseIds().Count;

        public void AddCourse(Guid courseId)
        {
            var list = GetCourseIds().ToList();
            if (!list.Contains(courseId))
                list.Add(courseId);
            Save(list);
        }

        public void RemoveCourse(Guid courseId)
        {
            var list = GetCourseIds().ToList();
            list.Remove(courseId);
            Save(list);
        }

        public void Clear()
        {
            Session?.Remove(SessionKey);
        }
    }
}
