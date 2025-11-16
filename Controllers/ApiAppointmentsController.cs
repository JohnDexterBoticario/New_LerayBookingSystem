using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiAppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _audit;

        public ApiAppointmentsController(ApplicationDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // GET: api/ApiAppointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            var appointments = await _context.Appointments.ToListAsync();

            await _audit.LogAsync(
                action: "Read",
                entityType: "Appointment",
                entityId: "N/A",
                details: "Fetched all appointments",
                module: "Appointments",
                description: "User fetched all appointment records"
            );

            return appointments;
        }

        // GET: api/ApiAppointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            await _audit.LogAsync(
                action: "Read",
                entityType: "Appointment",
                entityId: id.ToString(),
                details: appointment == null
                    ? "Appointment not found"
                    : "Fetched appointment details",
                module: "Appointments",
                description: appointment == null
                    ? $"Appointment ID {id} was requested but not found"
                    : $"Fetched details for appointment ID {id}"
            );

            if (appointment == null)
                return NotFound();

            return appointment;
        }

        // PUT: api/ApiAppointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Appointment updated)
        {
            if (id != updated.Id)
                return BadRequest("ID mismatch");

            _context.Entry(updated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                await _audit.LogAsync(
                    action: "Update",
                    entityType: "Appointment",
                    entityId: id.ToString(),
                    details: "Appointment updated",
                    module: "Appointments",
                    description: $"Appointment ID {id} was updated"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Appointments.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/ApiAppointments
        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                action: "Create",
                entityType: "Appointment",
                entityId: appointment.Id.ToString(),
                details: "New appointment created",
                module: "Appointments",
                description: $"Created new appointment with ID {appointment.Id}"
            );

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
        }

        // DELETE: api/ApiAppointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                await _audit.LogAsync(
                    action: "DeleteFailed",
                    entityType: "Appointment",
                    entityId: id.ToString(),
                    details: "Attempted to delete appointment but it was not found",
                    module: "Appointments",
                    description: $"Delete failed: Appointment ID {id} not found"
                );

                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                action: "Delete",
                entityType: "Appointment",
                entityId: id.ToString(),
                details: "Appointment deleted",
                module: "Appointments",
                description: $"Appointment ID {id} was deleted"
            );

            return NoContent();
        }
    }
}
