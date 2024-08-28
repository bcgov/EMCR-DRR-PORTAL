import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpSummaryComponent } from './drif-fp-summary.component';

describe('DrifFpSummaryComponent', () => {
  let component: DrifFpSummaryComponent;
  let fixture: ComponentFixture<DrifFpSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpSummaryComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
