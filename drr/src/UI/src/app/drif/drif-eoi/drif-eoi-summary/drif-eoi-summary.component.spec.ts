import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiSummaryComponent } from './drif-eoi-summary.component';

describe('SummaryComponent', () => {
  let component: DrifEoiSummaryComponent;
  let fixture: ComponentFixture<DrifEoiSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiSummaryComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
