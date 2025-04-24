import { inject, Injectable } from '@angular/core';
import { AttachmentService } from '../../..';

export const RecordType = {
  FullProposal: 'FullProposal',
  ProgressReport: 'ProgressReport',
  Invoice: 'Invoice',
  ForecastReport: 'ForecastReport',
  ConditionRequest: 'ConditionRequest',
};

@Injectable({
  providedIn: 'root',
})
export class FileService {
  private attachmentsService = inject(AttachmentService);

  private contentTypes = {
    kml: 'application/vnd.google-earth.kml+xml',
    kmz: 'application/vnd.google-earth.kmz',
    las: 'application/vnd.las',
    laz: 'application/vnd.laszip',
    default: 'application/octet-stream',
  };

  downloadFile(fileId: string) {
    this.attachmentsService
      .attachmentDownloadAttachment(fileId, { observe: 'response' })
      .subscribe({
        next: (response) => {
          if (response == null) {
            console.error('No response received for file download.');
            return;
          }

          // Extract filename from Content-Disposition header
          const contentDisposition = response.headers.get(
            'Content-Disposition',
          );
          let filename = 'downloaded-file';
          if (contentDisposition) {
            const match = contentDisposition.match(/filename="(.+)"/);
            if (match && match[1]) {
              filename = match[1];
            }
          }

          const url = window.URL.createObjectURL(response.body!);
          const a = document.createElement('a');
          a.href = url;
          a.download = filename;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error downloading file:', error);
        },
      });
  }

  getCustomContentType(file: File): string {
    const fileExtension = file.name.split('.').pop();
    switch (fileExtension) {
      case 'kml':
        return this.contentTypes.kml;
      case 'kmz':
        return this.contentTypes.kmz;
      case 'las':
        return this.contentTypes.las;
      case 'laz':
        return this.contentTypes.laz;
      default:
        return this.contentTypes.default;
    }
  }
}
