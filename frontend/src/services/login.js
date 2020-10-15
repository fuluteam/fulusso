import axios, { setWithCredentials } from '../utils/axios';
import concatUrl from '../utils/url';

export function login(params) {
    setWithCredentials(true);
    return axios.post(concatUrl('/api/User/Login', true), params);
}

export function smsLogin(params) {
    setWithCredentials(true);
    return axios.post(concatUrl('/api/User/LoginBySms', true), params);
}
