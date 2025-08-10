// Form Helper Functions

// Password visibility toggle
function setupPasswordToggles() {
    const toggleButtons = document.querySelectorAll('[data-toggle-password]');
    
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetId = this.getAttribute('data-toggle-password');
            const passwordInputs = targetId ? 
                [document.getElementById(targetId)] : 
                document.querySelectorAll('input[type="password"], input[data-password-field]');
            
            let showPassword = false;
            
            passwordInputs.forEach(input => {
                if (input && (input.type === 'password' || input.getAttribute('data-password-field'))) {
                    showPassword = input.type === 'password';
                    input.type = showPassword ? 'text' : 'password';
                    input.setAttribute('data-password-field', 'true');
                }
            });
            
            // Update icon
            const icon = this.querySelector('i');
            if (icon) {
                icon.classList.toggle('fa-eye');
                icon.classList.toggle('fa-eye-slash');
            }
        });
    });
}

// Character counter for textareas
function setupCharacterCounters() {
    const textareas = document.querySelectorAll('[data-character-counter]');
    
    textareas.forEach(textarea => {
        const maxLength = parseInt(textarea.getAttribute('maxlength') || '1000');
        const counterId = textarea.getAttribute('data-character-counter');
        const counter = document.getElementById(counterId);
        
        if (counter) {
            // Initial count
            updateCounter(textarea, counter, maxLength);
            
            // Update on input
            textarea.addEventListener('input', function() {
                updateCounter(this, counter, maxLength);
            });
        }
    });
}

function updateCounter(textarea, counter, maxLength) {
    const currentLength = textarea.value.length;
    const remaining = maxLength - currentLength;
    
    counter.textContent = `${currentLength} / ${maxLength}`;
    
    // Change color based on remaining characters
    if (remaining < 50) {
        counter.classList.add('text-danger');
        counter.classList.remove('text-warning', 'text-muted');
    } else if (remaining < 100) {
        counter.classList.add('text-warning');
        counter.classList.remove('text-danger', 'text-muted');
    } else {
        counter.classList.add('text-muted');
        counter.classList.remove('text-danger', 'text-warning');
    }
}

// Date validation
function setupDateValidation() {
    const deathDateInputs = document.querySelectorAll('[data-validate-death-date]');
    
    deathDateInputs.forEach(deathInput => {
        const birthInputId = deathInput.getAttribute('data-validate-death-date');
        const birthInput = document.getElementById(birthInputId);
        
        if (birthInput) {
            deathInput.addEventListener('change', function() {
                validateDeathDate(birthInput, deathInput);
            });
            
            birthInput.addEventListener('change', function() {
                if (deathInput.value) {
                    validateDeathDate(birthInput, deathInput);
                }
            });
        }
    });
}

function validateDeathDate(birthInput, deathInput) {
    const birthDate = new Date(birthInput.value);
    const deathDate = new Date(deathInput.value);
    
    if (birthDate && deathDate && deathDate < birthDate) {
        deathInput.setCustomValidity('Датата на смърт не може да бъде преди датата на раждане');
        deathInput.classList.add('is-invalid');
        
        // Show error message
        let feedback = deathInput.nextElementSibling;
        if (!feedback || !feedback.classList.contains('invalid-feedback')) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            deathInput.parentNode.insertBefore(feedback, deathInput.nextSibling);
        }
        feedback.textContent = 'Датата на смърт не може да бъде преди датата на раждане';
    } else {
        deathInput.setCustomValidity('');
        deathInput.classList.remove('is-invalid');
        
        // Remove error message
        const feedback = deathInput.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.remove();
        }
    }
}

// Auto-hide alerts
function setupAutoHideAlerts() {
    const alerts = document.querySelectorAll('.alert[data-auto-hide]');
    
    alerts.forEach(alert => {
        const delay = parseInt(alert.getAttribute('data-auto-hide') || '5000');
        
        setTimeout(() => {
            // Use Bootstrap's alert close if available
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } else {
                // Fallback to jQuery fadeOut or native removal
                if (typeof $ !== 'undefined') {
                    $(alert).fadeOut(500, function() {
                        $(this).remove();
                    });
                } else {
                    alert.style.transition = 'opacity 0.5s';
                    alert.style.opacity = '0';
                    setTimeout(() => alert.remove(), 500);
                }
            }
        }, delay);
    });
}

// Contact form handler
function setupContactForm() {
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Съобщението е изпратено успешно! Ще се свържем с вас скоро.');
            this.reset();
        });
    }
}

// Initialize all form helpers
document.addEventListener('DOMContentLoaded', function() {
    setupPasswordToggles();
    setupCharacterCounters();
    setupDateValidation();
    setupAutoHideAlerts();
    setupContactForm();
});