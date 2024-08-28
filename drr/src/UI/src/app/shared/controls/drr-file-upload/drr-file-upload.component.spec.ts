import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrrFileUploadComponent } from './drr-file-upload.component';

describe('DrrFileUploadComponent', () => {
  let component: DrrFileUploadComponent;
  let fixture: ComponentFixture<DrrFileUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrrFileUploadComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DrrFileUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
