import axios from 'axios';
import { message } from 'antd';

const toNavLogin = () => {
    message.error('授权失败，请重新登录');
    setTimeout(() => {
        localStorage.removeItem('access_token');
        window.location.href = '/login.html';
    }, 1000);
};

if (window.location.pathname === '/login.html') {
    axios.defaults.withCredentials = true;
}
axios.defaults.timeout = 30000;
axios.defaults.headers.post['Content-Type'] = 'application/json';

axios.interceptors.request.use((config) => {
    if (localStorage.getItem('access_token')) {
        config.headers.Authorization = `Bearer ${localStorage.getItem('access_token')}`;
    }
    // if (config.url.endsWith('api/User/GetUserInfo')) {
    // }
    return config;
}, (error) => {
    return Promise.reject(error);
});

axios.interceptors.response.use((response) => {
    return response.data;
}, (error) => {
    if (error.response) {
        switch (error.response.status) {
            case 400:
                message.error(`请求参数（data）格式错误（${error.config.method + error.config.url}）`);
                break;
            case 401:
                if (window.location.pathname !== '/' && window.location.pathname !== '/userCenter.html') {
                    toNavLogin();
                }
                break;
            case 504:
            case 403:
                toNavLogin();
                break;
            case 404:
                message.error(`请求 URL 格式错误（${error.config.url}）`);
                break;
            case 405:
                message.error(`请求 Method 格式错误（${error.config.url}）`);
                break;
            case 406:
                message.error(`请求 Content-Type 格式错误（${error.config.url}）`);
                break;
            case 408:
                message.warning(`请求超时（${error.config.url}）`);
                break;
            default:
                break;
        }
        const err = /^5\d{2}$/g;
        if (err.test(error.response.status)) {
            message.error('服务错误！');
        }
    } else if (error.message) {
        if (error.message === `timeout of ${error.config.timeout}ms exceeded`) {
            message.warning('请求超时，请刷新页面重新请求！');
        }
        if (error.message === 'Network Error') {
            message.error('请求错误！');
        }
    }
    return Promise.reject(error);
});

const setWithCredentials = (bool = false) => {
    axios.defaults.withCredentials = bool;
};

export default axios;

export {
    setWithCredentials,
};
