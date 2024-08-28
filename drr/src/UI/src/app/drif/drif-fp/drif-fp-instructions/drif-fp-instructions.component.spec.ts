import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifFpInstructionsComponent } from './drif-fp-instructions.component';

describe('DrifFpInstructionsComponent', () => {
  let component: DrifFpInstructionsComponent;
  let fixture: ComponentFixture<DrifFpInstructionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifFpInstructionsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrifFpInstructionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
