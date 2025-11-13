// --- /js/Auth.js ---

// Global API URL
// NOTE: Assuming your backend runs on port 5078 (from your browser screenshots)
// and the AuthController is mapped to /api/Auth
const API_URL = 'http://localhost:5078/api/Auth'; // <-- CORRECTED PORT/PATH
const apiCall = authApi.apiCall; // Extract apiCall for local use

/**
 * Utility object for handling API calls
 */
const authApi = {
    // ... (All existing functions are fine) ...
    
    // --- Re-usable fetch utility ---
    apiCall: async (endpoint, body) => {
        try {
            // NOTE: URL structure: http://localhost:5078/api/Auth/register
            const response = await fetch(`${API_URL}/${endpoint}`, { 
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body),
            });
            const data = await response.json();
            
            if (!response.ok) {
                // Critical: Throw a detailed error message if available
                const backendMessage = data.message || data.title || (data.errors ? JSON.stringify(data.errors) : 'An unknown error occurred.');
                throw new Error(backendMessage); 
            }
            return { success: true, data };
        } catch (error) {
            console.error('API Error:', error.message);
            // Return only the message (which could be complex JSON stringified)
            return { success: false, message: error.message }; 
        }
    }
};

/**
 * Utility object for managing auth state (localStorage)
 */
const authService = {
    // ... (All existing functions are fine) ...
    // Note: ensure the user object you save matches the properties you try to access (user.name)
};

/**
 * Controls the UI state based on authentication
 */
const uiStateController = {
    // ... (All existing functions are fine) ...
};


/**
 * Main function to wire up all event listeners
 */
document.addEventListener('DOMContentLoaded', () => {

    // --- Get all forms and modals ---
    // (Existing definitions are fine)
    const signupModal = document.getElementById('signupModal');
    const loginModal = document.getElementById('loginModal');
    // NOTE: otpModal, mfaModal are not defined in your HTML, will be null.
    // Ensure you add these modal elements to your _Layout.cshtml if you use them.

    const signupForm = document.getElementById('signupForm');
    const loginForm = document.getElementById('loginForm');
    
    // --- Error message elements ---
    // NOTE: The previous logic creates a new <p> element for errors.
    // We need to match the placeholders you manually added in the previous step.
    
    // Select the existing error placeholders added to the forms in the last step
    const signupErrorEl = document.getElementById('signupForm-error'); 
    const loginErrorEl = document.getElementById('loginForm-error');
    // Ensure your _Layout.cshtml has elements with IDs 'otp-error' and 'mfa-error' if used.
    const otpErrorEl = document.getElementById('otp-error');
    const mfaErrorEl = document.getElementById('mfa-error');

    // --- Check initial login state ---
    if (authService.isLoggedIn()) {
        uiStateController.updateUiForLogin();
    } // Removed updateUiForLogout here, will run if logged in fails

    // --- 1. SIGNUP FORM SUBMISSION ---
    if (signupForm) {
        signupForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            // Display error element based on its existence
            if (signupErrorEl) signupErrorEl.textContent = ''; 

            const pass = document.getElementById('password').value; // <-- FIX: Changed from 'signupPassword' to 'password'
            const confirm = document.getElementById('confirmPassword').value;

            if (pass !== confirm) {
                if (signupErrorEl) signupErrorEl.textContent = 'Passwords do not match.';
                return;
            }

            // --- GATHER DTO DATA (CRITICAL ID MATCHING FIXES) ---
            const dto = {
                firstName: document.getElementById('firstName').value, // <-- FIX: ID 'FirstName' is 'firstName' in HTML
                lastName: document.getElementById('lastName').value,   // <-- FIX: ID 'LastName' is 'lastName' in HTML
                email: document.getElementById('signupEmail').value,
                // --- PHONE NUMBER FIX ---
                // Since your HTML is missing a dedicated phone field, use the manual prompt as a temporary fix
                phone: prompt("Please enter your mobile number (e.g., +639...):"), 
                password: pass,
                confirmPassword: confirm,
                captchaToken: 'mock-captcha-token' 
            };
            
            // Temporary prompt check
            if (!dto.phone) {
                if (signupErrorEl) signupErrorEl.textContent = 'Mobile number is required for OTP.';
                return;
            }

            const result = await authApi.register(dto);

            if (result.success) {
                // Handle Success flow
                if (signupModal) signupModal.style.display = 'none';
                // Show OTP modal logic requires the modal to be present in HTML
                // For now, reload the page and prompt user to check OTP
                alert('Registration successful! Please proceed to login.');
                window.location.reload(); 

                // --- ORIGINAL OTP FLOW (Requires HTML Modals) ---
                /*
                document.getElementById('otpIdentifier').value = dto.email;
                if (signupModal) signupModal.style.display = 'none';
                if (otpModal) otpModal.style.display = 'block';
                */
            } else {
                if (signupErrorEl) signupErrorEl.textContent = result.message;
            }
        });
    }

    // --- 2. OTP FORM SUBMISSION (Skipping, requires HTML) ---
    // ... (Keep existing OTP logic, but ensure modals/forms are in HTML) ...

    // --- 3. LOGIN FORM SUBMISSION ---
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (loginErrorEl) loginErrorEl.textContent = '';

            // --- GATHER DTO DATA (CRITICAL ID MATCHING FIXES) ---
            const identifier = document.getElementById('login-username').value; // <-- FIX: ID is 'login-username' in HTML
            const dto = {
                loginIdentifier: identifier,
                password: document.getElementById('login-password').value, // <-- FIX: ID is 'login-password' in HTML
                captchaToken: 'mock-captcha-token' 
            };

            const result = await authApi.login(dto);

            if (result.success) {
                if (result.data.mfaRequired) {
                    // ADMIN 2FA Flow (Requires HTML Modals)
                    // ... (Keep existing MFA logic) ...
                } else {
                    // CLIENT Login Flow
                    authService.saveAuthData(result.data);
                    uiStateController.updateUiForLogin();
                    if (loginModal) loginModal.style.display = 'none';
                    window.location.reload(); // Simple redirect on success
                }
            } else {
                if (loginErrorEl) loginErrorEl.textContent = result.message;
            }
        });
    }

    // --- 4. MFA FORM SUBMISSION (Skipping, requires HTML) ---
    // ... (Keep existing MFA logic, but ensure modals/forms are in HTML) ...

    // --- 5. SOCIAL LOGIN BUTTONS ---
    // ... (Existing social login logic is fine for now) ...

    // Function to get the stored JWT token
