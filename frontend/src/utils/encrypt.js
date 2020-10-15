/* eslint-disable max-len */

let rsaKeyPair = null;
const layzInit = () => {
    if (!rsaKeyPair) {
        window.setMaxDigits(130);
        rsaKeyPair = new window.RSAKeyPair('10001', '', 'A28AF4D78D316AA27E4BD2696304B15075FED7E648FF6DD75C0BCD173461258EA9201406A6777703FB10197EB9271830FEEE0F21BD5C76496F769BABFA932EA1DDEB835C58FCD9CC67F157D78D0B34FEDF702A5D27F7E180B037A8E6D967AB26398750DBA9EDF3CAF9A45F5843E0647C82F9C7FFB75841DC4B4E9E6F7DFB5B47');
    }
};
const encryptPassword = (password) => {
    layzInit();
    return window.encryptedString(rsaKeyPair, window.base64encode(password));
};

export default encryptPassword;
