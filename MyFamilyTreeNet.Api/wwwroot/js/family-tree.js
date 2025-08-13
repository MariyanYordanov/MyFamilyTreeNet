// Family Tree D3.js Implementation

function initializeFamilyTree(familyId, members) {
    // Check if D3.js is loaded
    if (typeof d3 === 'undefined') {
        console.error('D3.js is not loaded');
        return;
    }
    const container = d3.select('#family-tree-container');
    const width = container.node().getBoundingClientRect().width;
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

    // Convert flat members array to hierarchical data
    function buildHierarchy(members) {
        const memberMap = {};
        members.forEach(m => {
            memberMap[m.id] = {
                id: m.id,
                name: `${m.firstName} ${m.lastName}`,
                birthDate: m.birthDate,
                deathDate: m.deathDate,
                children: []
            };
        });

        // Build parent-child relationships
        const roots = [];
        members.forEach(m => {
            if (m.fatherId || m.motherId) {
                const parentId = m.fatherId || m.motherId;
                if (memberMap[parentId]) {
                    memberMap[parentId].children.push(memberMap[m.id]);
                } else {
                    roots.push(memberMap[m.id]);
                }
            } else {
                roots.push(memberMap[m.id]);
            }
        });

        // Return single root or create artificial root
        if (roots.length === 1) {
            return roots[0];
        } else {
            return {
                id: 'root',
                name: 'Семейство',
                children: roots
            };
        }
    }

    const root = d3.hierarchy(buildHierarchy(members));
    
    // Create tree layout
    const treeLayout = d3.tree()
        .size([width - 100, height - 100]);

    treeLayout(root);

    // Draw links
    const links = g.selectAll('.link')
        .data(root.links())
        .enter()
        .append('path')
        .attr('class', 'link')
        .attr('d', d3.linkVertical()
            .x(d => d.x)
            .y(d => d.y));

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
            const birth = d.data.birthDate ? new Date(d.data.birthDate).getFullYear() : '?';
            const death = d.data.deathDate ? new Date(d.data.deathDate).getFullYear() : '';
            return death ? `${birth} - ${death}` : `р. ${birth}`;
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
        const newWidth = container.node().getBoundingClientRect().width;
        svg.attr('width', newWidth);
        treeLayout.size([newWidth - 100, height - 100]);
        treeLayout(root);
        
        // Update links and nodes positions
        links.attr('d', d3.linkVertical()
            .x(d => d.x)
            .y(d => d.y));
        nodes.attr('transform', d => `translate(${d.x},${d.y})`);
        
        centerTree();
    });
}