// Family Tree D3.js Implementation

function getRelationshipText(relationshipType) {
    // Handle Bulgarian relationship types from server
    if (relationshipType) {
        // If already translated from server, return as is
        if (typeof relationshipType === 'string' && /[а-я]/.test(relationshipType)) {
            return relationshipType;
        }
    }
    
    switch(relationshipType) {
        case 'Parent': return 'родител';
        case 'Child': return 'дете';
        case 'Spouse': return 'съпруг/а';
        case 'Sibling': return 'брат/сестра';
        case 'Grandparent': return 'дядо/баба';
        case 'Grandchild': return 'внук/внучка';
        case 'GreatGrandparent': return 'прадядо/прабаба';
        case 'GreatGrandchild': return 'правнук/правнучка';
        case 'Uncle': return 'чичо/вуйчо';
        case 'Aunt': return 'леля/тетка';
        case 'Nephew': return 'племенник';
        case 'Niece': return 'племенничка';
        case 'Cousin': return 'братовчед/сестричка';
        case 'StepParent': return 'доведен родител';
        case 'StepChild': return 'доведено дете';
        case 'StepSibling': return 'доведен брат/сестра';
        case 'HalfSibling': return 'полубрат/полусестра';
        case 'Other': return 'друго';
        default: return relationshipType || 'връзка';
    }
}

// Store tree state
let currentTreeData = null;
let currentFamilyId = null;

