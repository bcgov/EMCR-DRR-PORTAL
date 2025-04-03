import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatTabNavPanel, MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { ProjectListComponent } from './project-list/project-list.component';
import { SubmissionListComponent } from './submission-list/submission-list.component';

export interface Tab {
  type: TabType;
  label: string;
  link: string;
}

export enum TabType {
  SUBMISSIONS = 'submissions',
  PROJECTS = 'projects',
}

@Component({
  selector: 'drr-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatTabNavPanel,
    TranslocoModule,
    ProjectListComponent,
    SubmissionListComponent,
    RouterModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  route = inject(ActivatedRoute);

  tabType = TabType;

  activeTab: TabType = TabType.SUBMISSIONS;

  tabs: Tab[] = [
    {
      type: TabType.SUBMISSIONS,
      label: 'submissionsTitle',
      link: 'submissions',
    },
    {
      type: TabType.PROJECTS,
      label: 'projectsTitle',
      link: 'projects',
    },
  ];

  ngOnInit() {
    const currentPath = this.route.snapshot.firstChild?.url[0]?.path;
    if (currentPath == TabType.PROJECTS) {
      this.activeTab = TabType.PROJECTS;
    } else {
      this.activeTab = TabType.SUBMISSIONS;
    }
  }
}
