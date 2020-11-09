import axios, { setWithCredentials } from '../utils/axios';
import concatUrl from '../utils/url';

export function smsBind(params) {
    setWithCredentials(true);
    return axios.post(concatUrl('/api/User/LoginByCodeBind', true), params);
}

export function accountBind(params) {
    setWithCredentials(true);
    return axios.post(concatUrl('/api/User/LoginByPassBind', true), params);
}

export function sendSmsCode(params) {
    setWithCredentials(true);
    return axios.post(concatUrl('/api/Sms/Send'), params);
}
