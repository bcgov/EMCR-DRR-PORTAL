import { CommonModule } from '@angular/common';
import { Component, inject, isDevMode } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { VersionService } from '../../../api/version/version.service';
import { VersionInformation } from '../../../model';
import { ConfigurationStore } from '../../store/configuration.store';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-footer',
  standalone: true,
  imports: [CommonModule, MatToolbarModule, TranslocoModule, RouterModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  versionService = inject(VersionService);

  configStore = inject(ConfigurationStore);

  get homeUrl() {
    return this.configStore.configuration()!.oidc?.postLogoutRedirectUri;
  }

  isDevMode = isDevMode();
  versions?: VersionInformation[];

  get uiVersion() {
    return this.versions?.find((v) => v.name === 'EMCR.DRR.UI')?.version;
  }

  get apiVersion() {
    return this.versions?.find((v) => v.name === 'EMCR.DRR.API')?.version;
  }

  ngOnInit() {
    this.versionService.versionGetVersionInformation().subscribe((version) => {
      this.versions = version;
    });
  }
}
