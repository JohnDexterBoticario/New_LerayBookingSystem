// ===== LeRay_script.js (Unified Main Site Logic) =====
document.addEventListener("DOMContentLoaded", function () {

  /* ==========================
     DROPDOWN MENU (Services)
  =========================== */
  const dropdownToggle = document.getElementById("servicesToggle");
  const dropdownMenu = document.querySelector(".dropdown-menu");

  if (dropdownToggle && dropdownMenu) {
    dropdownToggle.addEventListener("click", (e) => {
      e.stopPropagation();
      dropdownMenu.classList.toggle("show");

      const expanded = dropdownToggle.getAttribute("aria-expanded") === "true";
      dropdownToggle.setAttribute("aria-expanded", !expanded);
    });

    // Hide dropdown when clicking outside
    document.addEventListener("click", () => {
      dropdownMenu.classList.remove("show");
      dropdownToggle.setAttribute("aria-expanded", "false");
    });
  }

  /* ==========================
     LEARN MORE BUTTON (About Page)
  =========================== */
  const learnMoreBtn = document.getElementById("learnMoreBtn");
  if (learnMoreBtn) {
    learnMoreBtn.addEventListener("click", () => {
      alert("Thank you for your interest! Visit our Services page to explore more treatments.");
      window.location.href = "/Home/Index";
    });
  }

  /* ==========================
     BOOK NOW BUTTON
  =========================== */
  const bookNowBtn = document.getElementById("bookNowBtn");
  if (bookNowBtn) {
    bookNowBtn.addEventListener("click", (e) => {
      e.preventDefault();
      const signupModal = document.getElementById("signupModal");
      if (signupModal) {
        signupModal.style.display = "flex";
      } else {
        // Fallback redirect if modal doesn't exist
        window.location.href = "/Home/Index";
      }
    });
  }

  /* ==========================
     LOGIN & SIGNUP MODALS
  =========================== */
  const signupModal = document.getElementById("signupModal");
  const loginModal = document.getElementById("loginModal");
  const openLoginLink = document.getElementById("openLogin");
  const openSignupLink = document.getElementById("openSignup");
  const backToSignup = document.getElementById("backToSignup");

  const closeSignup = signupModal ? signupModal.querySelector(".close") : null;
  const closeLogin = loginModal ? loginModal.querySelector(".close-login") : null;

  // Switch from signup → login
  if (openLoginLink && signupModal && loginModal) {
    openLoginLink.addEventListener("click", (e) => {
      e.preventDefault();
      signupModal.style.display = "none";
      loginModal.style.display = "flex";
    });
  }

  // Switch from login → signup
  if (openSignupLink && signupModal && loginModal) {
    openSignupLink.addEventListener("click", (e) => {
      e.preventDefault();
      loginModal.style.display = "none";
      signupModal.style.display = "flex";
    });
  }

  // Back to signup from login
  if (backToSignup && signupModal && loginModal) {
    backToSignup.addEventListener("click", () => {
      loginModal.style.display = "none";
      signupModal.style.display = "flex";
    });
  }

  // Close modal buttons
  if (closeSignup) {
    closeSignup.addEventListener("click", () => {
      signupModal.style.display = "none";
    });
  }
  if (closeLogin) {
    closeLogin.addEventListener("click", () => {
      loginModal.style.display = "none";
    });
  }

  // Close modal when clicking outside
  window.addEventListener("click", (event) => {
    if (event.target === signupModal) signupModal.style.display = "none";
    if (event.target === loginModal) loginModal.style.display = "none";
  });

  /* ==========================
     FAQ TOGGLE (About Page)
  =========================== */
  const faqItems = document.querySelectorAll(".faq-item");
  faqItems.forEach(item => {
    item.addEventListener("click", () => {
      item.classList.toggle("active");
    });
  });

});
