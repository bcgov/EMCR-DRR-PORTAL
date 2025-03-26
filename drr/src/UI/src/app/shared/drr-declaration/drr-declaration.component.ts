import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DeclarationForm } from './drr-declaration-form';

@Component({
  selector: 'drr-drr-declaration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drr-declaration.component.html',
  styleUrl: './drr-declaration.component.scss',
})
export class DrrDeclarationComponent {
  @Input() authorizedRepresentativeText?: string;
  @Input() accuracyOfInformationText?: string;

  @Input() declarationForm?: IFormGroup<DeclarationForm>;
}
