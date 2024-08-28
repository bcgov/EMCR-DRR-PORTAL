import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoModule } from '@ngneat/transloco';
import { NgxFileDropEntry, NgxFileDropModule } from 'ngx-file-drop';
import { FileForm } from '../../../drif/drif-fp/drif-fp-form';

@Component({
  selector: 'drr-file-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, NgxFileDropModule, TranslocoModule],
  templateUrl: './drr-file-upload.component.html',
  styleUrl: './drr-file-upload.component.scss',
})
export class DrrFileUploadComponent {
  @Input()
  multiple = true;

  @Input()
  useDropzone = true;

  @Output()
  filesSelected: EventEmitter<FileForm[]> = new EventEmitter<FileForm[]>();

  filesDropped(files: NgxFileDropEntry[]) {
    const filesToEmit: FileForm[] = [];
    files.map((file) => {
      const fileEntry = file.fileEntry as FileSystemFileEntry;
      fileEntry.file((f: File) => {
        const fileForm: FileForm = {
          name: f.name,
          type: f.type,
          id: f.name,
        };
        filesToEmit.push(fileForm);
      });
    });
    this.filesSelected.emit(filesToEmit);
  }

  filesSelectedFromInput(event: any) {
    const filesToEmit: FileForm[] = [];
    [...event.target.files].map((file: File) => {
      const fileForm: FileForm = {
        name: file.name,
        type: file.type,
        id: file.name,
      };
      filesToEmit.push(fileForm);
    });

    this.filesSelected.emit(filesToEmit);
  }
}
