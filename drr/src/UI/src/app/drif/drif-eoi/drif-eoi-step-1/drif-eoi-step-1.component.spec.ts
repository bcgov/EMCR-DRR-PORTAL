import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep1Component } from './drif-eoi-step-1.component';

describe('DrifStep1Component', () => {
  let component: DrifEoiStep1Component;
  let fixture: ComponentFixture<DrifEoiStep1Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep1Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep1Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
