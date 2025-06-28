// Js/Login.js
document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    const loginFeedback = document.getElementById('loginFeedback');
    // Ensure this backendBaseUrl matches your backend's actual running URL
    const backendBaseUrl = 'http://localhost:5034/api'; 

    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value.trim();

        if (!email || !password) {
            loginFeedback.textContent = 'Please enter both email and password.';
            loginFeedback.className = 'text-red-500 mt-4';
            return;
        }

        try {
            const response = await fetch(`${backendBaseUrl}/User/login`, { // Ensure this endpoint is correct
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            if (response.ok) {
                const data = await response.json();

                // --- DEBUGGING LINES (can remove after successful test) ---
                console.log("Full login response data received:", data);
                console.log("Token:", data.token);
                console.log("User ID:", data.userId);
                console.log("User Name:", data.name);
                console.log("User Email:", data.email);
                console.log("User Role:", data.role);
                // --- END DEBUGGING LINES ---

                // Store token, userId, userName, userEmail, and userRole in localStorage
                localStorage.setItem('token', data.token);     // Corrected to data.token (lowercase 't')
                localStorage.setItem('userId', data.userId);   // Corrected to data.userId (camelCase)
                localStorage.setItem('userName', data.name);   // Added: Store user's name
                localStorage.setItem('userEmail', data.email); // Added: Store user's email
                localStorage.setItem('userRole', data.role);   // This was already correct

                loginFeedback.textContent = 'Login successful!';
                loginFeedback.className = 'text-green-500 mt-4';

                // --- ROLE-BASED REDIRECTION LOGIC ---
                if (data.role === 'Admin') {
                    window.location.href = 'admin.html';
                } else {
                    window.location.href = 'dashboard.html';
                }
                // --- END ROLE-BASED REDIRECTION LOGIC ---

            } else {
                const errorData = await response.json(); // Try to parse as JSON first
                loginFeedback.textContent = errorData.message || `Login failed: ${response.statusText}`;
                loginFeedback.className = 'text-red-500 mt-4';
            }
        } catch (err) {
            console.error('An error occurred during login request:', err);
            loginFeedback.textContent = 'An error occurred. Please try again.';
            loginFeedback.className = 'text-red-500 mt-4';
        }
    });
});