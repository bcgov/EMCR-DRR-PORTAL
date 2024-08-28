import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep5Component } from './drif-fp-step-5.component';

describe('DrifFpStep5Component', () => {
  let component: DrifFpStep5Component;
  let fixture: ComponentFixture<DrifFpStep5Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep5Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep5Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
