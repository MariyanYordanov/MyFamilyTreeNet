// Family Tree Viewer JavaScript

// Global variables
let familyMembers = [];
let familyRelationships = [];
let currentZoom = 1;

// Initialize family tree viewer
function initializeFamilyTreeViewer(members, relationships) {
    familyMembers = members || [];
    familyRelationships = relationships || [];
    
    setupZoomControls();
    
    // Render with a small delay to allow loading animation
    setTimeout(function() {
        renderFamilyTree();
    }, 500);
}


// Setup zoom controls
function setupZoomControls() {
    const zoomInBtn = document.getElementById('zoomIn');
    const zoomOutBtn = document.getElementById('zoomOut');
    const zoomResetBtn = document.getElementById('zoomReset');
    
    const zoomStep = 0.2;
    const minZoom = 0.5;
    const maxZoom = 2;
    
    if (zoomInBtn) {
        zoomInBtn.addEventListener('click', function() {
            if (currentZoom < maxZoom) {
                currentZoom += zoomStep;
                applyZoom();
            }
        });
    }
    
    if (zoomOutBtn) {
        zoomOutBtn.addEventListener('click', function() {
            if (currentZoom > minZoom) {
                currentZoom -= zoomStep;
                applyZoom();
            }
        });
    }
    
    if (zoomResetBtn) {
        zoomResetBtn.addEventListener('click', function() {
            currentZoom = 1;
            applyZoom();
        });
    }
}

// Render the family tree
function renderFamilyTree() {
    const container = document.getElementById('familyTreeContainer');
    if (!container) return;
    
    // Clear loading indicator
    container.innerHTML = '';
    
    if (!familyMembers || familyMembers.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-sitemap fa-4x text-muted mb-3"></i>
                <h5 class="text-muted">Няма данни за семейно дърво</h5>
                <p class="text-muted">Добавете роднински връзки за да се покаже дървото</p>
            </div>
        `;
        return;
    }
    
    // Build simplified tree structure
    const generations = getSimplifiedTree();
    
    if (generations.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-sitemap fa-4x text-muted mb-3"></i>
                <h5 class="text-muted">Няма данни за семейно дърво</h5>
                <p class="text-muted">Добавете роднински връзки за да се покаже дървото</p>
            </div>
        `;
        return;
    }
    
    // Header
    container.innerHTML = `
        <div class="tree-header text-center mb-4">
            <h4 class="text-primary">
                <i class="fas fa-sitemap me-2"></i>
                Семейно дърво
            </h4>
            <p class="text-muted">Интерактивна визуализация на семейните връзки</p>
        </div>
    `;
    
    // Render generations
    generations.forEach((generation, genIndex) => {
        const generationHtml = renderGeneration(generation, genIndex);
        container.insertAdjacentHTML('beforeend', generationHtml);
    });
}

// Build simplified tree structure - matching the inline implementation
function getSimplifiedTree() {
    if (!familyMembers || familyMembers.length === 0 || !familyRelationships) {
        return [];
    }

    const generations = [];
    const processedMembers = new Set();
    
    // Find spouse relationships (relationshipType === 3)
    const spouseRelationships = familyRelationships.filter(rel => rel.relationshipType === 3);
    const couples = [];
    
    // Create couples from spouse relationships
    spouseRelationships.forEach(rel => {
        const member1 = familyMembers.find(m => m.id === rel.primaryMemberId);
        const member2 = familyMembers.find(m => m.id === rel.relatedMemberId);
        
        if (member1 && member2 && !processedMembers.has(member1.id) && !processedMembers.has(member2.id)) {
            const children = getChildrenOfCouple(member1.id, member2.id);
            
            couples.push({
                member1: member1,
                member2: member2,
                children: children,
                generation: 0
            });
            
            processedMembers.add(member1.id);
            processedMembers.add(member2.id);
        }
    });
    
    // Add single members who have children
    familyMembers.forEach(member => {
        if (!processedMembers.has(member.id)) {
            const children = getChildren(member.id);
            if (children.length > 0) {
                couples.push({
                    member1: member,
                    member2: null,
                    children: children,
                    generation: 0
                });
                processedMembers.add(member.id);
            }
        }
    });
    
    // If no couples found but we have members, show them as single units
    if (couples.length === 0) {
        familyMembers.forEach(member => {
            if (!processedMembers.has(member.id)) {
                couples.push({
                    member1: member,
                    member2: null,
                    children: getChildren(member.id),
                    generation: 0
                });
            }
        });
    }
    
    if (couples.length > 0) {
        generations.push(couples);
    }
    
    return generations;
}


