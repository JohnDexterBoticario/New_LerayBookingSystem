// ===== login.js (Handles Login, Signup, and Logout + API integration) =====
document.addEventListener("DOMContentLoaded", function () {
      // ===========================================================
      // 🧩 ELEMENTS & REGEX
      // ===========================================================
      const bookNowBtn = document.getElementById("bookNowBtn");
      const signupModal = document.getElementById("signupModal");
      const loginModal = document.getElementById("loginModal");
    
      const openLoginLink = document.getElementById("openLogin");
      const openSignupLink = document.getElementById("openSignup");
    
      const closeSignup = signupModal?.querySelector(".close");
      const closeLogin = loginModal?.querySelector(".close-login");
    
      const signupForm = document.getElementById("signupForm");
      const loginForm = document.getElementById("loginForm");
      const logoutBtn = document.getElementById("logoutBtn");
    
      const phoneRegex = /^\+?\d{10,15}$/; 
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
      // Inject Notification Container into the DOM
      const notificationContainer = document.createElement('div');
      notificationContainer.id = 'notification-container';
      document.body.appendChild(notificationContainer);
  
      // Apply basic styles to the container
      notificationContainer.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 10000;
        display: flex;
        flex-direction: column;
        gap: 10px;
      `;
  
      // ===========================================================
      // ⚙️ Utility functions (Custom Notification System)
      // ===========================================================
  
      /**
        * Replaces the use of alert() with a custom, non-blocking notification.
        * @param {string} message The message to display.
        * @param {'success' | 'error' | 'info'} type The type of notification.
        */
      function displayMessage(message, type = 'info') {
        const notification = document.createElement('div');
        
        let bgColor = '#4F46E5'; // Info (Indigo)
        if (type === 'success') bgColor = '#10B981'; // Success (Emerald)
        if (type === 'error') bgColor = '#EF4444'; // Error (Red)
  
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
        
        // Animate in
        setTimeout(() => {
          notification.style.opacity = 1;
          notification.style.transform = 'translateX(0)';
        }, 10);
        
        // Animate out and remove after 5 seconds
        setTimeout(() => {
          notification.style.opacity = 0;
          notification.style.transform = 'translateX(100%)';
          setTimeout(() => {
            notification.remove();
          }, 500); // Wait for transition to finish
        }, 5000);
      }
  
  
      // ===========================================================
      // 🧩 ELEMENT LISTENERS (Validation & Modals)
      // ===========================================================
  
      // ✨ Email validation (Signup)
      const signupEmail = document.getElementById("signupEmail");
      if (signupEmail) {
        signupEmail.addEventListener("input", () => {
          if (!emailRegex.test(signupEmail.value.trim())) {
            // Using built-in validation for immediate feedback on the input field
            signupEmail.setCustomValidity("Please enter a valid email (must include a domain like .com or .ph).");
          } else {
            signupEmail.setCustomValidity("");
          }
        });
      }
    
      // ✨ Auto-detect login input type
      const loginInput = document.getElementById("login-username");
      if (loginInput) {
        loginInput.addEventListener("input", () => {
          const value = loginInput.value.trim();
    
          if (emailRegex.test(value)) {
            loginInput.placeholder = "Logging in with Email 📧";
          } else if (phoneRegex.test(value)) {
            loginInput.placeholder = "Logging in with Phone 📱";
          } else {
            loginInput.placeholder = "Logging in with Username 👤";
          }
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
    
      // 🌐 URL PARAMETERS & INITIAL OPENING
      const urlParams = new URLSearchParams(window.location.search);
    
      if (bookNowBtn) {
        bookNowBtn.addEventListener("click", (e) => {
          e.preventDefault();
          openModal(signupModal); 
        });
      }
    
      if (urlParams.has("ReturnUrl")) {
        openModal(loginModal);
      }
    
      // 🔄 Switch between login/signup modals
      openLoginLink?.addEventListener("click", (e) => {
        e.preventDefault();
        closeModal(signupModal);
        openModal(loginModal);
      });
    
      openSignupLink?.addEventListener("click", (e) => {
        e.preventDefault();
        closeModal(loginModal);
        openModal(signupModal);
      });
    
      closeSignup?.addEventListener("click", () => closeModal(signupModal));
      closeLogin?.addEventListener("click", () => closeModal(loginModal));
    
      window.addEventListener("click", (event) => {
        if (event.target === signupModal) closeModal(signupModal);
        if (event.target === loginModal) closeModal(loginModal);
      });
    
      // ===========================================================
    // 🟢 SIGNUP HANDLER (Email only or Email + optional phone)
    // ===========================================================
    signupForm?.addEventListener("submit", async (e) => {
      e.preventDefault();
    
      // ✅ Terms and Privacy checkbox validation
      const agreeTerms = document.getElementById("agreeTerms");
      if (agreeTerms && !agreeTerms.checked) {
        displayMessage("⚠️ Please agree to the Terms and Privacy Policy before creating an account.", 'error');
        return;
      }
    
      const firstName = document.getElementById("firstName")?.value.trim() || "";
      const lastName = document.getElementById("lastName")?.value.trim() || "";
      const email = document.getElementById("signupEmail")?.value.trim() || "";
      const password = document.getElementById("password")?.value.trim() || "";
      const confirmPassword = document.getElementById("confirmPassword")?.value.trim() || "";
    
      // ✅ Regex rules
      const nameRegex = /^[A-Za-z\s'-]+$/; // only letters, spaces, apostrophes, hyphens
    
      // ✅ Validate first and last names
      if (!firstName || !nameRegex.test(firstName)) {
        alert("❌ First name must contain letters only (no numbers or special characters).");
        displayMessage("❌ First name must contain letters only (no numbers or special characters).", 'error');
        return;
      }
    
      if (!lastName || !nameRegex.test(lastName)) {
        alert("❌ Last name must contain letters only (no numbers or special characters).");
        displayMessage("❌ Last name must contain letters only (no numbers or special characters).", 'error');
        return;
      }
    
      // ✅ Require email
      if (!email) {
      alert("❌ Please enter a valid email address — must include a valid domain like .com, .ph, or .org.");
        displayMessage("❌ Please enter your email address.", 'error');
        return;
      }
    
      // ✅ Validate email format
      if (email && !emailRegex.test(email)) {
        displayMessage("❌ Please enter a valid email address.", 'error');
        return;
      }
    
      // ✅ Check password match
      if (password !== confirmPassword) {
        alert("❌ Passwords do not match! Please retype them correctly.");
        displayMessage("❌ Passwords do not match!", 'error');
        return;
      }
    
      // ✅ Send data
      const registerDto = {
        firstName,
        lastName,
        email,
        password,
        confirmPassword,
        captchaToken: "mock-captcha-token",
      };
    
      try {
        const res = await fetch("/api/AccountApi/signup", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          credentials: "same-origin",
          body: JSON.stringify(registerDto),
        });
    
        const result = await res.json();
    
        if (res.ok) {
          alert("🎉 Signup successful! You can now log in.");
          displayMessage("✅ " + result.message, 'success');
          closeModal(signupModal);
          openModal(loginModal);
        } else {
          const err = result.message || result.errors || "An unknown error occurred.";
          console.error("Signup failed:", result);
          // Ensure a readable error message is displayed
          displayMessage("❌ Signup failed: " + (typeof err === 'string' ? err : JSON.stringify(err)), 'error');
        }
      } catch (error) {
        console.error("Error during signup:", error);
        displayMessage("❌ Network error while signing up. Check your API endpoint.", 'error');
      }
    });
    
   // ===========================================================
// 🟢 LOGIN HANDLER (Email / Username / Phone) + Remember Me
// ===========================================================
loginForm?.addEventListener("submit", async (e) => {
  e.preventDefault();

  const emailOrUsername = document.getElementById("login-username")?.value.trim() || "";
  const password = document.getElementById("login-password")?.value.trim() || "";
  const rememberMe = document.getElementById("rememberMe")?.checked; // ✅ added

  const loginDto = { 
    EmailOrUsername: emailOrUsername,
    Password: password 
  };

  // ✅ Check if fields are filled
  if (!emailOrUsername || !password) {
    alert("❌ Please enter both your account identifier and password.");
    displayMessage("❌ Please enter both your account identifier and password.", 'error');
    return;
  }

  try {
    const res = await fetch("/api/AccountApi/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "same-origin",
      body: JSON.stringify(loginDto),
    });

    const result = await res.json();

    if (res.ok) {
      alert("✅ Login successful! Redirecting...");
      displayMessage("✅ " + result.message, 'success');
      closeModal(loginModal);

      // ✅ Remember Me logic (only stores username/email)
      if (rememberMe) {
        localStorage.setItem("rememberedUser", emailOrUsername);
      } else {
        localStorage.removeItem("rememberedUser");
      }

      // Redirect to the return URL if it exists, otherwise home.
      window.location.href = result.redirectUrl || urlParams.get("ReturnUrl") || "/Home/Index";
    } else {
      const errMsg = result.message || "Invalid credentials. Please try again.";
      alert("❌ Login failed: " + errMsg);
      displayMessage("❌ Login failed: " + errMsg, 'error');
    }
  } catch (error) {
    console.error("Error during login:", error);
    alert("❌ Network error while logging in. Please check your connection or API endpoint.");
    displayMessage("❌ Network error while logging in. Check your API endpoint.", 'error');
  }
});

  
  // ✅ Load saved remembered user when page loads
  document.addEventListener("DOMContentLoaded", () => {
    const savedUser = localStorage.getItem("rememberedUser");
    const usernameField = document.getElementById("login-username");
    const rememberCheckbox = document.getElementById("rememberMe");
  
    if (savedUser && usernameField && rememberCheckbox) {
      usernameField.value = savedUser;
      rememberCheckbox.checked = true;
    }
  });
    
    // ===========================================================
  // 🔴 LOGOUT HANDLER
  // ===========================================================
  if (logoutBtn) {
    logoutBtn.addEventListener("click", async (e) => {
      e.preventDefault();
      try {
        const res = await fetch("/api/AccountApi/logout", {
          method: "POST",
          credentials: "same-origin",
        });
        const result = await res.json();
  
        if (res.ok) {
          alert("👋 You’ve been logged out successfully!");
          displayMessage("👋 " + result.message, 'info');
          window.location.href = "/Home/Index";
        } else {
          alert("⚠️ Logout failed. Server response not OK.");
          displayMessage("⚠️ Logout failed. Server response not OK.", 'error');
        }
      } catch (error) {
        console.error("Logout error:", error);
        alert("⚠️ Network error while logging out. Please check your connection.");
        displayMessage("⚠️ Network error while logging out.", 'error');
      }
    });
  }
  
    
      // ===========================================================
      // 🧩 SOCIAL LOGIN HANDLERS
      // ===========================================================
      const googleBtn = document.querySelector(".social-btn.google");
      const facebookBtn = document.querySelector(".social-btn.facebook");
      const instagramBtn = document.querySelector(".social-btn.instagram");
    
      // Uses /signin-{provider} endpoints configured in Program.cs
      if (googleBtn) googleBtn.addEventListener("click", () => (window.location.href = "/signin-google"));
      if (facebookBtn) facebookBtn.addEventListener("click", () => (window.location.href = "/signin-facebook"));
      
      // NOTE: Instagram is not configured in Program.cs, so this handler is commented out.
      // if (instagramBtn) instagramBtn.addEventListener("click", () => displayMessage("Instagram login is not currently supported.", 'info'));
    });