using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockRecordDto
    {
        public long RecordId { get; set; }
        public int LockId { get; set; }
        public int RecordTypeFromLock { get; set; }
        public int RecordType { get; set; } 
        public int Success { get; set; } // 1-Yes, 0-No
        public string? Username { get; set; }
        public string? KeyboardPwd { get; set; } 
        public long LockDate { get; set; } 
        public long ServerDate { get; set; }
    }
}
