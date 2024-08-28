import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep1Component } from './drif-fp-step-1.component';

describe('DrifFpStep1Component', () => {
  let component: DrifFpStep1Component;
  let fixture: ComponentFixture<DrifFpStep1Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep1Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep1Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
