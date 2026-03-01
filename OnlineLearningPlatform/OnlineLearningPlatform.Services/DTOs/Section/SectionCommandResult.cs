using SectionEntity = OnlineLearningPlatform.Models.Entities.Section;

namespace OnlineLearningPlatform.Services.DTOs.Section
{
    public class SectionCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SectionEntity? Section { get; set; }
    }
}
