// wwwroot/js/adminCalendar.js
document.addEventListener('DOMContentLoaded', function () {
    const calendarEl = document.getElementById('admin-calendar');

    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        navLinks: true,
        selectable: true,
        editable: true,
        eventClick: function(info) {
            openEditModal(info.event);
        },
        eventMouseEnter: function(info) {
            // optional: show tooltip on hover (use tippy.js or title)
            info.el.title = info.event.title;
        },
        events: {
            url: '/api/Apiappointments',
            method: 'GET'
        },
        eventDrop: async function(info) {
            // If event dragged to a new date/time, save via API
            const ev = info.event;
            await saveAppointmentTime(ev.id, ev.start, ev.end);
            // optionally show success
        }
    });

    calendar.render();

    // quick view buttons (month/week/day)
    document.getElementById('btn-month').addEventListener('click', () => calendar.changeView('dayGridMonth'));
    document.getElementById('btn-week').addEventListener('click', () => calendar.changeView('timeGridWeek'));
    document.getElementById('btn-day').addEventListener('click', () => calendar.changeView('timeGridDay'));
    document.getElementById('calendar-today').addEventListener('click', () => calendar.today());

    // open and populate modal
    async function openEditModal(event) {
        // fetch full appointment details
        try {
            const res = await axios.get('/api/appointments/' + event.id);
            const appt = res.data;

            document.getElementById('edit-appointment-id').value = appt.id;
            document.getElementById('edit-service-name').value = appt.serviceName || '';
            document.getElementById('edit-start').value = appt.start ? toLocalDatetimeInput(appt.start) : '';
            document.getElementById('edit-end').value = appt.end ? toLocalDatetimeInput(appt.end) : '';
            document.getElementById('edit-total-price').value = appt.totalPrice || '';
            document.getElementById('edit-notes').value = appt.notes || '';
            document.getElementById('edit-status').value = appt.status || 'Pending';

            if (appt.client) {
                document.getElementById('edit-client-name').value = appt.client.name || '';
                document.getElementById('edit-client-email').value = appt.client.email || '';
                document.getElementById('edit-client-phone').value = appt.client.phone || '';
            } else {
                document.getElementById('edit-client-name').value = '';
                document.getElementById('edit-client-email').value = '';
                document.getElementById('edit-client-phone').value = '';
            }

            // show modal (Bootstrap 5)
            var modal = new bootstrap.Modal(document.getElementById('editAppointmentModal'));
            modal.show();
        } catch (err) {
            console.error('Failed to load appointment', err);
            alert('Could not load appointment details.');
        }
    }

    // Submit edit form
    document.getElementById('editAppointmentForm').addEventListener('submit', async function (e) {
        e.preventDefault();
        const id = document.getElementById('edit-appointment-id').value;

        const payload = {
            startTime: fromLocalInputToISO(document.getElementById('edit-start').value),
            endTime: document.getElementById('edit-end').value ? fromLocalInputToISO(document.getElementById('edit-end').value) : null,
            serviceName: document.getElementById('edit-service-name').value,
            totalPrice: parseFloat(document.getElementById('edit-total-price').value || 0),
            notes: document.getElementById('edit-notes').value,
            status: document.getElementById('edit-status').value,
            client: {
                id: parseInt(document.getElementById('edit-client-id') ? document.getElementById('edit-client-id').value : 0),
                name: document.getElementById('edit-client-name').value,
                email: document.getElementById('edit-client-email').value,
                phone: document.getElementById('edit-client-phone').value
            }
        };

        try {
            await axios.put('/api/appointments/' + id, payload);
            // refresh calendar events
            calendar.refetchEvents();
            // hide modal
            bootstrap.Modal.getInstance(document.getElementById('editAppointmentModal')).hide();
            alert('Appointment updated');
        } catch (err) {
            console.error(err);
            alert('Failed to update appointment');
        }
    });

    // Helper: when event dragged, save time only
    async function saveAppointmentTime(id, start, end) {
        const payload = {
            startTime: start ? start.toISOString() : null,
            endTime: end ? end.toISOString() : null
        };
        await axios.put('/api/appointments/' + id, payload);
    }

    // Helpers: convert ISO <-> input[type=datetime-local] value (local)
    function toLocalDatetimeInput(isoString) {
        const d = new Date(isoString);
        const tzOffset = d.getTimezoneOffset() * 60000;
        const local = new Date(d.getTime() - tzOffset);
        return local.toISOString().slice(0,16);
    }
    function fromLocalInputToISO(localVal) {
        if (!localVal) return null;
        // localVal is like "2025-01-06T13:00"
        const dt = new Date(localVal);
        return dt.toISOString();
    }

    // Delete button
    const delBtn = document.getElementById('delete-appointment-btn');
    if (delBtn) {
        delBtn.addEventListener('click', async function () {
            if (!confirm('Delete appointment?')) return;
            const id = document.getElementById('edit-appointment-id').value;
            try {
                await axios.delete('/api/appointments/' + id);
                calendar.refetchEvents();
                bootstrap.Modal.getInstance(document.getElementById('editAppointmentModal')).hide();
            } catch (err) {
                console.error(err);
                alert('Failed to delete');
            }
        });
    }
});
