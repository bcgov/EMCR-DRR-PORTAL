import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrrDeclarationComponent } from './drr-declaration.component';

describe('DrrDeclarationComponent', () => {
  let component: DrrDeclarationComponent;
  let fixture: ComponentFixture<DrrDeclarationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrrDeclarationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DrrDeclarationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
