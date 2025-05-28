import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { BreakpointObserver } from '@angular/cdk/layout';
import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, ElementRef, inject, Input, ViewChild } from '@angular/core';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
} from '@angular/material/autocomplete';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';
import { map, Observable, startWith } from 'rxjs';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-chip-autocomplete',
  standalone: true,
  imports: [
    CommonModule,
    MatChipsModule,
    MatIconModule,
    TranslocoModule,
    MatFormFieldModule,
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    AsyncPipe,
  ],
  template: `
    <ng-container>
      <mat-label [class]="hasRequiredError() ? 'drr-label--error' : ''"
        >{{ label }}{{ getMandatoryMark() }}</mat-label
      >
      <mat-form-field style="width: 100%">
        <mat-chip-grid
          #chipGrid
          [formControl]="rxFormControl"
          required="{{ isRequired() }}"
        >
          @for (option of rxFormControl.value; track $index) {
            <mat-chip-row (removed)="removeOption($index)">
              {{ option }}
              <button matChipRemove [attr.aria-label]="'remove ' + option">
                <mat-icon>cancel</mat-icon>
              </button>
            </mat-chip-row>
          }
        </mat-chip-grid>
        <input
          placeholder="{{ placeholder }}"
          #currentInput
          [formControl]="currentInputControl"
          [maxlength]="maxlength"
          (focus)="onFocus()"
          (blur)="onBlur()"
          [matChipInputFor]="chipGrid"
          [matAutocomplete]="auto"
          [ariaMultiLine]="true"
          [matChipInputSeparatorKeyCodes]="separatorKeysCodes"
          (matChipInputTokenEnd)="addOption($event)"
        />
        <mat-hint *ngIf="maxlength && isFocused" align="end"
          >{{ getCount() }} / {{ maxlength }}</mat-hint
        >
        <mat-autocomplete
          #auto="matAutocomplete"
          (optionSelected)="optionSelected($event)"
        >
          @for (option of filteredOptions | async; track option) {
            <mat-option [value]="option">{{ option }}</mat-option>
          }
        </mat-autocomplete>
        <mat-error *ngIf="rxFormControl.hasError('required')">
          Field is required
        </mat-error>
      </mat-form-field>
    </ng-container>
  `,
  styles: [
    `
      :host ::ng-deep .mat-mdc-standard-chip .mdc-evolution-chip__cell--primary,
      :host
        ::ng-deep
        .mat-mdc-standard-chip
        .mdc-evolution-chip__action--primary,
      :host ::ng-deep .mat-mdc-standard-chip .mat-mdc-chip-action-label {
        overflow: hidden;
      }
    `,
  ],
})
export class DrrChipAutocompleteComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  isFocused = false;

  @Input()
  label = '';

  @Input()
  placeholder = '';

  @Input()
  options?: string[];

  @Input()
  maxlength = 200;

  onFocus() {
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
  }

  getCount() {
    return this.currentInputControl.value?.length ?? 0;
  }

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  separatorKeysCodes: number[] = [ENTER, COMMA];
  currentInputControl = new FormControl('');

  @ViewChild('currentInput', { static: true })
  currentInputElement!: ElementRef<HTMLInputElement>;

  filteredOptions?: Observable<string[]>;

  isMobile = false;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });

    this.rxFormControl.statusChanges.subscribe((status: any) => {
      if (status === 'DISABLED') {
        this.rxFormControl.setValue([], { emitEvent: false });
      }
    });

    this.filteredOptions = this.currentInputControl.valueChanges.pipe(
      startWith(null),
      map((option: string | null) =>
        option ? this._filter(option) : this.options!.slice(),
      ),
    );
  }

  addOption(event: MatChipInputEvent) {
    const value = (event.value || '').trim();

    if (!value || this.rxFormControl.value.includes(value)) {
      return;
    }

    event.chipInput!.clear();
    this.currentInputControl.setValue('');

    this.rxFormControl.setValue([...this.rxFormControl.value, value], {
      emitEvent: false,
    });
  }

  removeOption(index: number) {
    const options = [...this.rxFormControl.value];
    options.splice(index, 1);
    this.rxFormControl.setValue(options, { emitEvent: false });
  }

  optionSelected(event: MatAutocompleteSelectedEvent) {
    this.currentInputControl.setValue('');
    this.currentInputElement.nativeElement.value = '';

    let value = event.option.viewValue;
    if (value.includes('Press Enter to add')) {
      value = value.replace('Press Enter to add "', '').replace('"', '');
    }

    event.option.deselect();

    if (!value || this.rxFormControl.value.includes(value)) {
      return;
    }

    this.rxFormControl.setValue([...this.rxFormControl.value, value], {
      emitEvent: false,
    });
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();

    const results = this.options!.filter((option) =>
      option.toLowerCase().includes(filterValue),
    );

    if (results.length === 0) {
      results.push(`Press Enter to add "${value}"`);
    }

    return results;
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : this.rxFormControl?.hasValidator(Validators.required);
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
  }

  hasRequiredError(): boolean {
    return (
      this.rxFormControl.hasError('required') &&
      !this.rxFormControl.disabled &&
      this.rxFormControl.touched
    );
  }
}
