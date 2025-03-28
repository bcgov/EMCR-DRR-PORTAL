import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-datepicker',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatDatepickerModule,
    TranslocoModule,
  ],
  templateUrl: './drr-datepicker.component.html',
  styleUrl: './drr-datepicker.component.scss',
})
export class DrrDatepickerComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  @Input() label = '';
  @Input() id = '';
  @Input() min?: Date | string;
  @Input() max?: Date | string;
  @Input() minErrorLabel = '';
  @Input() maxErrorLabel = '';

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  @Input()
  set disabled(disabled: boolean) {
    disabled ? this.rxFormControl.disable() : this.rxFormControl.enable();
  }

  isMobile = false;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : !!this.rxFormControl?.validator?.({})?.required;
  }
}
