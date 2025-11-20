using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockIcCardResponse
    {
        public List<TTLockIcCardDto> List { get; set; } = new List<TTLockIcCardDto>();
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }
}
