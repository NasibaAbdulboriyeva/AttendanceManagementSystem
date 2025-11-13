using System.ComponentModel;
namespace AttendanceManagementSystem.Domain.Entities

{
    public enum AttendanceStatus
    {
        [Description("Успешно")]
        Success = 0,

        [Description("Дверь открыта")]
        DoorOpened = 1,

        [Description("Дверь закрыта")]
        DoorClosed = 2,

        [Description("Неизвестный статус")]
        Unknown = 99
    }
}
