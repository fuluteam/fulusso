import React from 'react';
import { Checkbox, Form, Tabs, Input, Button, message } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import SendSmsButton from 'components/SendSmsButton';
import ThirdPartyLoginView from 'components/ThirdPartyLoginView';
import ArrowRight from 'components/ArrowRight';
import { toNavApp } from '../../utils/url';
import CODE_OK from '../../utils/Constants';
import encrypt from '../../utils/encrypt';
import './index.less';

const { TabPane } = Tabs;

@model('login', 'register')
class Login extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            loginType: 'pass',
        };
        this.onSubmit = this.onSubmit.bind(this);
        this.onSendSms = this.onSendSms.bind(this);
        this.onTabChange = this.onTabChange.bind(this);
    }
    componentDidMount() {
        const phone = localStorage.getItem("phone");
        if (phone) {
            this.props.form.setFieldsValue({ phone });
            document.querySelector('#isRemember').click();
        }
        const access_token = localStorage.getItem("access_token");
        if (access_token) {
            localStorage.removeItem('access_token');
        }
    }

    onSubmit(e) {
        e.preventDefault();
        const { form: { validateFields } } = this.props;
        validateFields((errors, { isRemember, ...values }) => {
            if (!errors) {
                const { loginType } = this.state;
                const payload = {};
                if (loginType === 'pass') { // 密码登录
                    payload.username = values.phone;
                    payload.password = encrypt(values.password);
                } else {
                    payload.phone = values.phone;
                    payload.code = values.code;
                }
                if (isRemember) { // 记住账号
                    localStorage.setItem('phone', values.phone);
                } else {
                    localStorage.removeItem('phone');
                }
                if (loginType === 'pass') {
                    new TencentCaptcha('2079932958', (res) => {
                        if (res.ret === 0) {
                            const { ticket, randstr } = res;
                            payload.Ticket = ticket;
                            payload.RandStr = randstr;
                            this.onLogin(payload, loginType);
                        }
                    }).show();
                } else {
                    this.onLogin(payload, loginType);
                }
            }
        });
    }

    onLogin(payload, loginType) {
        this.dispatch({
            type: loginType === 'pass' ? 'login' : 'smsLogin',
            payload,
        }).then(({ code, message: msg }) => {
            if (code == CODE_OK) {
                toNavApp();
            } else {
                message.error(msg);
            }
        });
    }

    onTabChange(type) {
        this.setState({
            loginType: type,
        });
        this.props.form.resetFields();
    }
    onSendSms({ ticket, randstr }) {
        const { form: { validateFields } } = this.props;
        return new Promise((resolve) => {
            validateFields(['phone'], (errors, { phone }) => {
                if (!errors) {
                    this.dispatch({
                        type: 'register/sendSmsCode',
                        payload: {
                            phone,
                            ticket,
                            randstr,
                            type: 2,
                        },
                    })
                    .then(resolve)
                    .catch(resolve);
                } else {
                    resolve(false);
                }
            });
        })
    }
    _render() {
        const { loginType } = this.state;
        const { form, login } = this.props;
        const { loading } = login;
        const { getFieldDecorator } = form;
        return (
            <>
                <main className="main-box login">
                    <Header />
                    <div className="wrap-box">
                        <div className="content-box">
                            <div className="f-l l-area">
                                <Tabs onChange={this.onTabChange}>
                                    <TabPane tab="账户登录" key="pass">
                                    </TabPane>
                                    <TabPane tab="短信登录" key="sms">
                                    </TabPane>
                                </Tabs>
                                <Form onSubmit={this.onSubmit}>
                                    <Form.Item>
                                        {getFieldDecorator('phone', {
                                            rules: [{
                                                required: true,
                                                message: '请输入常用手机号',
                                            }, {
                                                pattern: loginType === 'pass' ? /^[\d\w]{4,11}$/ : /^1\d{10}$/,
                                                message: '手机号码不合法',
                                            }],
                                        })(<Input placeholder="请输入常用手机号" />)}
                                    </Form.Item>
                                    {
                                        loginType === 'pass' ? 
                                        <Form.Item>
                                            {getFieldDecorator('password', {
                                                rules: [{
                                                    required: true,
                                                    message: '请输入密码',
                                                }],
                                            })(<Input placeholder="请输入密码" type="password" />)}
                                        </Form.Item> :
                                        <Form.Item>
                                            {getFieldDecorator('code', {
                                                rules: [{
                                                    required: true,
                                                    message: '请输入短信验证码',
                                                }, {
                                                    pattern: /^\d{6}$/,
                                                    message: '验证码为6位数字',
                                                }],
                                            })(<Input placeholder="请输入短信验证码" className="inpt-code" maxLength="6" />)}
                                            <SendSmsButton
                                                className="btn-send-code"
                                                onSendSms={this.onSendSms}
                                            >
                                                获取验证码
                                            </SendSmsButton>
                                        </Form.Item>
                                    }
                                    <Form.Item className="form-help-item">
                                        {getFieldDecorator('isRemember')(<Checkbox>记住账号</Checkbox>)}
                                        <a href="./resetPassword.html" className="f-r">忘记密码?</a>
                                    </Form.Item>
                                    <Button
                                        type="primary"
                                        htmlType="submit"
                                        className="btn-login"
                                        loading={loading}
                                    >
                                        登录
                                    </Button>
                                    <Button
                                        className="btn-register"
                                        href="./register.html"
                                    >
                                       立即注册
                                    </Button>
                                </Form>
                            </div>
                            <div className="f-l or-area">
                                <span className="or">or</span>
                            </div>
                            <div className="f-l other-area">
                                <div className="help-link">
                                    <p>还没有账号:</p>
                                    <a href="./register.html">
                                        <span>立即注册</span>
                                        <ArrowRight />
                                    </a>
                                </div>
                                <ThirdPartyLoginView />
                            </div>
                        </div>
                    </div>
                </main>
                <Footer />
            </>
        );
    }
}

export default Form.create()(Login);