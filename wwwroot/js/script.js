// ==========================
// MAIN SCRIPT
// ==========================
document.addEventListener('DOMContentLoaded', () => {
  // ELEMENT REFERENCES
  const hamburger = document.getElementById('hamburger');
  const navMenu = document.getElementById('nav-menu');
  const accountBtn = document.getElementById('accountBtn');
  const sidebar = document.getElementById('sidebar');
  const closeSidebar = document.getElementById('closeSidebar');
  const sidebarOverlay = document.getElementById('sidebarOverlay');
  const bookNowBtn = document.getElementById('bookNowBtn');
  const openSidebarBtn = document.getElementById('openSidebar'); // account icon button

  // ==========================
  // HAMBURGER MENU TOGGLE
  // ==========================
  if (hamburger && navMenu) {
    hamburger.addEventListener('click', () => {
      navMenu.classList.toggle('active');
    });
  }

  // ==========================
  // SIDEBAR TOGGLE
  // ==========================
  const openSidebar = () => {
    sidebar.classList.add('active');
    if (sidebarOverlay) sidebarOverlay.classList.add('active');
  };

  const closeSidebarMenu = () => {
    sidebar.classList.remove('active');
    if (sidebarOverlay) sidebarOverlay.classList.remove('active');
  };

  if (accountBtn && sidebar) accountBtn.addEventListener('click', openSidebar);
  if (openSidebarBtn && sidebar) openSidebarBtn.addEventListener('click', openSidebar);
  if (closeSidebar) closeSidebar.addEventListener('click', closeSidebarMenu);
  if (sidebarOverlay) sidebarOverlay.addEventListener('click', closeSidebarMenu);

  // ==========================
  // BOOK NOW BUTTON REDIRECT
  // ==========================
  if (bookNowBtn) {
    bookNowBtn.addEventListener('click', () => {
      window.location.href = "/Home/Booking"; 
    });
  }

  // ==========================
  // HERO CAROUSEL FUNCTIONALITY
  // ==========================
  const slides = document.querySelectorAll('.carousel-slide');
  const carousel = document.querySelector('.hero-carousel');
  const prevSlideBtn = document.querySelector('.carousel-control.prev');
  const nextSlideBtn = document.querySelector('.carousel-control.next');
  let currentSlide = 0;

  if (slides.length > 0) {
    const showSlide = (n) => {
      slides.forEach((slide) => slide.classList.remove('active'));
      slides[n].classList.add('active');
    };

    const nextSlide = () => {
      currentSlide = (currentSlide + 1) % slides.length;
      showSlide(currentSlide);
    };

    const prevSlide = () => {
      currentSlide = (currentSlide - 1 + slides.length) % slides.length;
      showSlide(currentSlide);
    };

    // Auto-play
    let slideInterval = setInterval(nextSlide, 3000);

    // Pause on hover
    if (carousel) {
      carousel.addEventListener('mouseenter', () => clearInterval(slideInterval));
      carousel.addEventListener('mouseleave', () => {
        slideInterval = setInterval(nextSlide, 3000);
      });
    }

    // Manual controls
    if (prevSlideBtn) prevSlideBtn.addEventListener('click', prevSlide);
    if (nextSlideBtn) nextSlideBtn.addEventListener('click', nextSlide);
  }

  // ==========================
  // TESTIMONIAL SLIDER
  // ==========================
  const testimonials = [
    {
      name: 'Client’s name',
      quote:
        '"I have always had a wonderful experience. The receptionists at Le’Ray are warm and welcoming, and the professionals are incredibly knowledgeable when it comes to skincare and wellness. They offer helpful advice tailored to my needs. I’ve recommended Le’Ray to friends, and they’ve all had the same amazing experience. Great job, Le’Ray Aesthetic and Wellness Center!"',
      rating: 5,
    },
    // Add more testimonials if needed
  ];

  let currentTestimonial = 0;
  const quoteEl = document.querySelector('.testimonial-box .quote');
  const nameEl = document.querySelector('.testimonial-box h3');
  const starsEl = document.querySelector('.testimonial-box .stars');
  const dots = document.querySelectorAll('.testimonial .dot');
  const prevBtn = document.getElementById('prevBtn');
  const nextBtn = document.getElementById('nextBtn');

  const showTestimonial = (n) => {
    if (quoteEl && nameEl && starsEl) {
      const testimonial = testimonials[n];
      quoteEl.textContent = testimonial.quote;
      nameEl.textContent = testimonial.name;
      starsEl.textContent = '★'.repeat(testimonial.rating);
      dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === n);
      });
    }
  };

  // Initialize first testimonial
  showTestimonial(currentTestimonial);

  if (prevBtn) {
    prevBtn.addEventListener('click', () => {
      currentTestimonial = (currentTestimonial - 1 + testimonials.length) % testimonials.length;
      showTestimonial(currentTestimonial);
    });
  }

  if (nextBtn) {
    nextBtn.addEventListener('click', () => {
      currentTestimonial = (currentTestimonial + 1) % testimonials.length;
      showTestimonial(currentTestimonial);
    });
  }
});

// ==========================
// PASSWORD TOGGLE FUNCTION
// ==========================
function togglePassword(fieldId, icon) {
  const field = document.getElementById(fieldId);
  if (field) {
    if (field.type === 'password') {
      field.type = 'text';
      icon.innerHTML = '<i class="fa-solid fa-eye-slash"></i>';
    } else {
      field.type = 'password';
      icon.innerHTML = '<i class="fa-solid fa-eye"></i>';
    }
  }
}


