import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiStep6Component } from './drif-eoi-step-6.component';

describe('Step6Component', () => {
  let component: DrifEoiStep6Component;
  let fixture: ComponentFixture<DrifEoiStep6Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiStep6Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiStep6Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
