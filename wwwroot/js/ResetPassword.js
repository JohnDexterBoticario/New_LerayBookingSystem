document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("resetForm");

    form.addEventListener("submit", (e) => {
        e.preventDefault();

        const newPassword = form.querySelector('input[name="NewPassword"]').value.trim();
        const confirmPassword = form.querySelector('input[name="ConfirmPassword"]').value.trim();

        if (!newPassword || !confirmPassword) return alert("Please fill in both fields.");
        if (newPassword !== confirmPassword) return alert("Passwords do not match.");

        // Optionally, call API via fetch/ajax
        form.submit(); // fallback: submit to MVC controller
    });
});
