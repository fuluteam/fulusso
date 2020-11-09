import * as service from '../services/bindAccount';
import CODE_OK from '../utils/Constants';

export default {
    namespace: 'bindAccount',
    state: {
        sendSmsLoading: false,
        loading: false,
        msg: null,
    },
    effects: {
        async sendSmsCode(payload) {
            this.setStore({
                sendSmsLoading: true,
            });
            try {
                const result = await service.sendSmsCode(payload);
                return result;
            } finally {
                this.setStore({
                    sendSmsLoading: false,
                });
            }
        },
        async smsBind(payload) {
            this.setStore({
                loading: true,
            });
            try {
                const result = await service.smsBind(payload);
                return result;
            } finally {
                this.setStore({
                    loading: false,
                });
            }
        },
        async accountBind(payload) {
            this.setStore({
                loading: true,
            });
            try {
                const result = await service.accountBind(payload);
                return result;
            } finally {
                this.setStore({
                    loading: false,
                });
            }
        },
    },
};
