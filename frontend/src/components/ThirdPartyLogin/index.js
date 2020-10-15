/**
 * @desc 三方登录视图组件
 */
import React from 'react';

const ThirdPartyLogin = ({ onLogin }) => {
    function onLoginEnterClick(e) {
        const { type } = e.target.dataset;
        onLogin && onLogin(type);
    }
    return (
        <div className="third-party-login-box">
            <p>第三方账号登录:</p>
            <a
                className="login-icon wechat"
                id="wechat"
                data-type="wechat"
                title="使用微信登录"
                onClick={onLoginEnterClick}
            />
            <a
                className="login-icon ding"
                id="ding"
                data-type="ding"
                title="使用钉钉登录"
                onClick={onLoginEnterClick}
            />
            <a
                className="login-icon qy-wechat"
                id="workwechat"
                data-type="workwechat"
                title="使用企业微信登录"
                onClick={onLoginEnterClick}
            />
        </div>
    );
};

export default ThirdPartyLogin;
