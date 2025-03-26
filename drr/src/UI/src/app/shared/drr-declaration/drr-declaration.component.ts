import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { DeclarationForm } from './drr-declaration-form';

@Component({
  selector: 'drr-declaration',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RxReactiveFormsModule,
    MatInputModule,
    TranslocoModule,
    MatFormFieldModule,
    MatCheckboxModule,
    ReactiveFormsModule,
  ],
  templateUrl: './drr-declaration.component.html',
  styleUrl: './drr-declaration.component.scss',
})
export class DrrDeclarationComponent {
  @Input() authorizedRepresentativeText?: string;
  @Input() accuracyOfInformationText?: string;

  @Input() declarationForm?: IFormGroup<DeclarationForm>;
}
