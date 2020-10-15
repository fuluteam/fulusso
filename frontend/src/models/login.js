import * as service from '../services/login';

export default {
    namespace: 'login',
    state: {
        loading: false,
    },
    effects: {
        async login(payload) {
            this.setStore({
                loading: true,
            });
            try {
                const result = await service.login(payload);
                return result;
            } finally {
                this.setStore({
                    loading: false,
                });
            }
        },
        async smsLogin(payload) {
            this.setStore({
                loading: true,
            });
            try {
                const result = await service.smsLogin(payload);
                return result;
            } finally {
                this.setStore({
                    loading: false,
                });
            }
        },
    },
};