// Function to get the stored JWT token
function getAuthToken() {
    // This key must match the key used in auth.js when saving the token.
    return localStorage.getItem('authToken'); 
}

/**
 * Custom fetch function for authenticated API calls.
 * @param {string} endpoint - The specific API route (e.g., 'Bookings/my-bookings')
 * @param {object} options - Standard fetch options (method, body, etc.)
 */
async function authenticatedFetch(endpoint, options = {}) {
    const token = getAuthToken();
    const url = `/api/${endpoint}`;

    // 1. Prepare headers
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers 
    };

    // 2. Attach Authorization header (CRITICAL STEP)
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    } else {
        // Handle case where token is missing for a protected call
        console.error('Authentication token is missing. Redirecting to login.');
        // Optional: Force a client-side redirect to the home page to prompt login modal
        // window.location.href = '/';
    }

    // 3. Perform the fetch request
    const response = await fetch(url, {
        ...options, 
        headers: headers 
    });

    // 4. Handle token expiration/invalidity
    if (response.status === 401 || response.status === 403) {
        console.warn("Session expired or unauthorized. Clearing token.");
        localStorage.removeItem('authToken');
    }

    return response;
}
// Example in a dashboard script (e.g., clientHistory.js)

async function loadMyBookings() {
    try {
        // Use the new client to call the protected API endpoint
        const response = await authenticatedFetch('Bookings/my-bookings');

        if (response.ok) {
            const bookings = await response.json();
            console.log("Bookings loaded successfully:", bookings);
            // Update the DOM to display bookings...
        } else {
            // Display specific error returned by the server
            const errorData = await response.json();
            console.error("Failed to load bookings:", errorData.Message);
        }
    } catch (error) {
        console.error("Network or parsing error:", error);
    }
}
});