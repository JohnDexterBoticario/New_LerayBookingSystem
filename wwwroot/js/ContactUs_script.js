document.addEventListener("DOMContentLoaded", () => {
  // ===== Book Now Button =====
  const bookBtn = document.getElementById("bookBtn");
  if (bookBtn) {
    bookBtn.addEventListener("click", () => {
      window.location.href = "/Home/Booking";
    });
  }
});
