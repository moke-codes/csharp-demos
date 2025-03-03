// SVG Icons
const EYE_ICON = '/icons/eye.svg';
const EYE_SLASH_ICON = '/icons/eye-slash.svg';

// Cache for SVG content
let svgCache = {};

// Load SVG content
async function loadSvg(path) {
    if (svgCache[path]) {
        return svgCache[path];
    }
    const response = await fetch(path);
    const content = await response.text();
    svgCache[path] = content;
    return content;
}

// Password toggle functionality
function togglePassword(inputId) {
    const input = document.getElementById(inputId);
    const toggleButton = document.getElementById(`toggle${inputId.charAt(0).toUpperCase() + inputId.slice(1)}`);
    const img = toggleButton.querySelector('img');
    
    if (input.type === 'password') {
        input.type = 'text';
        img.src = '/icons/eye-slash.svg';
        toggleButton.setAttribute('aria-label', 'Hide password');
    } else {
        input.type = 'password';
        img.src = '/icons/eye.svg';
        toggleButton.setAttribute('aria-label', 'Show password');
    }
}

// Show error message
function showError(message, elementId = 'message') {
    const messageDiv = document.getElementById(elementId);
    messageDiv.textContent = message;
    messageDiv.className = 'mt-4 text-center text-sm text-red-600';
}

// Show success message
function showSuccess(message, elementId = 'message') {
    const messageDiv = document.getElementById(elementId);
    messageDiv.textContent = message;
    messageDiv.className = 'mt-4 text-center text-sm text-green-600';
}

// Format password validation error messages
function formatPasswordError(errorMessage) {
    if (errorMessage.includes("Passwords must have at least one non alphanumeric character")) {
        return "Password must contain at least one special character (e.g., !@#$%^&*)";
    } else if (errorMessage.includes("Passwords must have at least one digit")) {
        return "Password must contain at least one number";
    } else if (errorMessage.includes("Passwords must have at least one uppercase")) {
        return "Password must contain at least one uppercase letter";
    } else if (errorMessage.includes("Passwords must have at least one lowercase")) {
        return "Password must contain at least one lowercase letter";
    } else if (errorMessage.includes("Passwords must be at least")) {
        return "Password must be at least 8 characters long";
    }
    return errorMessage;
}

// Check if user is already logged in
function checkAuth() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token) {
        window.location.href = '/dashboard.html';
    }
}

// Decode JWT token
function decodeToken(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error decoding token:', error);
        return null;
    }
}

// Get user info from token
function getUserInfo() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (!token) return null;
    return decodeToken(token);
} 