import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep6Component } from './drif-fp-step-6.component';

describe('DrifFpStep6Component', () => {
  let component: DrifFpStep6Component;
  let fixture: ComponentFixture<DrifFpStep6Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep6Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep6Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
