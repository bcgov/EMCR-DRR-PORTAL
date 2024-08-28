import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpStep11Component } from './drif-fp-step-11.component';

describe('DrifFpStep11Component', () => {
  let component: DrifFpStep11Component;
  let fixture: ComponentFixture<DrifFpStep11Component>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpStep11Component]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpStep11Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
