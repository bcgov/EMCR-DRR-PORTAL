import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';

export interface EntitiesState {
  affectedParties?: string[];
  professionals?: string[];
  costReductions?: string[];
  coBenefits?: string[];
  resiliency?: string[];
  verificationMethods?: string[];
}

type EntitiesStore = {
  entities: EntitiesState;
};

const initialState: EntitiesState = {
  affectedParties: [],
  professionals: [],
  costReductions: [],
  coBenefits: [],
  resiliency: [],
  verificationMethods: [],
};

export const EntitiesStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    setEntities(entities: EntitiesState) {
      patchState(store, entities);
    },
    getEntities() {
      return store;
    },
  }))
);
