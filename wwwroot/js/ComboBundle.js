document.addEventListener("DOMContentLoaded", () => {
      const bookBtn = document.getElementById("bookBtn");
      if (bookBtn) {
        bookBtn.addEventListener("click", () => {
          window.location.href = "/Home/Booking";
        });
      }
    });

  