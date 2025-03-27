import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'drr-auto-save',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drr-auto-save.component.html',
  styleUrl: './drr-auto-save.component.scss',
})
export class DrrAutoSaveComponent {
  @Input() lastSavedAt?: Date;

  autoSaveCountdown = 0;
  autoSaveTimer: any;
  autoSaveInterval = 60;
}
