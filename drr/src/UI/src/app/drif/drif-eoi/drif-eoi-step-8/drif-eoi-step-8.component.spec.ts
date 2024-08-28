import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep8Component } from './drif-eoi-step-8.component';

describe('Step8Component', () => {
  let component: DrifEoiStep8Component;
  let fixture: ComponentFixture<DrifEoiStep8Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep8Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep8Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
