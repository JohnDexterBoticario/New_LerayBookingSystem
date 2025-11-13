using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    // Corresponds to the 'appointment_services' table
    public class AppointmentService
    {
        // Primary Key (Corresponds to AppointmentServiceID in your SQL)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentServiceID { get; set; } 

        // Foreign Key to the Appointment table
        public int AppointmentId { get; set; }
        
        // Navigation property for the Appointment
        public Appointment Appointment { get; set; } = null!;

        // Foreign Key to the Service table (This is the one that caused the original error)
        public int ServiceId { get; set; } 

        // Navigation property for the Service/CustomerService
        // Use the model name you are using for services (CustomerService based on your controller)
        public CustomerService Service { get; set; } = null!;

        // Other required data fields from your table definition
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }
        
        public int Quantity { get; set; } = 1;
    }
}