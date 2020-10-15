import CODE_OK from '../utils/Constants';
import * as service from '../services/resetPassword';

export default {
    namespace: 'resetPassword',
    state: {
        validatePriority: null,
        loading: false,
    },
    effects: {
        async checkCodeValid(payload) {
            this.setStore({
                loading: true,
                validatePriority: null,
            });
            const result = await service.checkCodeValid(payload);
            const newStore = {
                loading: false,
            };
            if (result && result.code === CODE_OK) {
                newStore.validatePriority = result.data;
            }
            this.setStore(newStore);
            return result;
        },
        async resetPassword(payload) {
            this.setStore({
                loading: true,
            });
            try {
                return await service.resetPassword(payload);
            } catch(e) {

            } finally {
                this.setStore({
                    loading: false,
                });
            }
        },
    },
};
