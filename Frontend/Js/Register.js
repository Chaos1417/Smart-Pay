// Js/Register.js
document.addEventListener('DOMContentLoaded', () => {
    const registerForm = document.getElementById('registerForm');
    const messageDiv = document.getElementById('message'); // Get the message element

    // Ensure this backendBaseUrl matches your backend's actual running URL
    const backendBaseUrl = 'http://localhost:5034/api'; 

    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const name = document.getElementById('name').value.trim();
        const mobile = document.getElementById('mobile').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value.trim();

        if (!name || !mobile || !email || !password) {
            messageDiv.textContent = 'Please fill in all fields.';
            messageDiv.className = 'text-red-500 mt-4'; // Apply Tailwind class for error
            return;
        }

        try {
            const response = await fetch(`${backendBaseUrl}/User/register`, { // Ensure this endpoint is correct
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name, mobile, email, password })
            });

            if (response.ok) {
                messageDiv.textContent = 'Registration successful! Awaiting admin approval. You can now login.';
                messageDiv.className = 'text-green-500 mt-4'; // Apply Tailwind class for success
                registerForm.reset(); // Clear the form
                // Optionally redirect to login page after a short delay
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 3000); // Redirect after 3 seconds
            } else {
                const errorText = await response.text(); // Get raw error text
                messageDiv.textContent = `Registration failed: ${errorText}`;
                messageDiv.className = 'text-red-500 mt-4'; // Apply Tailwind class for error
            }
        } catch (err) {
            console.error('An error occurred during registration request:', err);
            messageDiv.textContent = 'An error occurred. Please try again.';
            messageDiv.className = 'text-red-500 mt-4'; // Apply Tailwind class for error
        }
    });
});