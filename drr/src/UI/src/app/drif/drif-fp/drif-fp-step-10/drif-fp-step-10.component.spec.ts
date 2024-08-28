import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep10Component } from './drif-fp-step-10.component';

describe('DrifFpStep10Component', () => {
  let component: DrifFpStep10Component;
  let fixture: ComponentFixture<DrifFpStep10Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep10Component],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifFpStep10Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
