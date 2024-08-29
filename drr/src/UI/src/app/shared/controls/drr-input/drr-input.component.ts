import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  Input,
  inject,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';
import { NgxMaskDirective } from 'ngx-mask';

export type InputType = 'text' | 'tel' | 'number' | 'email';

@Component({
  selector: 'drr-input',
  templateUrl: './drr-input.component.html',
  styleUrl: './drr-input.component.scss',
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
    if (this.type === 'tel' || this.type === 'number') {
      return null;
    }

    return this.maxlength ?? null;
  }

  get numberInputMin() {
    return this.type === 'number' ? 0 : undefined;
  }

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  changeDetector = inject(ChangeDetectorRef);

  ngAfterViewInit() {
    this.changeDetector.detectChanges();
  }

  getCount(): number {
    const inputValue = this.rxFormControl?.value ?? '';
    return inputValue.length;
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

    if (this.type === 'number') {
      // number input doesn't not support maxlength by default
      // so we need to add it manually to match text input behavior,
      // but allow decimal point to be moved around
      if (
        this.maxlength
          ? this.getCount() >= Number(this.maxlength) && inputValue !== '.'
          : false
      ) {
        event.preventDefault();
      }

      // Allow positive numbers and decimals
      const pattern = /^\d*\.?\d*$/;

      if (!pattern.test(inputValue)) {
        // Invalid character, prevent input
        event.preventDefault();
      }
    }
  }

  isEmail() {
    return this.type === 'email';
  }
}