// Get children of a member
function getChildren(memberId) {
    const childRelationships = familyRelationships.filter(
        rel => rel.relationshipType === 1 && rel.primaryMemberId === memberId
    );
    
    return childRelationships
        .map(rel => familyMembers.find(m => m.id === rel.relatedMemberId))
        .filter(member => member !== undefined);
}

// Get children of a couple
function getChildrenOfCouple(parent1Id, parent2Id) {
    const parent1Children = getChildren(parent1Id);
    const parent2Children = getChildren(parent2Id);
    
    // Find common children (children of both parents)
    return parent1Children.filter(child => 
        parent2Children.some(p2Child => p2Child.id === child.id)
    );
}

// Render a generation
function renderGeneration(generation, genIndex) {
    const generationLabels = [
        'Основатели', 
        'Първо поколение', 
        'Второ поколение', 
        'Трето поколение', 
        'Четвърто поколение'
    ];
    const generationLabel = generationLabels[genIndex] || `${genIndex + 1}-то поколение`;
    
    let html = `
        <div class="generation-container mb-5" style="background: white; border-radius: 1rem; padding: 2rem; box-shadow: 0 4px 6px rgba(0,0,0,0.1); border: 1px solid #e9ecef;">
            <div class="text-center mb-4">
                <span class="badge bg-primary" style="font-size: 1rem; padding: 0.75rem 1.5rem; border-radius: 2rem; font-weight: 600;">${generationLabel}</span>
            </div>
            <div class="couples-container d-flex justify-content-center flex-wrap gap-4 mb-4" style="min-height: 200px;">
    `;
    
    generation.forEach(couple => {
        html += renderCouple(couple);
    });
    
    html += `
            </div>
        </div>
    `;
    
    return html;
}

// Render a couple
function renderCouple(couple) {
    let html = `
        <div class="couple-unit" style="background: #f8f9fa; border-radius: 1rem; padding: 1.5rem; border: 2px solid #e9ecef; min-width: 300px;">
            <div class="couple-display d-flex align-items-center gap-3" style="justify-content: center;">
    `;
    
    // Member 1
    html += renderMember(couple.member1);
    
    // Marriage connection if there's a spouse
    if (couple.member2) {
        html += `
            <div class="marriage-connection d-flex flex-column align-items-center" style="margin: 0 1rem;">
                <div style="width: 3px; height: 40px; background: linear-gradient(to bottom, #e91e63, #ff6b9d); border-radius: 2px;"></div>
                <div style="color: #e91e63; font-size: 1.5rem; font-weight: bold; margin: 0.25rem 0; text-shadow: 0 1px 3px rgba(0,0,0,0.2);">♥</div>
                <div style="font-size: 0.7rem; color: #6c757d; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;">женени</div>
            </div>
        `;
        
        // Member 2 (spouse)
        html += renderMember(couple.member2);
    }
    
    html += `
            </div>
    `;
    
    // Children indicator
    if (couple.children && couple.children.length > 0) {
        const childrenText = couple.children.length === 1 ? 'дете' : 'деца';
        const childrenNames = couple.children.map(child => `${child.firstName} ${child.lastName}`).join(', ');
        
        html += `
            <div class="children-indicator text-center mt-3" style="border-top: 2px solid #28a745; padding-top: 1rem; margin-top: 1rem;">
                <div style="width: 2px; height: 20px; background: #28a745; margin: 0 auto 0.5rem;"></div>
                <div class="badge bg-success" style="font-size: 0.8rem; padding: 0.5rem 1rem; border-radius: 2rem; display: inline-block;">
                    <i class="fas fa-baby me-1"></i>
                    ${couple.children.length} ${childrenText}
                </div>
                <div class="text-muted small mt-1" style="margin-top: 0.5rem; font-style: italic; max-width: 250px; margin-left: auto; margin-right: auto;">
                    ${childrenNames}
                </div>
            </div>
        `;
    }
    
    html += `
        </div>
    `;
    
    return html;
}