function initializeFamilyTree(familyId, treeData) {
    // Store data for resize
    currentTreeData = treeData;
    currentFamilyId = familyId;
    
    // Check if D3.js is loaded
    if (typeof d3 === 'undefined') {
        console.error('D3.js is not loaded');
        return;
    }
    
    const container = d3.select('#family-tree-container');
    if (!container.node()) {
        console.error('Family tree container not found');
        return;
    }
    
    const containerNode = container.node();
    if (!containerNode.offsetParent) {
        console.error('Family tree container is hidden');
        return;
    }
    
    const rect = containerNode.getBoundingClientRect();
    const width = rect.width > 0 ? rect.width : 800; // fallback width
    const height = 600;

    // Clear existing content
    container.selectAll('*').remove();

    // Create SVG
    const svg = container.append('svg')
        .attr('width', width)
        .attr('height', height)
        .attr('class', 'tree-svg');

    const g = svg.append('g');

    // Add zoom functionality
    const zoom = d3.zoom()
        .scaleExtent([0.1, 3])
        .on('zoom', function(event) {
            g.attr('transform', event.transform);
        });

    svg.call(zoom);

    // Use the tree data directly (already hierarchical from server)
    console.log('Tree data received:', treeData);
    const root = d3.hierarchy(treeData);
    
    // Create tree layout - inverted (children on top)
    const treeLayout = d3.tree()
        .size([width - 100, height - 100]);

    treeLayout(root);
    
    // Invert the y coordinates to flip the tree (children on top)
    const maxY = Math.max(...root.descendants().map(d => d.y));
    root.descendants().forEach(d => {
        d.y = maxY - d.y + 50; // Add some padding from top
    });

    // Draw links
    const links = g.selectAll('.link')
        .data(root.links())
        .enter()
        .append('g')
        .attr('class', 'link-group');
        
    // Add spouse links (between nodes with same parent)
    const spouseLinks = [];
    root.descendants().forEach(node => {
        if (node.data.spouseId && node.parent) {
            // Find spouse node
            const spouse = node.parent.children.find(child => child.data.id === node.data.spouseId);
            if (spouse && node.data.id < spouse.data.id) { // Avoid duplicates
                spouseLinks.push({source: node, target: spouse, isSpouse: true});
            }
        }
    });
    
    // Draw spouse links
    const spouseLinkGroups = g.selectAll('.spouse-link')
        .data(spouseLinks)
        .enter()
        .append('g')
        .attr('class', 'link-group spouse-link-group');
        
    spouseLinkGroups.append('path')
        .attr('class', 'link spouse')
        .attr('d', d => `M${d.source.x},${d.source.y} L${d.target.x},${d.target.y}`)
        .attr('stroke-width', 4);
        
    // Add labels to spouse links
    spouseLinkGroups.append('text')
        .attr('class', 'link-label')
        .attr('x', d => (d.source.x + d.target.x) / 2)
        .attr('y', d => (d.source.y + d.target.y) / 2 - 5)
        .attr('text-anchor', 'middle')
        .attr('font-size', '10px')
        .attr('fill', '#e91e63')
        .text('Съпруг/а');
        
    // Add white background to spouse labels
    spouseLinkGroups.selectAll('.link-label')
        .each(function() {
            const bbox = this.getBBox();
            d3.select(this.parentNode)
                .insert('rect', '.link-label')
                .attr('x', bbox.x - 2)
                .attr('y', bbox.y - 1)
                .attr('width', bbox.width + 4)
                .attr('height', bbox.height + 2)
                .attr('fill', 'white')
                .attr('fill-opacity', 0.8)
                .attr('stroke', '#e91e63')
                .attr('stroke-width', 0.5)
                .attr('rx', 2);
        });

    // Add the link path
    links.append('path')
        .attr('class', function(d) {
            // Check if this is a spouse relationship
            const isSpouse = d.target.data.relationshipType && 
                           (d.target.data.relationshipType.includes('Съпруг') || 
                            d.target.data.relationshipType.includes('Съпруга'));
            if (isSpouse || Math.abs(d.source.y - d.target.y) < 10) {
                return 'link spouse';
            } else {
                return 'link parent-child';
            }
        })
        .attr('d', function(d) {
            // Check if this is a spouse relationship (nodes on same level)
            if (Math.abs(d.source.y - d.target.y) < 10) {
                // Horizontal line for spouses
                return `M${d.source.x},${d.source.y} L${d.target.x},${d.target.y}`;
            } else {
                // Vertical link for parent-child
                return d3.linkVertical()
                    .x(d => d.x)
                    .y(d => d.y)(d);
            }
        })
        .attr('stroke-width', 4);

    // Add relationship labels on links
    links.append('text')
        .attr('class', 'link-label')
        .attr('x', d => (d.source.x + d.target.x) / 2)
        .attr('y', d => (d.source.y + d.target.y) / 2 - 5)
        .attr('text-anchor', 'middle')
        .attr('font-size', '10px')
        .attr('fill', '#666')
        .attr('background', 'white')
        .text(d => {
            // Try to determine relationship from data
            console.log('Link data:', d.target.data);
            console.log('Full target data:', JSON.stringify(d.target.data, null, 2));
            if (d.target.data.relationshipType) {
                // The relationshipType should already be in Bulgarian from the server
                return d.target.data.relationshipType;
            }
            console.log('No relationshipType found, using default');
            return 'връзка';
        });

    // Add white background to labels for better readability
    links.selectAll('.link-label')
        .each(function() {
            const bbox = this.getBBox();
            d3.select(this.parentNode)
                .insert('rect', '.link-label')
                .attr('x', bbox.x - 2)
                .attr('y', bbox.y - 1)
                .attr('width', bbox.width + 4)
                .attr('height', bbox.height + 2)
                .attr('fill', 'white')
                .attr('fill-opacity', 0.8)
                .attr('stroke', '#ddd')
                .attr('stroke-width', 0.5)
                .attr('rx', 2);
        });

    // Draw nodes
    const nodes = g.selectAll('.node')
        .data(root.descendants())
        .enter()
        .append('g')
        .attr('class', 'node')
        .attr('transform', d => `translate(${d.x},${d.y})`);

    // Add rectangles for nodes
    nodes.append('rect')
        .attr('width', 120)
        .attr('height', 60)
        .attr('x', -60)
        .attr('y', -30)
        .attr('rx', 5);

    // Add text to nodes
    nodes.append('text')
        .attr('dy', 0)
        .text(d => d.data.name);

    // Add birth/death dates
    nodes.append('text')
        .attr('dy', 20)
        .attr('font-size', '10px')
        .attr('fill', '#666')
        .text(d => {
            const birth = d.data.birthYear || '?';
            const death = d.data.deathYear || '';
            return death ? `${birth} - ${death}` : d.data.isAlive !== false ? `р. ${birth}` : birth;
        });

    // Add click handlers
    nodes.on('click', function(event, d) {
        if (d.data.id !== 'root') {
            window.location.href = `/MemberMvc/Details/${d.data.id}`;
        }
    });

    // Center the tree initially
    centerTree();

    function centerTree() {
        const bounds = g.node().getBBox();
        const fullWidth = width;
        const fullHeight = height;
        const widthScale = fullWidth / bounds.width;
        const heightScale = fullHeight / bounds.height;
        const scale = 0.8 * Math.min(widthScale, heightScale);
        
        const translate = [
            fullWidth / 2 - scale * (bounds.x + bounds.width / 2),
            fullHeight / 2 - scale * (bounds.y + bounds.height / 2)
        ];

        svg.transition()
            .duration(750)
            .call(zoom.transform, d3.zoomIdentity
                .translate(translate[0], translate[1])
                .scale(scale));
    }

}

// Debounced resize handler - prevents rotation during resize
let resizeTimeout;
window.addEventListener('resize', function() {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(function() {
        if (currentTreeData && currentFamilyId) {
            const container = document.getElementById('family-tree-container');
            if (container && container.offsetParent) {
                // Redraw the tree with same data
                initializeFamilyTree(currentFamilyId, currentTreeData);
            }
        }
    }, 300); // Wait 300ms after resize stops
});