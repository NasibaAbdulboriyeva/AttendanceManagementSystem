
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class UploadRequestDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
