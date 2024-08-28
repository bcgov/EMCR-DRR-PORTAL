import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep4Component } from './drif-eoi-step-4.component';

describe('Step4Component', () => {
  let component: DrifEoiStep4Component;
  let fixture: ComponentFixture<DrifEoiStep4Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep4Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep4Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
