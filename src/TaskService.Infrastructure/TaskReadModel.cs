using System.ComponentModel.DataAnnotations;

namespace TaskService.Infrastructure
{
    public class TaskReadModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
