import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep3Component } from './drif-eoi-step-3.component';

describe('Step3Component', () => {
  let component: DrifEoiStep3Component;
  let fixture: ComponentFixture<DrifEoiStep3Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep3Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep3Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
