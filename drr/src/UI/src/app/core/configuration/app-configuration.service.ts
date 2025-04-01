import { Injectable, inject } from '@angular/core';
import { ConfigurationService } from '../../../api/configuration/configuration.service';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import { ConfigurationStore } from '../../store/configuration.store';
import { OptionsStore } from '../../store/options.store';

@Injectable({
  providedIn: 'root',
})
export class AppConfigurationService {
  configurationService = inject(ConfigurationService);
  drifAppService = inject(DrifapplicationService);
  configurationStore = inject(ConfigurationStore);
  optionsStore = inject(OptionsStore);

  async loadConfiguration() {
    return new Promise((resolve) =>
      this.configurationService.configurationGetConfiguration().subscribe(
        (config) => {
          this.configurationStore.setConfiguration(config);
          resolve(true);
        },
        (error) => {
          console.error('Error fetching appConfig', error);
          resolve(false);
        },
      ),
    );
  }

  // TODO: this could be re-implemented in the store
  async loadOptions() {
    return this.configurationService
      .configurationGetEntities()
      .subscribe((entities) => {
        this.optionsStore.setOptions({
          ...entities,
        });
      });
  }

  // TODO: this could be re-implemented in the store
  async loadDeclarations() {
    return new Promise((resolve) =>
      this.drifAppService.dRIFApplicationGetDeclarations().subscribe(
        (declarations) => {
          this.optionsStore.setDeclarations(declarations.items!);
          resolve(true);
        },
        (error) => {
          console.error('Error fetching declarations', error);
          resolve(false);
        },
      ),
    );
  }
}
