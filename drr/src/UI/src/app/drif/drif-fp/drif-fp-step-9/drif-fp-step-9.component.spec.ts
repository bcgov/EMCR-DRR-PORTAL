import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep9Component } from './drif-fp-step-9.component';

describe('DrifFpStep9Component', () => {
  let component: DrifFpStep9Component;
  let fixture: ComponentFixture<DrifFpStep9Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep9Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep9Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
