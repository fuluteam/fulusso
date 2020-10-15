import axios from '../utils/axios';
import concatUrl from '../utils/url';

export function checkCodeValid(params) {
    return axios.post(concatUrl('/api/User/ResetPasswordValidate'), params);
}

export function resetPassword(params) {
    return axios.post(concatUrl('/api/User/ResetPassword'), params);
}
