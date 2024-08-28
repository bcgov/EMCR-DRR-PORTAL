import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrifEoiViewComponent } from './drif-eoi-view.component';

describe('DrifSubmissionDetailsComponent', () => {
  let component: DrifEoiViewComponent;
  let fixture: ComponentFixture<DrifEoiViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrifEoiViewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DrifEoiViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
