import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { FamilyService } from '../../services/family.service';
import { MemberService } from '../../../member/services/member.service';
import { Family } from '../../models/family.model';
import { Member } from '../../../member/models/member.model';
import { DateFormatPipe, NameFormatPipe, AgePipe } from '../../../../shared/pipes';

declare var d3: any;

@Component({
  selector: 'app-family-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DateFormatPipe, NameFormatPipe, AgePipe],
  templateUrl: './family-detail.component.html',
  styleUrl: './family-detail.component.scss'
})
export class FamilyDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  family = signal<Family | null>(null);
  members = signal<Member[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  familyId: number | null = null;
  
  statistics = computed(() => {
    const membersList = this.members();
    const totalMembers = membersList.length;
    const aliveMembers = membersList.filter(m => !m.dateOfDeath).length;
    const deceasedMembers = totalMembers - aliveMembers;
    
    const ages = membersList
      .filter(m => m.age !== undefined)
      .map(m => m.age!);
    const averageAge = ages.length > 0 
      ? Math.round(ages.reduce((sum, age) => sum + age, 0) / ages.length)
      : 0;
    
    const generations = this.calculateGenerations(membersList);
    
    return {
      totalMembers,
      aliveMembers,
      deceasedMembers,
      averageAge,
      generations
    };
  });

  constructor(
    private route: ActivatedRoute,
    private familyService: FamilyService,
    private memberService: MemberService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (params) => {
        const id = params.get('id');
        if (id) {
          this.familyId = parseInt(id);
          this.loadFamilyData(this.familyId);
        } else {
          this.error.set('Невалиден ID на семейство');
        }
      },
      error: (error) => {
        console.error('Error loading family:', error);
        this.error.set('Грешка при зареждане на семейството');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFamilyData(familyId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.familyService.getFamilyById(familyId).pipe(
      switchMap(family => {
        this.family.set(family);
        return this.memberService.getMembers({ familyId });
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        this.members.set(response.members);
        this.loading.set(false);
        setTimeout(() => this.renderFamilyTree(), 100);
      },
      error: (error) => {
        console.error('Error loading family data:', error);
        this.error.set('Грешка при зареждане на данните за семейството');
        this.loading.set(false);
      }
    });
  }

  private calculateGenerations(members: Member[]): number {
    return Math.max(...members.map(m => {
      const birthYear = m.dateOfBirth ? new Date(m.dateOfBirth).getFullYear() : new Date().getFullYear();
      return Math.floor((new Date().getFullYear() - birthYear) / 25);
    }), 0) + 1;
  }

  private renderFamilyTree(): void {
    if (!this.members().length) return;

    const container = document.getElementById('family-tree-container');
    if (!container) return;

    container.innerHTML = '';

    const width = container.offsetWidth;
    const height = Math.max(600, this.members().length * 80);

    const svg = d3.select(container)
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .attr('class', 'family-tree-svg');

    const g = svg.append('g');

    const zoom = d3.zoom()
      .scaleExtent([0.1, 3])
      .on('zoom', (event: any) => {
        g.attr('transform', event.transform);
      });

    svg.call(zoom);

    const treeData = this.buildTreeData(this.members());
    
    const root = d3.hierarchy(treeData, (d: any) => d.children);
    
    const treeLayout = d3.tree()
      .size([height - 100, width - 200]);

    treeLayout(root);

    const links = g.selectAll('.link')
      .data(root.links())
      .enter()
      .append('path')
      .attr('class', 'link')
      .attr('d', d3.linkHorizontal()
        .x((d: any) => d.y + 100)
        .y((d: any) => d.x + 50)
      )
      .style('fill', 'none')
      .style('stroke', '#ccc')
      .style('stroke-width', 2);

    const nodes = g.selectAll('.node')
      .data(root.descendants())
      .enter()
      .append('g')
      .attr('class', 'node')
      .attr('transform', (d: any) => `translate(${d.y + 100}, ${d.x + 50})`)
      .style('cursor', 'pointer')
      .on('click', (event: any, d: any) => {
        if (d.data.id) {
          window.open(`/members/${d.data.id}`, '_blank');
        }
      });

    nodes.append('circle')
      .attr('r', 25)
      .style('fill', (d: any) => d.data.isAlive ? '#28a745' : '#6c757d')
      .style('stroke', '#fff')
      .style('stroke-width', 3);

    nodes.append('text')
      .attr('dy', '0.35em')
      .attr('text-anchor', 'middle')
      .style('font-size', '10px')
      .style('font-weight', 'bold')
      .style('fill', 'white')
      .text((d: any) => {
        const names = d.data.name.split(' ');
        return names[0].substring(0, 1) + (names[1] ? names[1].substring(0, 1) : '');
      });

    nodes.append('text')
      .attr('x', 35)
      .attr('dy', '-10px')
      .style('font-size', '12px')
      .style('font-weight', 'bold')
      .text((d: any) => d.data.name);

    nodes.append('text')
      .attr('x', 35)
      .attr('dy', '5px')
      .style('font-size', '10px')
      .style('fill', '#666')
      .text((d: any) => {
        if (d.data.birthYear) {
          return d.data.isAlive ? `р. ${d.data.birthYear}` : `${d.data.birthYear} - ${d.data.deathYear || '?'}`;
        }
        return '';
      });

    nodes.append('text')
      .attr('x', 35)
      .attr('dy', '20px')
      .style('font-size', '9px')
      .style('fill', '#999')
      .text((d: any) => d.data.age ? `${d.data.age} г.` : '');

    const centerX = width / 2;
    const centerY = height / 2;
    svg.call(zoom.transform, d3.zoomIdentity.translate(centerX - root.y - 100, centerY - root.x - 50));
  }

  private buildTreeData(members: Member[]): any {
    const memberMap = new Map();
    members.forEach(member => {
      memberMap.set(member.id, {
        id: member.id,
        name: `${member.firstName} ${member.lastName}`,
        birthYear: member.dateOfBirth ? new Date(member.dateOfBirth).getFullYear() : null,
        deathYear: member.dateOfDeath ? new Date(member.dateOfDeath).getFullYear() : null,
        isAlive: !member.dateOfDeath,
        age: member.age,
        children: []
      });
    });

    const rootMember = members.find(m => !this.hasParents(m, members)) || members[0];
    
    if (!rootMember) {
      return { name: 'Няма данни', children: [] };
    }

    const root = memberMap.get(rootMember.id);
    this.buildChildrenRecursive(root, members, memberMap, new Set());

    return root;
  }

  private hasParents(member: Member, members: Member[]): boolean {
    return false;
  }

  private buildChildrenRecursive(node: any, members: Member[], memberMap: Map<any, any>, visited: Set<number>): void {
    if (visited.has(node.id)) return;
    visited.add(node.id);

    const children = members.filter(m => 
      !visited.has(m.id) && 
      m.id !== node.id &&
      Math.abs((m.age || 0) - (node.age || 0)) > 15
    ).slice(0, 3);

    children.forEach(child => {
      const childNode = memberMap.get(child.id);
      if (childNode) {
        node.children.push(childNode);
        this.buildChildrenRecursive(childNode, members, memberMap, visited);
      }
    });
  }

  refreshTree(): void {
    if (this.familyId) {
      this.loadFamilyData(this.familyId);
    }
  }

  exportTree(): void {
    const svg = document.querySelector('.family-tree-svg');
    if (svg) {
      const svgData = new XMLSerializer().serializeToString(svg);
      const svgBlob = new Blob([svgData], {type: 'image/svg+xml;charset=utf-8'});
      const svgUrl = URL.createObjectURL(svgBlob);
      const downloadLink = document.createElement('a');
      downloadLink.href = svgUrl;
      downloadLink.download = `${this.family()?.name || 'family'}-tree.svg`;
      document.body.appendChild(downloadLink);
      downloadLink.click();
      document.body.removeChild(downloadLink);
    }
  }
}