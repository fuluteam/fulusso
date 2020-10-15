const validatePassWord = (rule, value, callback) => {
    if (value == null) {
        callback('请输入密码');
        return;
    }
    let count = 0;
    if (/\d+/.test(value)) {
        count += 1;
    }
    if (/[a-zA-Z]+/.test(value)) {
        count += 1;
    }
    if (/[~!@#$ %^&* ()_ +`\-={}:";'<>?,.\/]+/.test(value)) {
        count += 1;
    }
    if (value.length < 6 || value.length > 20) {
        callback('密码为6-20个字符');
        return;
    }
    if (count < 2) {
        callback('字母、数字、特殊符号至少包含2种以上');
        return;
    }
    callback();
};

export default validatePassWord;