// Render a member
function renderMember(member) {
    const genderClass = member.gender === 'Male' ? 'male' : 'female';
    const genderIcon = member.gender === 'Male' ? 'fa-mars' : 'fa-venus';
    const borderColor = member.gender === 'Male' ? '#007bff' : '#e91e63';
    const bgGradient = member.gender === 'Male' ? 'linear-gradient(135deg, #ffffff 0%, #e3f2fd 100%)' : 'linear-gradient(135deg, #ffffff 0%, #fce4ec 100%)';
    const avatarGradient = member.gender === 'Male' ? 'linear-gradient(45deg, #007bff, #0056b3)' : 'linear-gradient(45deg, #e91e63, #ad1457)';
    
    // Photo or avatar section
    let photoSection;
    if (member.profileImageUrl) {
        photoSection = `
            <img src="${member.profileImageUrl}" 
                 alt="${member.firstName} ${member.lastName}"
                 class="member-image" 
                 style="width: 70px; height: 70px; border-radius: 12px; object-fit: cover; border: 2px solid rgba(255,255,255,0.8); box-shadow: 0 2px 8px rgba(0,0,0,0.15); margin: 0 auto 0.75rem; display: block;">
        `;
    } else {
        photoSection = `
            <div class="member-avatar" style="width: 70px; height: 70px; border-radius: 12px; background: ${avatarGradient}; display: flex; align-items: center; justify-content: center; color: white; font-size: 1.8rem; margin: 0 auto 0.75rem; border: 2px solid rgba(255,255,255,0.8); box-shadow: 0 2px 8px rgba(0,0,0,0.15);">
                <i class="fas ${genderIcon}"></i>
            </div>
        `;
    }
    
    return `
        <div class="tree-node ${genderClass}" style="background: white; border: 2px solid ${borderColor}; background: ${bgGradient}; border-radius: 1rem; padding: 1rem; width: 180px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); transition: all 0.3s ease; cursor: pointer;" 
             data-member-id="${member.id}" onclick="showMemberDetails(${member.id})">
            <div class="member-photo" style="width: 70px; height: 70px; margin: 0 auto 0.75rem; position: relative;">
                ${photoSection}
            </div>
            <div class="member-info text-center" style="margin-bottom: 0.75rem;">
                <div class="member-name" style="font-weight: 600; font-size: 0.9rem; color: #333; line-height: 1.2;">${member.firstName} ${member.lastName}</div>
                <div class="member-details" style="margin-top: 0.25rem;">
                    <small class="text-muted">${member.gender === 'Male' ? 'Мъж' : 'Жена'}</small>
                </div>
            </div>
            <div class="member-actions text-center">
                <button class="btn btn-outline-primary btn-sm" onclick="event.stopPropagation(); showMemberDetails(${member.id})">
                    <i class="fas fa-eye"></i>
                </button>
            </div>
        </div>
    `;
}

// Apply zoom transformation
function applyZoom() {
    const container = document.getElementById('familyTree');
    if (!container) return;
    
    container.style.transform = `scale(${currentZoom})`;
    container.style.transformOrigin = 'top left';
}

// Show member details modal
function showMemberDetails(memberId) {
    const member = familyMembers.find(m => m.id === memberId);
    
    if (!member) {
        alert('Данните за този член не са намерени.');
        return;
    }
    
    const genderText = member.gender === 'Male' ? 'Мъж' : 'Жена';
    const birthDate = member.dateOfBirth ? new Date(member.dateOfBirth).toLocaleDateString('bg-BG') : 'Не е указана';
    const age = member.dateOfBirth ? calculateAge(member.dateOfBirth) : 'Неизвестна';
    
    const memberDetails = `
        <div class="text-center mb-3">
            <i class="fas fa-user-circle fa-4x text-primary"></i>
            <h5 class="mt-2">${member.firstName} ${member.middleName || ''} ${member.lastName}</h5>
        </div>
        <div class="row mb-3">
            <div class="col-6">
                <strong>Пол:</strong><br>
                <span class="text-muted">${genderText}</span>
            </div>
            <div class="col-6">
                <strong>Възраст:</strong><br>
                <span class="text-muted">${age} години</span>
            </div>
        </div>
        <div class="row mb-3">
            <div class="col-12">
                <strong>Дата на раждане:</strong><br>
                <span class="text-muted">${birthDate}</span>
            </div>
        </div>
        ${member.placeOfBirth ? `
        <div class="row mb-3">
            <div class="col-12">
                <strong>Място на раждане:</strong><br>
                <span class="text-muted">${member.placeOfBirth}</span>
            </div>
        </div>
        ` : ''}
        ${member.biography ? `
        <hr>
        <div class="mb-2">
            <strong>Биография:</strong><br>
            <span class="text-muted">${member.biography}</span>
        </div>
        ` : ''}
    `;

    // Update modal content
    const modal = document.getElementById('memberDetailsModal');
    if (!modal) return;
    
    const modalBody = modal.querySelector('.modal-body');
    if (modalBody) {
        modalBody.innerHTML = memberDetails;
    }
    
    // Show modal using Bootstrap 5
    const memberModal = new bootstrap.Modal(modal);
    memberModal.show();
}


// Calculate age helper function
function calculateAge(dateOfBirth) {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
        age--;
    }
    return age;
}

// Export for global use
window.initializeFamilyTreeViewer = initializeFamilyTreeViewer;
window.showMemberDetails = showMemberDetails;