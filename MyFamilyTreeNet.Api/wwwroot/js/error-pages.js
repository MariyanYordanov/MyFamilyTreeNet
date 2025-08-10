// Error Pages JavaScript

// Error logging function
function logError(error) {
    console.error('Application Error:', error);
    // In production, this would send to a logging service
}

// Auto-refresh for 500 error page
function setupAutoRefresh() {
    let refreshAttempts = 0;
    const maxAttempts = 3;
    
    // Check service status
    const serviceIndicator = document.querySelector('.service-status');
    if (serviceIndicator) {
        // Simulate service check
        setTimeout(() => {
            serviceIndicator.classList.add('heartbeat');
        }, 1000);
    }
    
    // Refresh button handler
    const refreshBtn = document.getElementById('refreshBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function() {
            refreshAttempts++;
            this.disabled = true;
            this.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Опресняване...';
            
            setTimeout(() => {
                location.reload();
            }, 1000);
        });
    }
    
    // Auto-refresh prompt
    if (window.location.pathname.includes('500')) {
        setTimeout(() => {
            if (refreshAttempts < maxAttempts) {
                const userConfirmed = confirm('Искате ли да опресним страницата автоматично?');
                if (userConfirmed) {
                    location.reload();
                }
            }
        }, 30000); // 30 seconds
    }
}

// Setup 404 page interactions
function setup404Page() {
    // Add hover effects
    const cards = document.querySelectorAll('.card-hover');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.boxShadow = '0 5px 15px rgba(0,0,0,0.2)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.boxShadow = '';
        });
    });
}

// Initialize error page functionality
document.addEventListener('DOMContentLoaded', function() {
    // Log the error
    if (window.location.pathname.includes('404')) {
        logError({
            type: '404',
            path: window.location.pathname,
            timestamp: new Date().toISOString()
        });
        setup404Page();
    } else if (window.location.pathname.includes('500')) {
        logError({
            type: '500',
            message: 'Internal Server Error',
            timestamp: new Date().toISOString()
        });
        setupAutoRefresh();
    }
});