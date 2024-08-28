import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep4Component } from './drif-fp-step-4.component';

describe('DrifFpStep4Component', () => {
  let component: DrifFpStep4Component;
  let fixture: ComponentFixture<DrifFpStep4Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep4Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep4Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
