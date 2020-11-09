import axios from '../utils/axios';
import concatUrl from '../utils/url';

export function sendSmsCode(params) {
    if (axios.defaults.withCredentials) {
        axios.defaults.withCredentials = false;
    }
    return axios.post(concatUrl('/api/Sms/Send'), params);
}

export function register(params) {
    return axios.post(concatUrl('/api/User/Register'), params);
}
