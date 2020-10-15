const { host: { api: apiHost, common: loginHost } } = window.configs;

const concatUrl = (url, login = false) => {
    return `${login ? loginHost : apiHost}${url}`;
};

const getQueryString = (name) => {
    const reg = new RegExp(`(^|&)${name}=([^&]*)(&|$)`);
    const r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return unescape(r[2]);
    }
    return null;
};

const getReturnUrl = () => {
    return getQueryString('ReturnUrl');
};

const toNavApp = () => {
    const returnUrl = getReturnUrl();
    if (returnUrl) {
        window.location.href = returnUrl;
    } else {
        window.location.href = `${loginHost}/connect/authorize/callback?client_id=10000001&redirect_uri=${encodeURIComponent(window.location.origin)}&response_type=code&scope=api&state=STATE`;
    }
};

export default concatUrl;

export {
    toNavApp,
    getReturnUrl,
    getQueryString,
};
