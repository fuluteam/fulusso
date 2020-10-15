import { message } from 'antd';
import CODE_OK from '../utils/Constants';
import * as service from '../services/register';

export default {
    namespace: 'register',
    state: {
        loading: false,
        registerSuccess: false,
    },
    effects: {
        async sendSmsCode(payload) {
            const result = await service.sendSmsCode(payload);
            return result;
        },
        async register(payload) {
            this.setStore({
                loading: true,
            });
            const newStore = {
                loading: false,
            };
            try {
                const result = await service.register(payload);
                const { code, message: msg } = result;
                if (code === CODE_OK) {
                    newStore.registerSuccess = true;
                } else if (msg) {
                    message.error(msg);
                }
            } catch(e) {

            } finally {
                this.setStore(newStore);
            }
        },
    },
};
