import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep7Component } from './drif-fp-step-7.component';

describe('DrifFpStep7Component', () => {
  let component: DrifFpStep7Component;
  let fixture: ComponentFixture<DrifFpStep7Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep7Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifFpStep7Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
