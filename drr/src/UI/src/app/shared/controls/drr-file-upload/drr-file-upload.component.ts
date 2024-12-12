import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import { NgxFileDropEntry, NgxFileDropModule } from 'ngx-file-drop';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-file-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, NgxFileDropModule, TranslocoModule],
  templateUrl: './drr-file-upload.component.html',
  styleUrl: './drr-file-upload.component.scss',
})
export class DrrFileUploadComponent {
  hotToast = inject(HotToastService);
  translocoService = inject(TranslocoService);

  @Input()
  multiple = true;

  @Input()
  useDropzone = true;

  @Input()
  attachButtonLabel = this.translocoService.translate('attach');

  @Output()
  filesSelected: EventEmitter<File[]> = new EventEmitter<File[]>();

  allowedExtensions = [
    'doc',
    'docx',
    'xls',
    'xlsx',
    'ppt',
    'pptx',
    'pdf',
    'txt',
    'csv',
    'kml',
    'kmz',
    'shp',
    'xml',
    'las',
    'laz',
    'png',
    'jpg',
    'jpeg',
    'mp4',
    'avi',
    'mov',
  ];
  allowedExtensionsString = this.allowedExtensions.join(', ');

  async filesDropped(files: NgxFileDropEntry[]) {
    const filesToEmit: File[] = await Promise.all(
      files.map((file) =>
        this.getFileFromEntry(file.fileEntry as FileSystemFileEntry)
      )
    );

    this.onFilesSelected(filesToEmit);
  }

  async filesSelectedFromInput(event: any) {
    const filesToEmit: File[] = [...event.target.files];
    this.onFilesSelected(filesToEmit);
  }

  private getFileFromEntry(fileEntry: FileSystemFileEntry): Promise<File> {
    return new Promise((resolve, reject) => {
      fileEntry.file((file: File) => resolve(file), reject);
    });
  }

  onFilesSelected(files: File[]) {
    // if not multiple, only take the first file
    if (!this.multiple) {
      files.splice(1);
    }

    const validFiles: File[] = [];
    files.forEach((file) => {
      // check if file size is less than 50MB
      if (file.size > 50 * 1024 * 1024) {
        this.hotToast.close();
        this.hotToast.error(
          `Please review your files. File ${file.name} size exceeds 50MB`
        );
        return;
      }

      const fileExtension = file.name.split('.').pop();
      if (!this.allowedExtensions.includes(fileExtension!)) {
        this.hotToast.close();
        this.hotToast.error(
          `Please review your files. File type .${fileExtension} is not supported`
        );
        return;
      }

      validFiles.push(file);
    });

    if (validFiles.length > 0) {
      this.filesSelected.emit(validFiles);
    }
  }
}
