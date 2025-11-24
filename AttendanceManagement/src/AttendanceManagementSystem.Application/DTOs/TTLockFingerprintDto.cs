using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockFingerprintDto
    {
        public int FingerprintId { get; set; } 
        public int LockId { get; set; }
        public string? FingerprintNumber { get; set; } 
        public int FingerprintType { get; set; } 
        public string? FingerprintName { get; set; } 
        public long StartDate { get; set; }
        public long EndDate { get; set; } 
        public long CreateDate { get; set; } 
       
    }
}
