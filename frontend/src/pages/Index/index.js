import React from 'react';
import { Button, Spin, message } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import { getQueryString } from '../../utils/url';
import thirdPartyLogin from '../../utils/common';
import CODE_OK from '../../utils/Constants';
import './index.less';

@model('userCenter')
class UserCenter extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            initFinish: false,
            visitorMode: true,
        };
        this.initCount = 0;
        this.onExit = this.onExit.bind(this);
        this.renderStatus = this.renderStatus.bind(this);
    }
    componentWillMount() {
       this.loadUserData();
    }
    async loadUserData() {
        const newState = {
            initFinish: true,
        };
        let isFetchTokenSuccess = false;
        if (!localStorage.getItem('access_token')) { // 本地没有token，重新请求获取token
            const code = getQueryString('code');
            if (!code) {
                this.setState(newState);
                return;
            }
            let tokenResult;
            try {
                tokenResult = await this.fetchAccessToken(code);
                if (tokenResult || tokenResult.code == CODE_OK) {
                    const { data: { access_token } } = tokenResult;
                    isFetchTokenSuccess = true;
                    localStorage.setItem('access_token', access_token);
                }
            } catch(e) {
            }
        } else {
            isFetchTokenSuccess = true;
        }
        if (!isFetchTokenSuccess) { // 获取token失败，页面为未登录模式
            this.setState(newState);
            return;
        }
        let user = null;
        try {
            user = await this.fetchUser();
            if (user && user.code === CODE_OK) {
                this.fetchThirdPartyBind();
                newState.visitorMode = false;
            }
        } catch (err) {
            if (this.initCount === 0) {
                localStorage.removeItem('access_token'); // 重新请求token
                this.loadUserData();
                this.initCount = 1;
            }
        }
        if (user && user.code == CODE_OK) { // 登录模式
            newState.visitorMode = false;
        }
        this.setState(newState);
    }
    /**
     * @desc 根据code获取token
     * @param {string} code
     */
    fetchAccessToken(code) {
        return this.dispatch({
            type: 'fetchAccessToken',
            payload: {
                redirectUri: location.origin,
                state: 'state',
                code,
            },
        });
    }
    /**
     * @desc 查询用户三方绑定状态
     */
    fetchThirdPartyBind() {
        return this.dispatch({
            type: 'fetchThirdPartyBind',
        });
    }
    fetchUser() {
        return this.dispatch({
            type: 'fetchUser',
        });
    }

    onNavToLogin() {
        window.location.href = `${window.location.origin}/login.html`;
    }

    renderStatus() {
        const { userCenter } = this.props;
        const { user } = userCenter;
        if (user) {
            return (
                <span className="user-info">
                    <span class="name">{user.phone}</span>
                    <a class="exit" onClick={this.onExit}>退出</a>
                </span>
            );
        } else {
            return (
                <Button
                    type="primary"
                    className="btn-login"
                    onClick={this.onNavToLogin}
                >
                    登录/注册
                </Button>
            );
        }
    }
    /**
     * @desc 用户退出
     */
    onExit() {
        localStorage.removeItem('access_token');
        window.location.href = `${window.configs.host.common}/api/user/Logout?returnurl=${encodeURIComponent(`${window.location.origin}/login.html`)}`;
    }
    /**
     * @desc 渲染三方登录绑定视图
     */
    renderThirdPartyItem(loginProvider = 'wechat') {
        const { userCenter: { thirdPartyBind = [] } } = this.props;
        const target = thirdPartyBind.find((item) => {
            return item.loginProvider === loginProvider;
        });
        return (
            <div className="third-party-item">
                <i className={`third-party-icon ${loginProvider}`}></i>
                <Button
                    disabled={loginProvider === 'wechat' || loginProvider === 'workwechat'}
                    onClick={() => {
                        if (target) { // 解绑
                            this.dispatch({
                                type: 'unbindThirdParty',
                                payload: {
                                    loginProvider,
                                },
                            }).then(({ code }) => {
                                if (code === CODE_OK) {
                                    message.success('解绑成功');
                                }
                            });
                        } else {
                            thirdPartyLogin(loginProvider);
                        }
                    }}
                >
                    {target ? '解绑' : '绑定'}
                </Button>
            </div>
        );
    }
    _render() {
        const { initFinish } = this.state;
        const { userCenter } = this.props;
        const { user } = userCenter;
        return (
            <Spin spinning={!initFinish}>
                <div className="main-wrapper">
                    <Header renderStatus={this.renderStatus} className="t-header" />
                    <div className="banner wrap">
                        <div className="ad">
                            <h3>葫芦藤安全中心</h3>
                            <div>简单、快速解决您的账号安全问题</div>
                        </div>
                        <div className="anim-box">
                            <div className="t"></div>
                            <div className="b"></div>
                        </div>
                    </div>
                    <div className="main-box">
                        <div className="menu-card-box">
                            <div className="menu-icon icon-key"/>
                            <div className="menu-name">密码管理</div>
                            <div className="menu-tips">修改用户登录密码及找回密码</div>
                            <div className="menu-list">
                                <Button
                                    onClick={() => {
                                        window.location.href="./resetPassword.html";
                                    }}
                                >重置密码</Button>
                            </div>
                        </div>
                        <div className="menu-card-box c-c">
                            <div className="menu-icon icon-mobile"/>
                            <div className="menu-name">手机管理</div>
                            <div className="menu-tips">修改用户认证手机号码</div>
                            <div className="menu-list"></div>
                        </div>
                        <div className="menu-card-box r-c">
                            <div className="menu-icon icon-third"/>
                            <div className="menu-name">第三方登录</div>
                            <div className="menu-tips">在第三方平台上已有的账号来快速完成登录</div>
                            <div className="menu-list third-party-list">
                                {this.renderThirdPartyItem()}
                                {this.renderThirdPartyItem('ding')}
                                {this.renderThirdPartyItem('workwechat')}
                            </div>
                        </div>
                    </div>
                    <div className="wrap empty-holder-box">
                        <div className="more-content-title">更多功能</div>
                        <div className="empty-holder"></div>
                    </div>
                    <div className="footer-holder"></div>
                </div>
                <div className="footer"></div>
            </Spin>
        );
    }
}

export default UserCenter;