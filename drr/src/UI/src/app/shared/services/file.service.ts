import { inject, Injectable } from '@angular/core';
import { AttachmentService } from '../../..';

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
    this.attachmentsService.attachmentDownloadAttachment(fileId).subscribe({
      next: (response) => {
        if (response.file?.content == null) {
          // TODO: show error
          return;
        }

        // alternative way to download file
        // const byteArray = this.base64ToByteArray(response.file.content!);
        // const blob = new Blob([byteArray], { type: response.file.contentType });
        // saveAs(blob, response.file.fileName);

        const byteArray = this.base64ToByteArray(response.file.content!);
        const blob = new Blob([byteArray], {
          type: response.file.contentType,
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = response.file.fileName!;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {},
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

  private base64ToByteArray(base64: string): Uint8Array {
    const binaryString = window.atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
  }
}
