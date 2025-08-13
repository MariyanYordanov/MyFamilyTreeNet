// World Family MVC Site JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Password visibility toggle
    setupPasswordToggles();

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function() {
        var alerts = document.querySelectorAll('.alert');
        alerts.forEach(function(alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Search functionality enhancement
    const searchInputs = document.querySelectorAll('input[type="search"], input[name="search"]');
    searchInputs.forEach(function(input) {
        input.addEventListener('input', function() {
            // Add search suggestions or real-time filtering here
            console.log('Searching for:', this.value);
        });
    });

    // Form validation enhancement
    const forms = document.querySelectorAll('form');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Family tree zoom functionality
    const zoomControls = document.querySelector('.zoom-controls');
    if (zoomControls) {
        let currentZoom = 1;
        const treeContainer = document.querySelector('.tree-container');
        
        zoomControls.addEventListener('click', function(e) {
            if (e.target.classList.contains('zoom-in')) {
                currentZoom = Math.min(currentZoom + 0.1, 2);
            } else if (e.target.classList.contains('zoom-out')) {
                currentZoom = Math.max(currentZoom - 0.1, 0.5);
            } else if (e.target.classList.contains('zoom-reset')) {
                currentZoom = 1;
            }
            
            if (treeContainer) {
                treeContainer.style.transform = `scale(${currentZoom})`;
            }
        });
    }

    // Family tree member click handlers
    const treeMembers = document.querySelectorAll('.tree-member');
    treeMembers.forEach(function(member) {
        member.addEventListener('click', function() {
            const memberId = this.dataset.memberId;
            const memberName = this.dataset.memberName;
            
            // Show member details in modal
            const modal = document.getElementById('memberModal');
            if (modal) {
                const modalTitle = modal.querySelector('.modal-title');
                const modalBody = modal.querySelector('.modal-body');
                
                if (modalTitle) modalTitle.textContent = memberName;
                if (modalBody) modalBody.innerHTML = `
                    <p><strong>ID:</strong> ${memberId}</p>
                    <p><strong>Име:</strong> ${memberName}</p>
                    <p>Повече информация за този член...</p>
                `;
                
                const bsModal = new bootstrap.Modal(modal);
                bsModal.show();
            }
        });
    });

    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('.btn-delete, [data-action="delete"]');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            const itemName = this.dataset.itemName || 'елемента';
            const confirmed = confirm(`Сигурни ли сте, че искате да изтриете ${itemName}?`);
            
            if (!confirmed) {
                e.preventDefault();
                return false;
            }
        });
    });

    // Loading states for buttons
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(function(button) {
        const form = button.closest('form');
        if (form) {
            form.addEventListener('submit', function(e) {
                // Only show loading if form is valid
                if (form.checkValidity()) {
                    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Обработва се...';
                    button.disabled = true;
                }
            });
        }
    });

    // Store original button text
    submitButtons.forEach(function(button) {
        button.dataset.originalText = button.innerHTML;
    });
});

// Global functions
function showSuccessMessage(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show';
    alertDiv.innerHTML = `
        <i class="fas fa-check-circle me-2"></i>${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
    }
}

function showErrorMessage(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show';
    alertDiv.innerHTML = `
        <i class="fas fa-exclamation-triangle me-2"></i>${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
    }
}

// Search functionality for help pages
function performSearch(query) {
    const searchResults = document.getElementById('searchResults');
    if (!searchResults) return;
    
    // Real search results based on help topics
    const helpResults = [
        { title: 'Как да създам семейство?', url: '/FamilyMvc/Create', snippet: 'Научете как да създадете ново семейство в системата...' },
        { title: 'Добавяне на членове', url: '/MemberMvc/Create', snippet: 'Стъпки за добавяне на нови членове към семейството...' },
        { title: 'Управление на семейства', url: '/FamilyMvc', snippet: 'Как да управлявате и редактирате семейна информация...' },
        { title: 'Преглед на членове', url: '/MemberMvc', snippet: 'Как да разглеждате и редактирате информация за членове...' },
        { title: 'Моят профил', url: '/Home/Dashboard', snippet: 'Достъп до вашия потребителски профил и статистики...' },
        { title: 'Контакти', url: '/Home/Contact', snippet: 'Как да се свържете с поддръжката...' }
    ];
    
    const filteredResults = helpResults.filter(result => 
        result.title.toLowerCase().includes(query.toLowerCase()) ||
        result.snippet.toLowerCase().includes(query.toLowerCase())
    );
    
    searchResults.innerHTML = filteredResults.map(result => `
        <div class="search-result mb-3">
            <h6><a href="${result.url}" class="text-decoration-none">${result.title}</a></h6>
            <p class="small text-muted">${result.snippet}</p>
        </div>
    `).join('');
    
    if (filteredResults.length === 0) {
        searchResults.innerHTML = '<p class="text-muted">Няма намерени резултати за "' + query + '"</p>';
    }
}

// Password visibility toggle function
function setupPasswordToggles() {
    const toggleButtons = document.querySelectorAll('.toggle-password, [data-toggle-password]');
    
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetId = this.getAttribute('data-toggle-password');
            let passwordInput;
            
            if (targetId) {
                passwordInput = document.getElementById(targetId);
            } else {
                passwordInput = this.parentElement.querySelector('input[type="password"], input[type="text"].password-field');
            }
            
            if (passwordInput) {
                const isPassword = passwordInput.type === 'password';
                passwordInput.type = isPassword ? 'text' : 'password';
                
                // Update icon
                const icon = this.querySelector('i');
                if (icon) {
                    if (isPassword) {
                        icon.classList.remove('fa-eye');
                        icon.classList.add('fa-eye-slash');
                    } else {
                        icon.classList.remove('fa-eye-slash');
                        icon.classList.add('fa-eye');
                    }
                }
            }
        });
    });
}