import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrInputComponent } from '../controls/drr-input/drr-input.component';
import { AuthorizedRepresentativeForm } from './auth-rep-form';

@Component({
  selector: 'drr-auth-rep',
  standalone: true,
  imports: [CommonModule, MatInputModule, TranslocoModule, DrrInputComponent],
  templateUrl: './drr-auth-rep.component.html',
  styleUrl: './drr-auth-rep.component.scss',
})
export class DrrAuthRepComponent {
  @Input()
  authorizedRepresentativeForm?: IFormGroup<AuthorizedRepresentativeForm>;
}
