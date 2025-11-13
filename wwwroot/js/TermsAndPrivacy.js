document.addEventListener("DOMContentLoaded", function () {
  const modal = new bootstrap.Modal(document.getElementById("termsPrivacyModal"));
  const modalBody = document.querySelector("#termsPrivacyModal .modal-body");

  document.querySelectorAll(".open-terms-privacy").forEach(button => {
    button.addEventListener("click", async function (e) {
      e.preventDefault();
      modalBody.innerHTML = `<p class="text-center text-muted">Loading...</p>`;

      try {
        const response = await fetch("/Home/TermsAndPrivacy");
        if (!response.ok) throw new Error("Failed to load Terms & Privacy");
        const html = await response.text();

        // Extract just the main container from the page
        const temp = document.createElement("div");
        temp.innerHTML = html;
        const content = temp.querySelector(".container")?.innerHTML || html;

        modalBody.innerHTML = content;
      } catch (error) {
        modalBody.innerHTML = `<p class="text-danger text-center">Failed to load content. Please try again later.</p>`;
      }

      modal.show();
    });
  });
});
