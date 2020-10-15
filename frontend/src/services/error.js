import axios from '../utils/axios';
import concatUrl from '../utils/url';

export function fetchError(params) {
    return axios.get(concatUrl('/api/user/GetErrorMsg', true), { params });
}
