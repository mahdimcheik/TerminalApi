namespace TerminalApi.Models
{
    public class ActivitiesTeacher
    {
        public List<OrderResponseForTeacherDTO> OrdersOfTheWeek { get; set; } = new List<OrderResponseForTeacherDTO>();
        public List<BookingResponseDTO> BookingsOftheWeek { get; set; } = new List<BookingResponseDTO>();    
        public List<UserResponseDTO> NewStudents { get; set; } = new List<UserResponseDTO>();
    }

    public class ActivitiesStudent
    {
        public List<OrderResponseForTeacherDTO> OrdersHistory { get; set; } = new List<OrderResponseForTeacherDTO>();
        public List<BookingResponseDTO> BookingsOftheWeek { get; set; } = new List<BookingResponseDTO>();
    }
}
