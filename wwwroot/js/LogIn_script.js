// ===== login.js (Handles Login, Signup, and Logout + API integration) =====
document.addEventListener("DOMContentLoaded", function () {
    // ===========================================================
    // 🧩 ELEMENTS & REGEX
    // ===========================================================
    const bookNowBtn = document.getElementById("bookNowBtn");
    const signupModalEl = document.getElementById("signupModal");
    const loginModalEl = document.getElementById("loginModal");
    const openLoginLink = document.getElementById("openLogin");
    const openSignupLink = document.getElementById("openSignup");
    const closeSignup = signupModalEl?.querySelector(".close");
    const closeLogin = loginModalEl?.querySelector(".close-login");
    const signupForm = document.getElementById("signupForm");
    const loginForm = document.getElementById("loginForm");
    const logoutBtn = document.getElementById("logoutBtn");
    const phoneRegex = /^\+?\d{10,15}$/;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // ===========================================================
    // ⚙️ Notification System
    // ===========================================================
    const notificationContainer = document.createElement('div');
    notificationContainer.id = 'notification-container';
    notificationContainer.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 10000;
        display: flex;
        flex-direction: column;
        gap: 10px;
    `;
    document.body.appendChild(notificationContainer);

    function displayMessage(message, type = 'info') {
        const notification = document.createElement('div');
        let bgColor = '#4F46E5'; // Info
        if (type === 'success') bgColor = '#10B981';
        if (type === 'error') bgColor = '#EF4444';

        notification.textContent = message;
        notification.style.cssText = `
            background-color: ${bgColor};
            color: white;
            padding: 12px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            opacity: 0;
            transition: opacity 0.5s, transform 0.5s;
            transform: translateX(100%);
            font-family: sans-serif;
            max-width: 300px;
        `;
        notificationContainer.appendChild(notification);

        setTimeout(() => {
            notification.style.opacity = 1;
            notification.style.transform = 'translateX(0)';
        }, 10);

        setTimeout(() => {
            notification.style.opacity = 0;
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 500);
        }, 5000);
    }

    // ===========================================================
    // 🧩 ELEMENT LISTENERS (Validation & Modals)
    // ===========================================================
    // Email validation for signup
    const signupEmail = document.getElementById("signupEmail");
    if (signupEmail) {
        signupEmail.addEventListener("input", () => {
            signupEmail.setCustomValidity(
                !emailRegex.test(signupEmail.value.trim())
                    ? "Please enter a valid email (must include a domain like .com or .ph)."
                    : ""
            );
        });
    }

    // Auto-detect login input type
    const loginInput = document.getElementById("login-username");
    if (loginInput) {
        loginInput.addEventListener("input", () => {
            const value = loginInput.value.trim();
            if (emailRegex.test(value)) loginInput.placeholder = "Logging in with Email 📧";
            else if (phoneRegex.test(value)) loginInput.placeholder = "Logging in with Phone 📱";
            else loginInput.placeholder = "Logging in with Username 👤";
        });
    }

    // ===========================================================
    // ⚙️ Modal Management
    // ===========================================================
    function openModal(modal) {
        if (modal) {
            modal.style.display = "flex";
            document.body.classList.add("modal-open");
        }
    }

    function closeModal(modal) {
        if (modal) {
            modal.style.display = "none";
            document.body.classList.remove("modal-open");
        }
    }

    const urlParams = new URLSearchParams(window.location.search);

    if (bookNowBtn) {
        bookNowBtn.addEventListener("click", (e) => {
            e.preventDefault();
            openModal(signupModalEl);
        });
    }

    if (urlParams.has("ReturnUrl")) openModal(loginModalEl);

    openLoginLink?.addEventListener("click", (e) => { e.preventDefault(); closeModal(signupModalEl); openModal(loginModalEl); });
    openSignupLink?.addEventListener("click", (e) => { e.preventDefault(); closeModal(loginModalEl); openModal(signupModalEl); });
    closeSignup?.addEventListener("click", () => closeModal(signupModalEl));
    closeLogin?.addEventListener("click", () => closeModal(loginModalEl));
    window.addEventListener("click", (event) => {
        if (event.target === signupModalEl) closeModal(signupModalEl);
        if (event.target === loginModalEl) closeModal(loginModalEl);
    });

    // ===========================================================
    // 🟢 SIGNUP HANDLER
    // ===========================================================
    // NOTE: signup is also handled by OTPVerification.js to show OTP modal. Keep this minimal.
    signupForm?.addEventListener("submit", async (e) => {
        // The OTPVerification.js listens to the same form and will show OTP modal after success.
        // We keep this handler for consistent behavior (but the heavy logic is in OTPVerification.js).
    });

    // ===========================================================
    // 🟢 LOGIN HANDLER
    // ===========================================================
    loginForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const emailOrUsername = document.getElementById("login-username")?.value.trim() || "";
        const password = document.getElementById("login-password")?.value.trim() || "";
        const rememberMe = document.getElementById("rememberMe")?.checked;

        if (!emailOrUsername || !password) {
            displayMessage("❌ Please enter both your account identifier and password.", 'error');
            return;
        }

        const loginDto = { EmailOrUsername: emailOrUsername, Password: password };

        try {
            const res = await fetch("/api/Auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "same-origin",
                body: JSON.stringify(loginDto),
            });

            const result = await res.json();

            if (res.ok) {
                displayMessage("✅ " + result.message, "success");
                closeModal(loginModalEl);

                if (rememberMe) localStorage.setItem("rememberedUser", emailOrUsername);
                else localStorage.removeItem("rememberedUser");

                window.location.href = result.redirectUrl || urlParams.get("ReturnUrl") || "/Home/Index";
            } else {
                displayMessage("❌ Login failed: " + (result.message || "Unknown error"), "error");
            }
        } catch (err) {
            console.error("Error during login:", err);
            displayMessage("❌ Network error while logging in. Check your API endpoint.", "error");
        }
    });

    // Load saved remembered user
    const savedUser = localStorage.getItem("rememberedUser");
    const usernameField = document.getElementById("login-username");
    const rememberCheckbox = document.getElementById("rememberMe");
    if (savedUser && usernameField && rememberCheckbox) {
        usernameField.value = savedUser;
        rememberCheckbox.checked = true;
    }

    // ===========================================================
    // 🔴 LOGOUT HANDLER
    // ===========================================================
    if (logoutBtn) {
        logoutBtn.addEventListener("click", async (e) => {
            e.preventDefault();
            try {
                const res = await fetch("/api/Auth/logout", {
                    method: "POST",
                    credentials: "same-origin"
                });

                const result = await res.json();

                if (res.ok) {
                    displayMessage("👋 " + (result.message || "Logged out"), "info");
                    window.location.href = "/Home/Index";
                } else {
                    displayMessage("⚠️ Logout failed.", "error");
                }
            } catch (err) {
                console.error("Logout error:", err);
                displayMessage("⚠️ Network error while logging out.", "error");
            }
        });
    }

    // ===========================================================
    // 🧩 SOCIAL LOGIN HANDLERS
    // ===========================================================
    document.querySelector(".social-btn.google")?.addEventListener("click", () => window.location.href = "/signin-google");
    document.querySelector(".social-btn.facebook")?.addEventListener("click", () => window.location.href = "/signin-facebook");
});
