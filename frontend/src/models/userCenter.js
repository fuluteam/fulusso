import { message } from 'antd';
import CODE_OK from '../utils/Constants';
import * as service from '../services/userCenter';

const execEffect = async (store, {
    effectName, dataKey, payload = {}, showError = true, loadStatusKey = 'loading', callback,
}) => {
    store.setStore({
        [loadStatusKey]: true,
    });
    const newStore = {
        [loadStatusKey]: false,
    };
    try {
        const result = await service[effectName](payload);
        if (result) {
            if (result.code === CODE_OK) {
                if (dataKey) {
                    newStore[dataKey] = result.data;
                }
                if (typeof callback === 'function') {
                    Object.assign(newStore, callback.call(store));
                }
            }
            if (result.code !== CODE_OK && showError && result.message) {
                message.error(result.message);
            }
        }
        return result;
    } finally {
        store.setStore(newStore);
    }
};

function updateBindStatus() {
    const user = this.getStore('user');
    return {
        user: Object.assign({}, user, { binding_status: user.binding_status === 0 ? 1 : 0 }),
    };
}

export default {
    namespace: 'userCenter',
    state: {
        user: null,
        thirdPartyBind: [],
        userIp: null,
        loading: false,
        validate: null,
        smsLoading: false,
    },
    effects: {
        updateStore(payload) {
            this.setStore(payload);
        },
        async fetchUser() {
            return execEffect(this, { effectName: 'fetchUser', dataKey: 'user' });
        },
        async fetchThirdPartyBind() {
            return execEffect(this, { effectName: 'fetchThirdPartyBind', dataKey: 'thirdPartyBind' });
        },
        async fetchUserIp() {
            return execEffect(this, { effectName: 'fetchUserIp', dataKey: 'userIp' });
        },
        async fetchAccessToken(payload) {
            return execEffect(this, { effectName: 'fetchAccessToken', dataKey: 'accessToken', payload });
        },
        async sendSmsCode(payload) {
            return execEffect(this, {
                effectName: 'sendSmsCode',
                payload,
                loadStatusKey: 'smsLoading',
                showError: false,
            });
        },
        async validateUser(payload) {
            return execEffect(this, { effectName: 'validateUser', dataKey: 'validate', payload });
        },
        async changePwd(payload) {
            return execEffect(this, { effectName: 'changePwd', payload });
        },
        async changePhone(payload) {
            return execEffect(this, {
                effectName: 'changePhone',
                payload,
                callback: function updatePhone() {
                    const user = this.getStore('user');
                    const newPhone = `${payload.phone.slice(0, 3)}****${payload.phone.slice(7)}`;
                    return { user: Object.assign({}, user, { cellphone: newPhone }) };
                },
            });
        },
        async fetchQrCode(payload) {
            return execEffect(this, { effectName: 'fetchQrCode', dataKey: 'qrCode', payload });
        },
        async bindApp(payload) {
            return execEffect(this, {
                effectName: 'bindApp',
                payload,
                callback: updateBindStatus,
            });
        },
        async unbindThirdParty(payload) {
            return execEffect(this, {
                effectName: 'unbindThirdParty',
                payload,
                callback: () => {
                    const thirdPartyBind = this.getStore('thirdPartyBind');
                    const _thirdPartyBind = thirdPartyBind || [];
                    const newArr = _thirdPartyBind.filter((item) => {
                        return item.loginProvider !== payload.loginProvider;
                    });
                    return {
                        thirdPartyBind: newArr,
                    };
                },
            });
        },
        async unBindApp(payload) {
            return execEffect(this, {
                effectName: 'unBindApp',
                payload,
                callback: updateBindStatus,
            });
        },
        async sendKey(payload) {
            const result = await service.sendKey(payload);
            return result;
        },
        async signOut() {
            const result = await service.signOut();
            return result;
        },
    },
};
