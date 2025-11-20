using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockFingerprintResponse
    {
        public List<TTLockFingerprintDto> List { get; set; } = new List<TTLockFingerprintDto>();
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }
}
