import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep7Component } from './drif-eoi-step-7.component';

describe('Step7Component', () => {
  let component: DrifEoiStep7Component;
  let fixture: ComponentFixture<DrifEoiStep7Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep7Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep7Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
