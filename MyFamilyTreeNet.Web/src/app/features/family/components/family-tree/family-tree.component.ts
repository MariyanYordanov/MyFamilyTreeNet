import { Component, OnInit, OnDestroy, Input, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FamilyService } from '../../services/family.service';
import * as d3 from 'd3';

export interface TreeNode {
  id: number;
  name: string;
  birthYear?: number;
  deathYear?: number;
  isAlive: boolean;
  age?: number;
  relationshipType?: string;
  children?: TreeNode[];
  x?: number;
  y?: number;
  depth?: number;
  height?: number;
  data?: any;
  parent?: TreeNode | null;
}

@Component({
  selector: 'app-family-tree',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './family-tree.component.html',
  styleUrls: ['./family-tree.component.scss']
})
export class FamilyTreeComponent implements OnInit, OnDestroy {
  @Input() familyId!: number;
  @ViewChild('treeContainer', { static: true }) treeContainer!: ElementRef;

  private familyService = inject(FamilyService);
  private router = inject(Router);
  private svg: any;
  private g: any;
  private zoom: any;

  isLoading = false;
  error: string | null = null;
  treeData: TreeNode | null = null;

  ngOnInit() {
    if (this.familyId) {
      this.loadTreeData();
    }
  }

  ngOnDestroy() {
    if (this.svg) {
      this.svg.remove();
    }
  }

  private loadTreeData() {
    this.isLoading = true;
    this.error = null;

    // Call the same endpoint as MVC version
    this.familyService.getFamilyTreeData(this.familyId).subscribe({
      next: (data) => {
        this.treeData = data;
        this.isLoading = false;
        this.initializeTree();
      },
      error: (error) => {
        console.error('Error loading tree data:', error);
        this.error = 'Failed to load family tree data';
        this.isLoading = false;
      }
    });
  }

  private initializeTree() {
    if (!this.treeData || !this.treeContainer?.nativeElement) {
      return;
    }

    // Clear any existing tree
    if (this.svg) {
      this.svg.remove();
    }

    const container = d3.select(this.treeContainer.nativeElement);
    const containerRect = this.treeContainer.nativeElement.getBoundingClientRect();
    const width = Math.max(containerRect.width, 800);
    const height = 600;

    // Create SVG
    this.svg = container.append('svg')
      .attr('width', width)
      .attr('height', height)
      .attr('class', 'tree-svg');

    this.g = this.svg.append('g');

    // Add zoom functionality
    this.zoom = d3.zoom<SVGSVGElement, unknown>()
      .scaleExtent([0.1, 3])
      .on('zoom', (event) => {
        this.g.attr('transform', event.transform);
      });

    this.svg.call(this.zoom);

    // Create tree layout
    const root = d3.hierarchy(this.treeData);
    const treeLayout = d3.tree<TreeNode>().size([width - 100, height - 100]);
    
    treeLayout(root);

    // Draw links (connections between nodes)
    this.g.selectAll('.link')
      .data(root.links())
      .enter().append('path')
      .attr('class', 'link')
      .attr('d', (d: any) => {
        return `M${d.source.x},${d.source.y}
                C${d.source.x},${(d.source.y + d.target.y) / 2}
                 ${d.target.x},${(d.source.y + d.target.y) / 2}
                 ${d.target.x},${d.target.y}`;
      });

    // Draw nodes
    const node = this.g.selectAll('.node')
      .data(root.descendants())
      .enter().append('g')
      .attr('class', 'node')
      .attr('transform', (d: any) => `translate(${d.x},${d.y})`)
      .style('cursor', 'pointer')
      .on('click', (event: MouseEvent, d: any) => {
        event.preventDefault();
        this.onNodeClick(d.data);
      });

    // Add rectangles for nodes
    node.append('rect')
      .attr('width', 120)
      .attr('height', 80)
      .attr('x', -60)
      .attr('y', -40)
      .attr('rx', 5)
      .attr('class', 'node-rect');

    // Add names
    node.append('text')
      .attr('dy', '-10')
      .attr('text-anchor', 'middle')
      .attr('class', 'node-name')
      .text((d: any) => d.data.name);

    // Add birth/death years
    node.append('text')
      .attr('dy', '5')
      .attr('text-anchor', 'middle')
      .attr('class', 'node-years')
      .text((d: any) => {
        const birth = d.data.birthYear ? d.data.birthYear.toString() : '?';
        const death = d.data.deathYear ? ` - ${d.data.deathYear}` : (d.data.isAlive ? '' : ' - ?');
        return birth + death;
      });

    // Add relationship type
    node.append('text')
      .attr('dy', '20')
      .attr('text-anchor', 'middle')
      .attr('class', 'node-relationship')
      .text((d: any) => d.data.relationshipType || '');

    // Add age if alive
    node.append('text')
      .attr('dy', '35')
      .attr('text-anchor', 'middle')
      .attr('class', 'node-age')
      .text((d: any) => {
        if (d.data.isAlive && d.data.age) {
          return `${d.data.age} год.`;
        }
        return '';
      });

    // Center the tree
    this.centerTree();
  }

  centerTree() {
    if (!this.svg || !this.zoom) return;

    const containerRect = this.treeContainer.nativeElement.getBoundingClientRect();
    const width = containerRect.width;
    const height = 600;

    const bounds = this.g.node()?.getBBox();
    if (!bounds) return;

    const fullWidth = bounds.width;
    const fullHeight = bounds.height;
    const midX = bounds.x + fullWidth / 2;
    const midY = bounds.y + fullHeight / 2;

    if (fullWidth === 0 || fullHeight === 0) return;

    const scale = Math.min(width / fullWidth, height / fullHeight) * 0.8;
    const translate = [width / 2 - scale * midX, height / 2 - scale * midY];

    this.svg.transition()
      .duration(750)
      .call(
        this.zoom.transform,
        d3.zoomIdentity.translate(translate[0], translate[1]).scale(scale)
      );
  }

  private onNodeClick(nodeData: TreeNode) {
    // Navigate to member detail
    this.router.navigate(['/members', nodeData.id]);
  }

  refresh() {
    this.loadTreeData();
  }
}