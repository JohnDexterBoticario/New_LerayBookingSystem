namespace New_LeRayBookingSystem.ViewModels
{
    public class DashboardViewModel
    {
        // Old appointment statistics (for dashboard cards + chart)
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public List<AppointmentByMonth> AppointmentsByMonth { get; set; } = new();

        // Sales / Revenue
        public decimal TotalSales { get; set; }
        public List<SalesDataPoint> SalesOverTime { get; set; } = new();

        // Best services
        public List<ServicePerformance> TopServices { get; set; } = new();

        // Upcoming clients
        public List<UpcomingAppointment> UpcomingAppointments { get; set; } = new();

        public List<RecentAppointment> RecentAppointments { get; set; } = new();
    }

    public class AppointmentByMonth
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SalesDataPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class ServicePerformance
    {
        public string ServiceName { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public double Percentage { get; set; }
    }

    public class UpcomingAppointment
    {
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
    public class RecentAppointment
    {
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime? Date { get; set; }   // ← make it nullable
        public string Status { get; set; } = string.Empty;
    }

}
