import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { BreakpointObserver } from '@angular/cdk/layout';
import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
} from '@angular/material/autocomplete';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoModule } from '@ngneat/transloco';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';
import { map, Observable, startWith } from 'rxjs';

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
  templateUrl: './drr-chip-autocomplete.component.html',
  styleUrl: './drr-chip-autocomplete.component.scss',
})
export class DrrChipAutocompleteComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  @Input()
  label = '';

  @Input()
  placeholder = '';

  @Input()
  options?: string[];

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

  // selectedOptions = signal<string[]>([]);
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
        option ? this._filter(option) : this.options!.slice()
      )
    );
  }

  addOption(event: MatChipInputEvent) {
    const value = (event.value || '').trim();

    if (!value || this.rxFormControl.value.includes(value)) {
      return;
    }

    event.chipInput!.clear();
    this.currentInputControl.setValue(null);

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

    const value = event.option.viewValue;

    event.option.deselect();

    if (!value || this.rxFormControl.value.includes(value)) {
      return;
    }

    this.rxFormControl.setValue(
      [...this.rxFormControl.value, event.option.viewValue],
      { emitEvent: false }
    );
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();

    return this.options!.filter((option) =>
      option.toLowerCase().includes(filterValue)
    );
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : !!this.rxFormControl?.validator?.({})?.required;
  }
}
