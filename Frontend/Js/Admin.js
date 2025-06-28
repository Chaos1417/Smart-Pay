// admin.js 

document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (!token || userRole !== 'Admin') {
        alert('Access Denied: You must be an authenticated administrator to view this page.');
        window.location.href = 'login.html';
        return;
    }

    const pendingUsersBody = document.getElementById('pendingUsersBody');
    const backendBaseUrl = 'http://localhost:5034/api/admin';

    // Function to fetch and display pending users
    async function fetchPendingUsers() {
        pendingUsersBody.innerHTML = '<tr><td colspan="7" class="py-3 px-6 text-center text-gray-400">Loading pending users...</td></tr>';
        try {
            const response = await fetch(`${backendBaseUrl}/pending-users`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const users = await response.json();
                renderPendingUsers(users);
            } else if (response.status === 401 || response.status === 403) {
                alert('Authentication/Authorization failed. Please log in again.');
                localStorage.clear();
                window.location.href = 'login.html';
            } else {
                const errorText = await response.text();
                pendingUsersBody.innerHTML = `<tr><td colspan="7" class="py-3 px-6 text-red-500 text-center">Failed to load pending users: ${errorText}</td></tr>`;
                console.error('Failed to load pending users:', errorText);
            }
        } catch (error) {
            pendingUsersBody.innerHTML = `<tr><td colspan="7" class="py-3 px-6 text-red-500 text-center">Network error: Could not connect to backend.</td></tr>`;
            console.error('Network error fetching pending users:', error);
        }
    }

    // Function to render users into the tbody
    function renderPendingUsers(users) {
        if (users.length === 0) {
            pendingUsersBody.innerHTML = '<tr><td colspan="7" class="py-3 px-6 text-center text-gray-400">No pending users found.</td></tr>';
            return;
        }

        let html = '';
        users.forEach((user, index) => {
            const rowBgClass = index % 2 === 0 ? 'bg-gray-700' : 'bg-gray-800';
            html += `
                <tr class="${rowBgClass} hover:bg-gray-600 transition duration-150 ease-in-out" data-user-id="${user.userID}">
                    <td class="py-3 px-6 text-left">${user.userID}</td>
                    <td class="py-3 px-6 text-left">${user.name}</td>
                    <td class="py-3 px-6 text-left">${user.email}</td>
                    <td class="py-3 px-6 text-left">${user.mobile}</td>
                    <td class="py-3 px-6 text-left">${user.status}</td>
                    <td class="py-3 px-6 text-center">
                        <div class="flex items-center justify-center space-x-2"> <!-- ADDED FLEX CONTAINER HERE -->
                            <button class="px-4 py-2 bg-green-500 text-white font-bold rounded-lg hover:bg-green-600 transition duration-200 shadow-md approve-btn">Approve</button>
                            <button class="px-4 py-2 bg-red-500 text-white font-bold rounded-lg hover:bg-red-600 transition duration-200 shadow-md reject-btn">Reject</button>
                        </div>
                    </td>
                </tr>
            `;
        });
        pendingUsersBody.innerHTML = html;

        // Attach event listeners to the new buttons
        document.querySelectorAll('.approve-btn').forEach(button => {
            button.addEventListener('click', handleApproveUser);
        });
        document.querySelectorAll('.reject-btn').forEach(button => {
            button.addEventListener('click', handleRejectUser);
        });
    }

    // --- Core Logic for Approving User ---
    async function handleApproveUser(event) {
        const row = event.target.closest('tr');
        const userId = row.dataset.userId;
        const defaultBalance = 5000;

        if (!confirm(`Are you sure you want to approve user ID ${userId} and set their balance to ${defaultBalance}?`)) {
            return;
        }

        try {
            const response = await fetch(`${backendBaseUrl}/approve-user/${userId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ Balance: defaultBalance })
            });

            if (response.ok) {
                alert(`User ${userId} approved successfully with a balance of ${defaultBalance}!`);
                fetchPendingUsers();
            } else {
                const errorText = await response.text();
                try {
                    const errorDetails = JSON.parse(errorText);
                    console.error('Approve User Error Response:', errorDetails);
                    alert(`Failed to approve user: ${response.status} - ${errorDetails.title || 'Unknown error'}. Details: ${JSON.stringify(errorDetails.errors || errorDetails.message)}`);
                } catch (jsonParseError) {
                     console.error('Approve User failed, non-JSON response:', errorText, jsonParseError);
                     alert(`Failed to approve user: ${response.status} - ${errorText || 'Unknown error. Server did not return valid JSON.'}`);
                }
            }
        } catch (error) {
            console.error('Network error approving user:', error);
            alert('An error occurred while trying to approve the user. Please check your network.');
        }
    }

    // --- Core Logic for Rejecting User ---
    async function handleRejectUser(event) {
        const row = event.target.closest('tr');
        const userId = row.dataset.userId;

        if (!confirm(`Are you sure you want to reject and delete user ID ${userId}? This action is irreversible.`)) {
            return;
        }

        try {
            const response = await fetch(`${backendBaseUrl}/reject-user/${userId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                alert(`User ${userId} rejected and deleted successfully!`);
                fetchPendingUsers();
            } else {
                const errorText = await response.text();
                 try {
                    const errorDetails = JSON.parse(errorText);
                    console.error('Reject User Error Response:', errorDetails);
                    alert(`Failed to reject user: ${response.status} - ${errorDetails.title || 'Unknown error'}. Details: ${JSON.stringify(errorDetails.errors || errorDetails.message)}`);
                } catch (jsonParseError) {
                     console.error('Reject User failed, non-JSON response:', errorText, jsonParseError);
                     alert(`Failed to reject user: ${response.status} - ${errorText || 'Unknown error. Server did not return valid JSON.'}`);
                }
            }
        } catch (error) {
            console.error('Network error rejecting user:', error);
            alert('An error occurred while trying to reject the user. Please check your network.');
        }
    }

    // Handle Logout Button
    const logoutButton = document.getElementById('logoutButton');
    if (logoutButton) {
        logoutButton.addEventListener('click', () => {
            localStorage.clear();
            alert('Logged out successfully.');
            window.location.href = 'login.html';
        });
    }

    // Initial fetch of pending users when the page loads
    fetchPendingUsers();
});