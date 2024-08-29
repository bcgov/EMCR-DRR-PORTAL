import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep5Component } from './drif-eoi-step-5.component';

describe('Step5Component', () => {
  let component: DrifEoiStep5Component;
  let fixture: ComponentFixture<DrifEoiStep5Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep5Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep5Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
