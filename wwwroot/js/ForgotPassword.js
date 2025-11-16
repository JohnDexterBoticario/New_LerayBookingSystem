document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("forgotForm");

    form.addEventListener("submit", (e) => {
        e.preventDefault();

        const email = form.querySelector('input[name="Email"]').value.trim();
        if (!email) return alert("Please enter your email address.");

        // Optionally, you can call your API via fetch/ajax here
        form.submit(); // fallback: submit form to MVC controller
    });
});
