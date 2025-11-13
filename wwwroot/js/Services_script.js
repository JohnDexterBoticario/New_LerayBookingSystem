document.querySelectorAll('.service-card').forEach(card => {
    card.addEventListener('click', () => {
      card.style.transform = 'scale(0.98)';
      setTimeout(() => (card.style.transform = 'scale(1)'), 150);
    });
  
  // Expand/collapse service category
  document.querySelectorAll(".category-header").forEach(header => {
    header.addEventListener("click", () => {
      const category = header.parentElement;
      category.classList.toggle("open");
      const list = category.querySelector(".category-list");
      list.style.display = category.classList.contains("open") ? "block" : "none";
    });
  });

  document.addEventListener("DOMContentLoaded", () => {
    const bookBtn = document.getElementById("bookBtn");
    if (bookBtn) {
      bookBtn.addEventListener("click", () => {
        window.location.href = "/Home/Booking";
      });
    }
  });
  
  // Continue button simulation
  function nextStep() {
    const steps = document.querySelectorAll(".steps li");
    const activeIndex = [...steps].findIndex(s => s.classList.contains("active"));
    if (activeIndex < steps.length - 1) {
      steps[activeIndex].classList.remove("active");
      steps[activeIndex + 1].classList.add("active");
      alert(`Proceeding to: ${steps[activeIndex + 1].textContent}`);
    } else {
      alert("You're on the last step!");
    }
   }
  });