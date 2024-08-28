import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep8Component } from './drif-fp-step-8.component';

describe('DrifFpStep8Component', () => {
  let component: DrifFpStep8Component;
  let fixture: ComponentFixture<DrifFpStep8Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep8Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep8Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
