import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep3Component } from './drif-fp-step-3.component';

describe('DrifFpStep3Component', () => {
  let component: DrifFpStep3Component;
  let fixture: ComponentFixture<DrifFpStep3Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep3Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep3Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
