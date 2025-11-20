using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;
using ClosedXML.Excel;
using System.Globalization;
namespace AttendanceManagementSystem.Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAttendanceLogRepository _logRepo;
        public UploadService(IEmployeeRepository employeeRepo, IAttendanceLogRepository logRepo)
        {
            _employeeRepo = employeeRepo;
            _logRepo = logRepo;
        }

        public async Task<int> UploadAttendanceLogAsync(UploadRequestDto file)
        {
            if (file == null || file.File.Length == 0)
            {
                throw new ArgumentException("Fayl topilmadi yoki bo'sh.");
            }

            var logsToSave = new List<AttendanceLog>();
            var employeeCache = new Dictionary<string, Employee>();

            using (var stream = file.File.OpenReadStream())
            {
                IXLWorkbook workbook;
                try
                {
                    workbook = new XLWorkbook(stream);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Fayl ClosedXML tomonidan o'qilmadi. Fayl formatini tekshiring (Faqat XLSX tavsiya etiladi).", ex);
                }

                IXLWorksheet worksheet = workbook.Worksheet(1);

                if (worksheet == null)
                {
                    throw new ArgumentException("Excel faylida 1-list topilmadi.");
                }

                var lastRow = worksheet.LastRowUsed();

                if (lastRow == null)
                {
                    return 0;
                }

                int rowCount = lastRow.RowNumber();
                int rowStart = 2;

                for (int rowNum = rowStart; rowNum <= rowCount; rowNum++)
                {
                    IXLRow row = worksheet.Row(rowNum);

                    var rawLog = new RawLogDto
                    {
                        Username = row.Cell(1).GetString().Trim(),
                        StatusText = row.Cell(3).GetString().Trim(),
                        DateText = row.Cell(4).GetString().Trim()
                    };

                    if (rawLog.Username == "Датчик двери" || string.IsNullOrWhiteSpace(rawLog.Username))
                    {
                        continue;
                    }

                    var icNumber = XlsxMappingHelper.ExtractICNumber(rawLog.Username);
                    if (icNumber == null)
                    {
                        continue;
                    }

                    Employee employee = null;


                    if (employeeCache.ContainsKey(icNumber))
                    {
                        employee = employeeCache[icNumber];
                    }
                    else
                    {
                        employee = await _employeeRepo.GetEmployeeByICCodeAsync(icNumber);

                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                Code = icNumber,
                                FullName = rawLog.Username.Split('-', 2).Last().Trim(),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            };
                            await _employeeRepo.AddEmployeeAsync(employee);
                        }
                        employeeCache[icNumber] = employee;
                    }

                    DateTime recordedTime;
                    IXLCell dateCell = row.Cell(4);

                    if (dateCell.DataType == XLDataType.DateTime || dateCell.DataType == XLDataType.Number)
                    {
                        try
                        {
                            recordedTime = dateCell.GetDateTime();
                        }
                        catch (Exception)
                        {
                            if (!DateTime.TryParse(rawLog.DateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out recordedTime))
                            {
                                continue;
                            }
                        }
                    }
                    else if (!DateTime.TryParse(rawLog.DateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out recordedTime))
                    {
                        // Agar cell tipi na DateTime, na Number bo'lsa, uni string sifatida tahlil qilishga urinish
                        continue;
                    }
                    // .....

                    if (employee != null)
                    {
                        logsToSave.Add(new AttendanceLog
                        {
                            EmployeeId = employee.EmployeeId,
                            RecordedTime = recordedTime,
                            RawUsername = rawLog.Username,
                            Status = rawLog.StatusText == "Успешно" ? AttendanceStatus.Success : AttendanceStatus.Unknown,
                            CreatedAt = DateTime.Now,
                        });
                    }
                }
            }

            await _logRepo.AddLogsAsync(logsToSave);
            return logsToSave.Count;
        }

        public async Task<int> UploadScheduleAsync(UploadRequestDto file)
        {
            if (file == null || file.File.Length == 0)
            {
                throw new ArgumentException("File not found or no data");
            }

            var schedulesToSave = new List<EmployeeSchedule>();
            var employeeCache = new Dictionary<string, Employee>();

            using (var stream = file.File.OpenReadStream())
            {
                IXLWorkbook workbook;
                try
                {
                    workbook = new XLWorkbook(stream);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Fayl ClosedXML tomonidan o'qilmadi. Fayl formatini tekshiring (faqat XLSX tavsiya etiladi).", ex);
                }


                IXLWorksheet worksheet = workbook.Worksheet(1);

                if (worksheet == null)
                {
                    throw new ArgumentException("Excel faylida 1-list topilmadi.");
                }

                var lastRow = worksheet.LastRowUsed();

                if (lastRow == null)
                {
                    return 0;
                }

                int rowCount = lastRow.RowNumber();
                int rowStart = 2;

                for (int rowNum = rowStart; rowNum <= rowCount; rowNum++)
                {
                    IXLRow row = worksheet.Row(rowNum);


                    var rawSchedule = new
                    {
                        Username = row.Cell(2).GetString().Trim(), // F.I.O. сотрудника
                        StartTimeText = row.Cell(3).GetString().Trim() // Время прихода
                    };

                    if (string.IsNullOrWhiteSpace(rawSchedule.Username))
                    {
                        continue;
                    }

                    var icNumber = XlsxMappingHelper.ExtractICNumber(rawSchedule.Username);
                    if (icNumber == null)
                    {
                        continue;
                    }
                    var employee = new Employee();
                    if (employeeCache.ContainsKey(icNumber))
                    {
                        employee = employeeCache[icNumber];
                    }
                    else
                    {
                        employee = await _employeeRepo.GetEmployeeByICCodeAsync(icNumber);

                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                Code = icNumber,
                                FullName = rawSchedule.Username.Split('-', 2).Last().Trim(),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            };
                            await _employeeRepo.AddEmployeeAsync(employee);
                        }
                        employeeCache[icNumber] = employee;
                    }

                    TimeSpan startTime;
                    IXLCell timeCell = row.Cell(3);

                    if (timeCell.DataType == XLDataType.TimeSpan)
                    {
                        startTime = timeCell.GetTimeSpan();
                    }
                    else if (!TimeSpan.TryParse(rawSchedule.StartTimeText, CultureInfo.InvariantCulture, out startTime))
                    {

                        continue;
                    }

                    if (employee != null)
                    {

                        schedulesToSave.Add(new EmployeeSchedule
                        {
                            EmployeeId = employee.EmployeeId,
                            StartTime = startTime,
                            CreatedAt = DateTime.Now,

                        });
                    }
                }
            }


            await _employeeRepo.AddRangeEmployeeScheduleAsync(schedulesToSave);


            return schedulesToSave.Count;
        }


    }

}