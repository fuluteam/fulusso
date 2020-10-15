import React from 'react';
import { Button, Spin } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import OperationCard from 'components/OperationCard';
import ChapterHeader from 'components/ChapterHeader';
import { getQueryString } from '../../utils/url';
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
        if (!isFetchTokenSuccess) {
            this.setState(newState);
            return;
        }
        let user = null;
        try {
            user = await this.fetchUser();
        } catch (err) {
            if (this.initCount === 0) {
                localStorage.removeItem('access_token'); // 防止token过期，重新请求
                this.loadUserData();
                this.initCount = 1;
            }
        }
        if (user && user.code == CODE_OK) { // 登录模式
            newState.visitorMode = false;
        }
        this.setState(newState);
    }
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
                    <span class="name">{user.cellphone}</span>
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
                    登录 / 注册
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
 
    _render() {
        const { initFinish } = this.state;
        const { userCenter } = this.props;
        const { user } = userCenter;
        return (
            <Spin spinning={!initFinish}>
                <div className="main-wrapper">
                    <Header renderStatus={this.renderStatus} className="t-header" />
                    <div className="banner">
                        <div className="b-l" />
                        <div className="b-c" />
                        <div className="b-r" />
                    </div>
                    <div className="main-box">
                    <ChapterHeader title="账号保护" />
                        <OperationCard
                            title={`账号：${user && user.cellphone}`}
                            unLoginTitle="密码管理"
                            unLoginTips="修改用户登录密码及找回密码"
                            type="account"
                            user={user}
                            renderBtn={() => {
                                return (
                                    <>
                                        {
                                            !user ? <Button
                                                type="primary"
                                                onClick={() => {
                                                    window.location.href = `${window.location.origin}/resetPassword.html`; 
                                                }}
                                            >
                                                找回密码
                                            </Button> : null
                                        }
                                    </>
                                );
                            }}
                        />
                    </div>
                    <div className="footer-holder"></div>
                </div>
                <Footer />
            </Spin>
        );
    }
}

export default UserCenter;