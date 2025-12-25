

using System.Text.Json.Serialization;

namespace AttendanceManagementSystem.Application.DTOs
{
    
        /// <summary>
        /// TTLock API'dan keladigan har qanday sahifalangan (paginated) ro'yxat javobining umumiy (generic) tuzilmasi.
        /// </summary>
        /// <typeparam name="T">Ro'yxat ichidagi ma'lumot ob'ektining turi (masalan, TTLockIcCardDto).</typeparam>
        public class TTLockGenericResponse<T>
        {
            [JsonPropertyName("list")]
            public List<T>? List { get; set; } = new List<T>();

            [JsonPropertyName("pageNo")]
            public int PageNo { get; set; }

            [JsonPropertyName("pageSize")]
            public int PageSize { get; set; }

            [JsonPropertyName("pages")]
            public int Pages { get; set; }

            [JsonPropertyName("total")]
            public int Total { get; set; }

           
        }
    }
