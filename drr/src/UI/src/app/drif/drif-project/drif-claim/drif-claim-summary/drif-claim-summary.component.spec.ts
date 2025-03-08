import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifClaimSummaryComponent } from './drif-claim-summary.component';

describe('DrifClaimSummaryComponent', () => {
  let component: DrifClaimSummaryComponent;
  let fixture: ComponentFixture<DrifClaimSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifClaimSummaryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DrifClaimSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
