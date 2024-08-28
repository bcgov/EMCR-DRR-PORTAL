import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep2Component } from './drif-eoi-step-2.component';

describe('Step2Component', () => {
  let component: DrifEoiStep2Component;
  let fixture: ComponentFixture<DrifEoiStep2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep2Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
