import axios from '../utils/axios';
import concatUrl from '../utils/url';

export function fetchUser() {
    return axios.get(concatUrl('/api/User/GetUserInfo'));
}

export function fetchAccessToken(params) {
    return axios.post(concatUrl('/api/authorization_code'), params);
}

export function fetchThirdPartyBind() {
    return axios.get(concatUrl('/api/ExternalUser/GetUsers'));
}

export function fetchUserIp() {
    return axios.get(concatUrl('/api/User/GetUserIpInfo'));
}

export function sendSmsCode(params) {
    return axios.post(concatUrl('/api/SmsCode/SendCode'), params);
}

export function unbindThirdParty(params) {
    return axios.post(concatUrl('/api/ExternalUser/UnbindUser'), params);
}

export function signOut() {
    return axios.post(concatUrl('/api/User/SignOut'));
}

export function validateUser(params) {
    return axios.post(concatUrl('/api/User/Validate'), params);
}

export function changePwd(params) {
    return axios.post(concatUrl('/api/User/ChangePwd'), params);
}

export function changePhone(params) {
    return axios.post(concatUrl('/api/User/ChangePhone'), params);
}

export function fetchQrCode() {
    return axios.get(concatUrl('/api/QrCode/Generate'));
}

export function bindApp(params) {
    return axios.post(concatUrl('/api/QrCode/Binding'), params);
}

export function unBindApp(params) {
    return axios.post(concatUrl('/api/QrCode/UnBinding'), params);
}

export function sendKey(params) {
    return axios.post(concatUrl('/api/QrCode/SendKey'), params);
}
