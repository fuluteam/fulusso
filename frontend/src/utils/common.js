import { getQueryString } from "./url";
const { host: { common: loginHost } } = window.configs;

/**
 * @desc 第三方登录跳转入口
 * @param {string} type
 */
const thirdPartyLogin = (type) => {
    const { location } = window;
    const auth = `${loginHost}/${type}`;
    let returnUrl =getQueryString('ReturnUrl');
    if(!returnUrl){
        returnUrl = `${loginHost}/connect/authorize/callback?client_id=10000001&redirect_uri=${encodeURIComponent(window.location.origin)}&response_type=code&scope=api&state=STATE`;
    }
    location.href = `${auth}?ReturnUrl=${encodeURIComponent(returnUrl)}`;
};

export default thirdPartyLogin;
