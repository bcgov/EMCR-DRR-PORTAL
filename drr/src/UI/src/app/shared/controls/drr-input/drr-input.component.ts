import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  Input,
  ViewChild,
  inject,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';
import { NgxMaskDirective } from 'ngx-mask';

export type InputType = 'text' | 'tel' | 'email';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-input',
  template: `
    <mat-label *ngIf="isMobile">{{ label }}{{ getMandatoryMark() }}</mat-label>
    <mat-form-field class="drr-input" *transloco="let t">
      <mat-label *ngIf="!isMobile">{{ label }}</mat-label>
      <input
        id="{{ id }}"
        #drrInput
        matInput
        [formControl]="rxFormControl"
        [maxlength]="getMaxLength"
        required="{{ isRequired() }}"
        [type]="type"
        (focus)="onFocus()"
        (blur)="onBlur()"
        [mask]="getMask()"
        [decimalMarker]="'.'"
        [thousandSeparator]="''"
      />
      <mat-hint *ngIf="isEmail() && !rxFormControl.value" align="start">{{
        t('emailExample')
      }}</mat-hint>
      <mat-hint *ngIf="maxlength && isFocused" align="end"
        >{{ getCount() }} / {{ maxlength }}</mat-hint
      >
      <mat-error *ngIf="rxFormControl.hasError('email')">{{
        t('emailError')
      }}</mat-error>
      <mat-error *ngIf="rxFormControl.hasError('required')">
        Field is required
      </mat-error>
    </mat-form-field>
  `,
  styles: [
    `
      .drr-input {
        width: 100%;
      }

      :host {
        .drr-input
          ::ng-deep
          .mdc-text-field--outlined.mdc-text-field--disabled
          .mdc-text-field__input {
          color: var(--mdc-outlined-text-field-input-text-color);
        }

        ::ng-deep .mdc-text-field--outlined.mdc-text-field--disabled {
          .mdc-floating-label,
          .mdc-floating-label--float-above {
            color: var(--mdc-outlined-text-field-label-text-color);
          }
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
    NgxMaskDirective,
    TranslocoModule,
  ],
})
export class DrrInputComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  isFocused = false;
  isMobile = false;

  @Input() label = '';
  @Input() id = '';
  @Input() maxlength?: string | number | null;
  @Input() type: InputType = 'text';

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  get getMaxLength() {
    if (this.type === 'tel') {
      return null;
    }

    return this.maxlength ?? null;
  }

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

  changeDetector = inject(ChangeDetectorRef);

  @ViewChild('drrInput') private drrInput: any;

  ngAfterViewInit() {
    this.changeDetector.detectChanges();
  }

  getCount(): number {
    const inputValue = this.rxFormControl?.value ?? '';
    return inputValue.toString().length;
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : !!this.rxFormControl?.validator?.({})?.required;
  }

  onFocus() {
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
  }

  getMask() {
    if (this.type === 'tel') {
      return '000-000-0000';
    }

    return '';
  }

  @HostListener('keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    const inputChar = String.fromCharCode(event.charCode);
    this.handleInputEvent(event, inputChar);
  }

  @HostListener('paste', ['$event'])
  handlePasteEvent(event: ClipboardEvent) {
    const value = event.clipboardData?.getData('text') ?? '';
    this.handleInputEvent(event, value);
  }

  private handleInputEvent(event: Event, inputValue: string) {
    if (this.type === 'tel') {
      // Allow numbers
      const pattern = /[0-9]/;

      if (!pattern.test(inputValue)) {
        // Invalid character, prevent input
        event.preventDefault();
      }
    }
  }

  isEmail() {
    return this.type === 'email';
  }

  focusOnInput() {
    const inputElement = this.drrInput.nativeElement;
    if (inputElement) {
      inputElement.focus();
    }
  }
}
