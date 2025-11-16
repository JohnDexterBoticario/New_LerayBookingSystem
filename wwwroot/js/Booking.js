// ===== Booking.js (Handles Multi-Step Flow and API Submission) =====
document.addEventListener("DOMContentLoaded", () => {
    // --- Data Fields (Hidden Inputs - Must match C# DTO properties) ---
    const serviceIdInput = document.getElementById('booking-service-id');
    const totalPriceInput = document.getElementById('booking-total-price');
    const notesInput = document.getElementById('booking-notes');
    const dateStringInput = document.getElementById('booking-date-string');
    const timeSlotStringInput = document.getElementById('booking-time-slot-string');
    // Reference to the new Payment Method input (from CSHTML fix)
    const paymentMethodInput = document.getElementById('booking-payment-method');

    // --- Setup Elements and Variables ---
    const steps = document.querySelectorAll(".step");
    const stepIndicators = document.querySelectorAll(".steps li");
    const btns = {
        continue1: document.getElementById("continue1"),
        continue2: document.getElementById("continue2"),
        continue3: document.getElementById("continue3"),
        back2: document.getElementById("back2"),
        back3: document.getElementById("back3"),
        back4: document.getElementById("back4"),
        finish: document.getElementById("finish"),
    };

    let currentStep = 0;
    let selectedServices = [];
    let totalPrice = 0;
    let selectedDate = "";
    let selectedTime = "";
    // Removed bundleServiceId as it's not strictly necessary here

    // --- Core Functions ---
    function showStep(index) {
        steps.forEach((step, i) => step.classList.toggle("active", i === index));
        stepIndicators.forEach((li, i) => {
            li.classList.toggle("active", i === index);
            if (i < index) li.classList.add("completed");
            else li.classList.remove("completed");
        });
        currentStep = index;
    }

    /**
     * FIX 1: Correctly extracts Service ID from value and Service Name from the label text.
     */
    function updateSelections() {
        const checkedBoxes = document.querySelectorAll('input[name="services[]"]:checked');
        selectedServices = [];
        totalPrice = 0;

        checkedBoxes.forEach((box) => {
            const id = box.value; // ID is correctly stored in the value attribute
            const price = parseFloat(box.dataset.price || 0);

            // Get the text content from the associated <label>
            const labelElement = document.querySelector(`label[for="${box.id}"]`);
            
            // Extract the name, stripping off the price at the end (e.g., "Classic Look – ₱699")
            let nameText = 'Service Name Missing';
            if (labelElement) {
                // Split by the dash, take the first part, and trim whitespace
                nameText = labelElement.textContent.split('–')[0].trim();
            }
            
            totalPrice += price;
            // Pushing the correct structure: { id: 55, name: "Classic Look", price: 699 }
            selectedServices.push({ id, name: nameText, price }); 
        });
    }

    /**
     * FIX 2: Uses the correctly extracted service.name to display in the summary list.
     */
    function updateBundleDisplay() {
        const bundleList = document.getElementById("bundle-list");
        bundleList.innerHTML = "";

        if (selectedServices.length === 0) {
            bundleList.innerHTML =
                '<li class="empty-message" style="padding: 15px; text-align: center; color: #888;">No services selected</li>';
            return;
        }

        selectedServices.forEach((service, index) => {
            const li = document.createElement("li");
            li.className = "bundle-item";
            li.innerHTML = `
                <div class="bundle-number">${index + 1}</div>
                <span class="service-name-display">${service.name}</span>
            `;
            bundleList.appendChild(li);
        });
    }

    function updateConfirmationUI() {
        // Creates a clean, formatted list of all selected services for display
        const serviceNames = selectedServices.map((s) => `${s.name} (₱${s.price})`).join(", ");
        const totalDown = (totalPrice * 0.2).toFixed(2);

        document.getElementById("confirm-service").textContent = serviceNames;
        document.getElementById("confirm-date").textContent = selectedDate;
        document.getElementById("confirm-time").textContent = selectedTime;
        document.getElementById("confirm-price").textContent = `₱${totalPrice.toFixed(2)}`;
        document.getElementById("confirm-downpayment").textContent = `₱${totalDown}`;

        document.getElementById("payment-service").textContent = serviceNames;
        document.getElementById("payment-date").textContent = selectedDate;
        document.getElementById("payment-time").textContent = selectedTime;
        document.getElementById("payment-final-price").textContent = `₱${totalPrice.toFixed(2)}`;
        document.getElementById("payment-final-downpayment").textContent = `₱${totalDown}`;
    }

    // --- Service Selection Logic ---
    const serviceSelect = document.getElementById("service-select");
    const allCategories = document.querySelectorAll(".service-category");

    if (serviceSelect) {
        serviceSelect.addEventListener("change", (e) => {
            const selectedValue = e.target.value;
            allCategories.forEach(category => {
                if (!selectedValue || category.dataset.category === selectedValue)
                    category.style.display = "block";
                else category.style.display = "none";
            });
        });
    }

    document.querySelectorAll(".category-checkbox-item input[type='checkbox']").forEach(checkbox => {
        checkbox.addEventListener("change", (e) => {
            const categoryId = e.target.dataset.category;
            const serviceCategory = document.getElementById(`category-${categoryId}`);

            if (serviceCategory) {
                if (e.target.checked) serviceCategory.classList.add("active");
                else {
                    serviceCategory.classList.remove("active");
                    // Uncheck services when the category bundle is unchecked
                    serviceCategory.querySelectorAll('input[type="checkbox"][name="services[]"]').forEach(box => box.checked = false);
                }
            }

            updateSelections();
            updateBundleDisplay();
        });
    });

    document.querySelectorAll('input[name="services[]"]').forEach((box) => {
        box.addEventListener("change", () => {
            updateSelections();
            updateBundleDisplay();
        });
    });

    // --- Step Navigation ---

    /**
     * FIX 3: Ensures ServiceId is set to the Aggregate placeholder (81) 
     * and sends all details in the Notes field.
     */
    if (btns.continue1) {
        btns.continue1.addEventListener("click", () => {
            updateSelections();

            if (selectedServices.length === 0) {
                alert("⚠️ Please select at least one service.");
                return;
            }

            // Set single ServiceId to the Aggregate/Placeholder ID (e.g., 81)
            // This prevents the 'null' or 'undefined' error on the DTO.
            serviceIdInput.value = '81'; 
            
            totalPriceInput.value = totalPrice.toFixed(2);
            
            // Format and aggregate ALL service details (IDs, names, prices) for backend processing
            const serviceDetails = selectedServices.map(s => `ID:${s.id} Name:${s.name} Price:₱${s.price}`).join('; ');
            notesInput.value = `Aggregate Services: ${serviceDetails}`;

            showStep(1); // Navigates to Step 2 (Date & Time)
        });
    }

    if (btns.continue2) {
        btns.continue2.addEventListener("click", () => {
            const selectedSlotBtn = document.querySelector(".timeslots button.selected");

            if (!selectedDate || !selectedSlotBtn) {
                alert("⚠️ Please select a date and time first.");
                return;
            }

            // Set hidden inputs for DTO validation
            selectedTime = selectedSlotBtn.textContent.trim();
            dateStringInput.value = selectedDate;
            timeSlotStringInput.value = selectedTime;

            updateConfirmationUI();
            showStep(2); // Navigates to Step 3 (Confirmation)
        });
    }

    if (btns.continue3)
        btns.continue3.addEventListener("click", () => showStep(3)); // Navigates to Step 4 (Payment)
        
    if (btns.finish) {
        btns.finish.addEventListener("click", async (e) => {
            e.preventDefault();
    
            // 1. Client-Side File Check
            const receiptInput = document.getElementById("receiptFile");
            if (!receiptInput || !receiptInput.files.length) {
                alert("⚠️ Please upload a payment receipt file.");
                return;
            }
    
            const formData = new FormData(); 

            /*
            const token = localStorage.getItem('authToken');
    
            if (!token) {
                alert("⚠️ You must be logged in to complete the booking.");
                window.location.href = '/';
                return;
            }
            */
            // Manually Set All DTO Fields from hidden inputs
            if (serviceIdInput) {
                 formData.append("ServiceId", serviceIdInput.value);
            }
            if (totalPriceInput) {
                formData.append("TotalPrice", totalPriceInput.value);
            }
            if (notesInput) {
                formData.append("Notes", notesInput.value);
            }
            
            // Date/Time fields
            if (dateStringInput) {
                formData.append("AppointmentDateString", dateStringInput.value);
            }
            if (timeSlotStringInput) {
                formData.append("TimeSlotString", timeSlotStringInput.value);
            }
            
            // Payment Method field
            if (paymentMethodInput) {
                formData.append("PaymentMethod", paymentMethodInput.value);
            } else {
                formData.append("PaymentMethod", "Online Payment"); 
            }
    
            // Explicitly set the file
            formData.append("PaymentReceiptFile", receiptInput.files[0], receiptInput.files[0].name);
    
            try {
                const response = await fetch("/api/Bookings", {
                    method: "POST",
                    body: formData,
                    //headers: { "Authorization": `Bearer ${token}` }
                });
    
                if (response.ok) {
                    const result = await response.json();
                    alert("✅ Booking confirmed successfully! ID: " + result.id);
                    window.location.href = "/Home/Index";
                } else {
                    let errorMessage = "An unknown error occurred.";
                    const contentType = response.headers.get("content-type");
    
                    if (contentType && contentType.includes("application/json")) {
                        const errorJson = await response.json();
                        
                        if (errorJson.errors) {
                            errorMessage = "Validation Errors:\n" + 
                                Object.entries(errorJson.errors)
                                    .map(([key, msgs]) => `• ${key}: ${msgs.join(', ')}`)
                                    .join('\n');
                        } else if (errorJson.title) {
                            errorMessage = errorJson.title;
                        }
                    } else {
                        errorMessage = await response.text();
                    }
    
                    alert("⚠️ Booking failed: " + errorMessage);
                }
            } catch (err) {
                alert("⚠️ Network error while submitting booking.");
            }
        });
    }

    // --- Time Slot Selection Logic ---
    document.querySelectorAll(".timeslots button").forEach(btn => {
        btn.addEventListener("click", (e) => {
            e.preventDefault();
            document.querySelectorAll(".timeslots button").forEach(b => b.classList.remove("selected"));
            btn.classList.add("selected");
        });
    });

    if (btns.back2) btns.back2.addEventListener("click", () => showStep(0));
    if (btns.back3) btns.back3.addEventListener("click", () => showStep(1));
    if (btns.back4) btns.back4.addEventListener("click", () => showStep(2));

    showStep(0);
    updateBundleDisplay(); // Initialize the summary display

    // --- Date Picker Initialization ---
    if (typeof flatpickr !== "undefined") {
        flatpickr("#date-picker", {
            inline: true,
            dateFormat: "m/d/Y",
            minDate: "today",
            onChange: (selectedDates, dateStr) => { selectedDate = dateStr; },
        });
    }

    // --- File Upload Display (UI Feedback) ---
    const fileInput = document.getElementById('receiptFile');
    const fileNameDisplay = document.getElementById('file-name-display');

    if (fileInput && fileNameDisplay) {
        fileInput.addEventListener('change', function(event) {
            if (fileInput.files.length > 0) {
                const fileName = fileInput.files[0].name;
                fileNameDisplay.textContent = 'Attached: ' + fileName;
                fileNameDisplay.classList.add('file-attached');
            } else {
                fileNameDisplay.textContent = 'No file attached.';
                fileNameDisplay.classList.remove('file-attached');
            }
        });
    }
    
    // ===============================================================
    // 🕒 Disable Fully Booked & Past Time Slots 
    // ===============================================================
    (async function disableUnavailableTimeSlots() {
        const timeButtons = document.querySelectorAll(".timeslots button");
        if (!timeButtons.length) return;

        try {
            const response = await fetch("/api/BookingApi/GetSlotStatus");
            const slotData = await response.json();
            const now = new Date();

            timeButtons.forEach(btn => {
                const slotTimeText = btn.textContent.trim();
                const slotTimeMatch = slotTimeText.match(/(\d{1,2}:\d{2})\s(AM|PM)/); 

                if (slotTimeMatch) {
                    const [hour, minute] = slotTimeMatch[1].split(":").map(Number);
                    const ampm = slotTimeMatch[2];
                    let hour24 = hour;
                    if (ampm === 'PM' && hour !== 12) hour24 += 12;
                    if (ampm === 'AM' && hour === 12) hour24 = 0; // 12:XX AM is 0 hour

                    const slotDate = new Date();
                    slotDate.setHours(hour24, minute, 0, 0);

                    // Disable past slots on the current day
                    if (slotDate < now) {
                        btn.disabled = true;
                        btn.classList.add("past-slot");
                        btn.title = "Time has passed";
                    }
                }

                // If GetSlotStatus returns data by full slot time and it's fully booked
                const bookingInfo = slotData.find(s => s.time === slotTimeText);
                if (bookingInfo && bookingInfo.count >= 5) {
                    btn.disabled = true;
                    btn.classList.add("fully-booked");
                    btn.title = "Fully booked";
                }
            });
        } catch (error) {
            console.error("Error loading slot data:", error);
        }
    })();
});