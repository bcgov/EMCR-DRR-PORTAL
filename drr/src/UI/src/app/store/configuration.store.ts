import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';

export interface ConfigurationState {
  configuration: {
    oidc?: {
      clientId?: string;
      issuer?: string;
      scope?: string;
      postLogoutRedirectUri?: string;
      accountRecoveryUrl?: string;
    };
    testDataEndpointsEnabled?: boolean;
  };

  isConfigurationLoaded: boolean;
}

type ConfigurationStore = {
  configuration: ConfigurationState;
};

const initialState: ConfigurationState = {
  configuration: {
    oidc: {
      clientId: '',
      issuer: '',
      scope: '',
      postLogoutRedirectUri: '',
      accountRecoveryUrl: '',
    },
    testDataEndpointsEnabled: false,
  },
  isConfigurationLoaded: false,
};

export const ConfigurationStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    setConfiguration(coniguration: ConfigurationState['configuration']) {
      patchState(store, {
        configuration: coniguration,
        isConfigurationLoaded: true,
      });
    },
  })),
);
