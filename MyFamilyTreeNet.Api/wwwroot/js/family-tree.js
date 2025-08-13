// Family Tree D3.js Implementation

function getRelationshipText(relationshipType) {
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
        default: return 'връзка';
    }
}

function initializeFamilyTree(familyId, treeData) {
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
    const root = d3.hierarchy(treeData[0]);
    
    // Create tree layout
    const treeLayout = d3.tree()
        .size([width - 100, height - 100]);

    treeLayout(root);

    // Draw links
    const links = g.selectAll('.link')
        .data(root.links())
        .enter()
        .append('g')
        .attr('class', 'link-group');

    // Add the link path
    links.append('path')
        .attr('class', 'link')
        .attr('d', d3.linkVertical()
            .x(d => d.x)
            .y(d => d.y));

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
            if (d.target.data.relationshipType) {
                return getRelationshipText(d.target.data.relationshipType);
            }
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

    // Handle window resize
    window.addEventListener('resize', function() {
        if (containerNode.offsetParent) {
            const newRect = containerNode.getBoundingClientRect();
            const newWidth = newRect.width > 0 ? newRect.width : width;
            svg.attr('width', newWidth);
            treeLayout.size([newWidth - 100, height - 100]);
            treeLayout(root);
            
            // Update links and nodes positions
            links.select('path').attr('d', d3.linkVertical()
                .x(d => d.x)
                .y(d => d.y));
            
            links.select('.link-label')
                .attr('x', d => (d.source.x + d.target.x) / 2)
                .attr('y', d => (d.source.y + d.target.y) / 2 - 5);
            
            nodes.attr('transform', d => `translate(${d.x},${d.y})`);
            
            centerTree();
        }
    });
}