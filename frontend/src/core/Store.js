import { extendObservable, action, toJS } from 'mobx';

let instance = null;

class Store {
    static getInstance() {
        if (instance === null) {
            instance = new Store();
            extendObservable(instance, {});
        }
        return instance;
    }

    @action.bound updateStore(storeKey, newStore = {}) {
        const componentStore = instance[storeKey];
        Object.keys(newStore).forEach((dataKey) => {
            if (!Object.hasOwnProperty.call(componentStore, dataKey)) {
                extendObservable(componentStore, {
                    [dataKey]: newStore[dataKey],
                });
            } else {
                componentStore[dataKey] = newStore[dataKey];
            }
        });
    }
}

const { updateStore } = Store.getInstance();

const Register = (storeKey, initialStore = {}, effects = {}) => {
    if (!Object.hasOwnProperty.call(instance, storeKey)) {
        instance[storeKey] = {};
        const store = Object.assign({}, initialStore);
        extendObservable(instance[storeKey], store);
        const obj = {
            getStore(dataKey) {
                const viewStore = instance[storeKey];
                return toJS(viewStore[dataKey] || viewStore);
            },
            setStore(newStore) {
                updateStore(storeKey, newStore);
            },
        };
        Object.keys(effects).forEach((effectName) => {
            effects[effectName] = effects[effectName].bind(obj);
        });
    }
    return instance[storeKey];
};

export { Register, updateStore };

export default Store;
