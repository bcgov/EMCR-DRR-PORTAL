import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrrChipAutocompleteComponent } from './drr-chip-autocomplete.component';

describe('DrrChipAutocompleteComponent', () => {
  let component: DrrChipAutocompleteComponent;
  let fixture: ComponentFixture<DrrChipAutocompleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrrChipAutocompleteComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrrChipAutocompleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
