import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'drr-drif-forecast',
  standalone: true,
  imports: [],
  templateUrl: './drif-forecast-view.component.html',
  styleUrl: './drif-forecast-view.component.scss',
})
export class DrifForecastViewComponent {
  route = inject(ActivatedRoute);

  projectId?: string;
  forecastId?: string;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.forecastId = params['forecastId'];
    });
  }
}
