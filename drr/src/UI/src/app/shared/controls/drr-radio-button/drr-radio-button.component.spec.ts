import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrrRadioButtonComponent } from './drr-radio-button.component';

describe('DrrRadioButtonComponent', () => {
  let component: DrrRadioButtonComponent;
  let fixture: ComponentFixture<DrrRadioButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrrRadioButtonComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrrRadioButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
