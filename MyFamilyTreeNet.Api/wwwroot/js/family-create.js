// Family Create Form JavaScript

document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('familyCreateForm');
    const nameInput = document.getElementById('Name');
    const submitBtn = document.getElementById('submitBtn');
    
    if (!form || !nameInput || !submitBtn) return;

    // Form submission handler
    form.addEventListener('submit', function(e) {
        // Remove any existing validation classes
        nameInput.classList.remove('is-invalid');
        
        // Validate name field
        const nameValue = nameInput.value.trim();
        if (nameValue === '') {
            e.preventDefault();
            nameInput.classList.add('is-invalid');
            nameInput.focus();
            return false;
        }

        // Show loading state
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Създава се...';
        
        // Form will submit normally after this
        return true;
    });

    // Real-time validation for name field
    nameInput.addEventListener('input', function() {
        const value = this.value.trim();
        
        if (value.length === 0) {
            this.classList.remove('is-valid');
            this.classList.add('is-invalid');
        } else if (value.length >= 3) {
            this.classList.remove('is-invalid');
            this.classList.add('is-valid');
        } else {
            this.classList.remove('is-valid');
            this.classList.add('is-invalid');
        }
    });

    // Clear validation on focus
    nameInput.addEventListener('focus', function() {
        this.classList.remove('is-invalid');
    });
});