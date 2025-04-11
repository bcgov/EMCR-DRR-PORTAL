import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { ProjectService } from '../../../api/project/project.service';
import { DraftDrrProject } from '../../../model';

@Component({
  selector: 'drr-project-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    RouterModule,
    TranslocoModule,
  ],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent {
  projectService = inject(ProjectService);

  projects?: DraftDrrProject[] = [];
  projectColumns = ['id', 'projectTitle', 'startDate', 'endDate'];
  projectListDataSource = new MatTableDataSource<DraftDrrProject>();

  ngOnInit() {
    this.projectService.projectGet().subscribe((projects) => {
      this.projects = projects.projects;
      this.projectListDataSource.data = this.projects!;
    });
  }

  onViewProjectClick(project: DraftDrrProject, event: any) {}

  onSortProjectTable(event: any) {}
}
