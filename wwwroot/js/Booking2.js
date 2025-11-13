/**
 * LeRay Booking System
 * booking.js
 *
 * Handles fetching services, checking availability, and creating bookings.
 */

document.addEventListener("DOMContentLoaded", () => {
    // Check if on booking page
    const bookingForm = document.getElementById("booking-form");
    if (bookingForm) {
        // Check login state first
        if (!isLoggedIn()) {
            // User is not logged in, redirect
            alert("Please log in to make a booking.");
            window.location.href = `/login.html?redirect=${window.location.pathname}`;
            return;
        }

        loadServices();
        bookingForm.addEventListener("submit", handleBookingSubmit);
    }

    // Check if on "My Bookings" page
    const myBookingsList = document.getElementById("my-bookings-list");
    if (myBookingsList) {
        if (!isLoggedIn()) {
            window.location.href = `/login.html?redirect=${window.location.pathname}`;
            return;
        }
        loadMyBookings();
    }
});

/**
 * Fetches services from the API and populates the dropdown.
 */
async function loadServices() {
    const serviceSelect = document.getElementById("service-select");
    if (!serviceSelect) return;

    try {
        const response = await fetch("/api/services");
        if (!response.ok) {
            throw new Error("Could not load services.");
        }
        const services = await response.json();

        // Clear existing options (except placeholder)
        serviceSelect.innerHTML = '<option value="" disabled selected>Select a Service</option>';

        services.forEach(service => {
            const option = document.createElement("option");
            option.value = service.id;
            option.textContent = `${service.name} (${service.durationInMinutes} min) - $${service.price}`;
            option.dataset.duration = service.durationInMinutes; // Store duration
            serviceSelect.appendChild(option);
        });

    } catch (err) {
        console.error("Error loading services:", err);
        serviceSelect.innerHTML = '<option value="" disabled>Error loading services</option>';
    }
}

/**
 * Handles the booking form submission.
 */
async function handleBookingSubmit(event) {
    event.preventDefault();
    const form = event.target;
    const errorEl = document.getElementById("booking-error");
    const successEl = document.getElementById("booking-success");
    errorEl.textContent = "";
    successEl.textContent = "";

    const serviceId = form.service.value;
    const date = form.date.value;
    const time = form.time.value;
    const notes = form.notes.value;
    const token = getAuthToken();

    if (!token) {
        handleLogout(); // Token is missing, force logout
        return;
    }

    if (!serviceId || !date || !time) {
        errorEl.textContent = "Please select a service, date, and time.";
        return;
    }

    // Combine date and time into a single ISO 8601 string
    const startTime = new Date(`${date}T${time}`).toISOString();

    const bookingData = {
        serviceId: parseInt(serviceId),
        startTime: startTime,
        notes: notes
    };

    try {
        const response = await fetch("/api/bookings", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(bookingData)
        });

        if (!response.ok) {
            if (response.status === 401) {
                // Unauthorized
                alert("Your session has expired. Please log in again.");
                handleLogout();
            }
            if (response.status === 409) {
                // Conflict - slot taken
                throw new Error("This time slot is no longer available. Please select another time.");
            }
            throw new Error("Booking failed. Please try again.");
        }

        const newBooking = await response.json();
        successEl.textContent = `Booking confirmed! Your appointment for ${newBooking.service.name} is at ${new Date(newBooking.startTime).toLocaleString()}.`;
        form.reset();
        
        // Optional: redirect to "My Bookings" page
        // window.location.href = "/my-bookings.html";

    } catch (err) {
        errorEl.textContent = err.message;
        console.error("Booking error:", err);
    }
}

/**
 * Fetches and displays the user's upcoming bookings.
 */
async function loadMyBookings() {
    const listEl = document.getElementById("my-bookings-list");
    const loadingEl = document.getElementById("bookings-loading");
    const token = getAuthToken();

    if (!token) {
        handleLogout();
        return;
    }

    try {
        loadingEl.style.display = "block";
        listEl.innerHTML = "";

        const response = await fetch("/api/bookings/my-bookings", {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!response.ok) {
            if (response.status === 401) {
                alert("Session expired. Please log in.");
                handleLogout();
            }
            throw new Error("Could not fetch bookings.");
        }

        const bookings = await response.json();
        loadingEl.style.display = "none";

        if (bookings.length === 0) {
            listEl.innerHTML = "<li>You have no upcoming bookings.</li>";
            return;
        }

        bookings.forEach(booking => {
            const li = document.createElement("li");
            li.className = "booking-item"; // Add a class for styling
            li.innerHTML = `
                <h4>${booking.service.name}</h4>
                <p><strong>When:</strong> ${new Date(booking.startTime).toLocaleString()}</p>
                <p><strong>Duration:</strong> ${booking.service.durationInMinutes} minutes</p>
                <p><strong>Status:</strong> ${booking.status}</p>
                <button class="cancel-btn" data-id="${booking.id}">Cancel</button>
            `;
            listEl.appendChild(li);
        });

        // Add event listeners for cancel buttons
        listEl.querySelectorAll('.cancel-btn').forEach(btn => {
            btn.addEventListener('click', handleCancelBooking);
        });

    } catch (err) {
        loadingEl.style.display = "none";
        listEl.innerHTML = "<li>Error loading bookings.</li>";
        console.error("Error fetching bookings:", err);
    }
}

/**
 * Handles the cancellation of a booking.
 */
async function handleCancelBooking(event) {
    const button = event.target;
    const bookingId = button.dataset.id;
    const token = getAuthToken();

    if (!confirm("Are you sure you want to cancel this booking?")) {
        return;
    }
    
    try {
        const response = await fetch(`/api/bookings/${bookingId}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!response.ok) {
             if (response.status === 401) {
                alert("Session expired. Please log in.");
                handleLogout();
            }
            throw new Error("Could not cancel booking.");
        }
        
        // Success
        alert("Booking cancelled successfully.");
        loadMyBookings(); // Refresh the list

    } catch(err) {
        alert(err.message);
        console.error("Error cancelling booking:", err);
    }
}
