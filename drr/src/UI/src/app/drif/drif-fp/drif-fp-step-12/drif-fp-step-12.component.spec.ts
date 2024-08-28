import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep12Component } from './drif-fp-step-12.component';

describe('DrifFpStep12Component', () => {
  let component: DrifFpStep12Component;
  let fixture: ComponentFixture<DrifFpStep12Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep12Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep12Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
