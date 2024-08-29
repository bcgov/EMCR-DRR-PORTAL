import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifSubmissionDetailsComponent } from './drif-eoi-summary.component';

describe('DrifSubmissionDetailsComponent', () => {
  let component: DrifSubmissionDetailsComponent;
  let fixture: ComponentFixture<DrifSubmissionDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifSubmissionDetailsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifSubmissionDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
