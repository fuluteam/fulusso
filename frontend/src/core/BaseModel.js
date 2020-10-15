import CODE_OK from '../utils/Constants';

/**
 * @desc  解析service层返回的数据
 * @param {object} result
 * @param {string} dataKey
 * @param {string|array|object} holdVal 占位默认数据
 */
const parseData = (result, dataKey, holdVal = {}) => {
    // 请求成功
    if (result && result.code === CODE_OK) {
        if (Object.hasOwnProperty.call(result, dataKey)) {
            return result[dataKey] || {};
        }
        const [, data] = Object.entries(result).find(([k]) => {
            return Object.hasOwnProperty.call(result[k], dataKey);
        });
        if (data) {
            return data[dataKey];
        }
        return holdVal;
    }
    return null;
};

export default parseData;
