import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Input, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-textarea',
  template: `
    <mat-label>{{ label }}{{ getMandatoryMark() }}</mat-label>
    <mat-form-field class="drr-textarea">
      <textarea
        id="{{ id }}"
        matInput
        [formControl]="rxFormControl"
        [maxlength]="maxlength"
        [rows]="rows"
      ></textarea>
      <mat-hint *ngIf="maxlength" align="end"
        >{{ getCount() }} / {{ maxlength }}</mat-hint
      >
    </mat-form-field>
  `,
  styles: [
    `
      .drr-textarea {
        width: 100%;
        margin-top: 8px;
      }

      @media screen and (max-width: 767px) {
        .drr-textarea mat-label {
          white-space: normal;
        }
      }

      :host {
        .drr-textarea
          ::ng-deep
          .mdc-text-field--outlined.mdc-text-field--disabled
          .mdc-text-field__input {
          color: var(--mdc-outlined-text-field-input-text-color);
        }
      }
    `,
  ],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
})
export class DrrTextareaComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);
  changeDetector = inject(ChangeDetectorRef);

  isMobile = false;

  @Input() label = '';
  @Input() id = '';
  @Input() maxlength = 0;
  @Input() rows = 3;

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }

  @Input()
  set disabled(disabled: boolean) {
    disabled ? this.rxFormControl.disable() : this.rxFormControl.enable();
  }

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  ngAfterViewInit() {
    this.changeDetector.detectChanges();
  }

  get rxFormControl() {
    return this._formControl;
  }

  getCount(): number {
    return this.rxFormControl?.value?.length ?? 0;
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ||
      this.rxFormControl?.hasError('required')
      ? '*'
      : '';
  }
}
