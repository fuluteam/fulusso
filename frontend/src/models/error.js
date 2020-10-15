import * as service from '../services/error';
import CODE_OK from '../utils/Constants';

export default {
    namespace: 'error',
    state: {
        loading: false,
        msg: null,
    },
    effects: {
        async fetchError(payload) {
            this.setStore({
                loading: true,
            });
            const newStore = {
                loading: false,
            };
            try {
                const result = await service.fetchError(payload);
                if (result && result.code === CODE_OK) {
                    newStore.msg = result.message;
                }
                return result;
            } finally {
                this.setStore(newStore);
            }
        },
    },
};
