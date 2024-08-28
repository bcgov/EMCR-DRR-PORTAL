import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep2Component } from './drif-fp-step-2.component';

describe('DrifFpStep2Component', () => {
  let component: DrifFpStep2Component;
  let fixture: ComponentFixture<DrifFpStep2Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep2Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifFpStep2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
