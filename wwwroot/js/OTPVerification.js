$(document).ready(function() {
    // Modal elements and Bootstrap instances
    const signupModalEl = document.getElementById('signupModal');
    const signupModalInstance = signupModalEl ? new bootstrap.Modal(signupModalEl) : null;
    const otpModalEl = document.getElementById('otpModal');
    const otpModalInstance = otpModalEl ? new bootstrap.Modal(otpModalEl) : null;

    const otpInputs = document.querySelectorAll(".otp-digit");

    // Auto-focus OTP inputs
    otpInputs.forEach((input, i) => {
        input.addEventListener('input', () => {
            if (input.value.length === 1 && i < otpInputs.length - 1) {
                otpInputs[i+1].focus();
            }
        });
    });

    // ================= SIGNUP =================
    $('#signupForm').submit(async function(e) {
        e.preventDefault();

        const firstName = $('#firstName').val().trim();
        const lastName = $('#lastName').val().trim();
        const email = $('#signupEmail').val().trim();
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();
        const agreeTerms = $('#agreeTerms').is(':checked');

        if (!agreeTerms) {
            alert("Please agree to the Terms and Privacy Policy.");
            return;
        }

        if (!firstName || !lastName || !email || !password || !confirmPassword) {
            alert("All fields are required.");
            return;
        }

        if (password !== confirmPassword) {
            alert("Passwords do not match.");
            return;
        }

        const registerDto = { firstName, lastName, email, password, confirmPassword };

        try {
            const res = await fetch('/api/Auth/signup', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(registerDto)
            });

            const contentType = res.headers.get('content-type');
            const result = contentType && contentType.includes('application/json') ? await res.json() : {};

            if (res.ok) {
                // Hide signup modal using bootstrap instance if available
                if (signupModalInstance) signupModalInstance.hide();

                // Pass email to OTP modal
                $('#otpEmail').val(email);

                // Clear OTP inputs
                otpInputs.forEach(input => input.value = '');

                // Show OTP modal
                if (otpModalInstance) otpModalInstance.show();
                else $('#otpModal').show();
            } else {
                alert(result.message || 'Signup failed');
                console.error(result);
            }
        } catch (err) {
            console.error(err);
            alert('Network error during signup.');
        }
    });

    // ================= VERIFY OTP =================
    $('#verifyOtpBtn').click(async function() {
        const code = Array.from(otpInputs).map(i => i.value).join('');
        const email = $('#otpEmail').val();

        if (code.length !== 6) {
            $('#otpMessage').text('Enter the 6-digit OTP.');
            return;
        }

        try {
            const res = await fetch('/api/Auth/verify-otp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, code })
            });

            const contentType = res.headers.get('content-type');
            const result = contentType && contentType.includes('application/json') ? await res.json() : {};

            if (res.ok) {
                $('#otpMessage').text('OTP Verified! Redirecting...');
                $('#otpMessage').removeClass('text-danger').addClass('text-success');

                setTimeout(() => {
                    if (otpModalInstance) otpModalInstance.hide();
                    window.location.href = '/Home/Index';
                }, 1500);
            } else {
                $('#otpMessage').text(result.message || 'Invalid OTP.');
            }
        } catch (err) {
            console.error(err);
            $('#otpMessage').text('Network error while verifying OTP.');
        }
    });

    // ================= RESEND OTP =================
    $('#resendOtpBtn').click(async function() {
        const email = $('#otpEmail').val();
        try {
            const res = await fetch('/api/Auth/resend-otp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });

            const contentType = res.headers.get('content-type');
            const result = contentType && contentType.includes('application/json') ? await res.json() : {};

            if (res.ok) {
                $('#otpMessage').text(result.message || 'OTP resent successfully!');
                $('#otpMessage').removeClass('text-danger').addClass('text-success');
            } else {
                $('#otpMessage').text(result.message || 'Failed to resend OTP.');
            }
        } catch (err) {
            console.error(err);
            $('#otpMessage').text('Network error while resending OTP.');
        }
    });
});
