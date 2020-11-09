import React, { Fragment } from 'react';
import { Form, Input, Button, Tooltip, Icon } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import ThirdPartyLoginView from 'components/ThirdPartyLoginView';
import SendSmsButton from 'components/SendSmsButton';
import SuccessNav from 'components/SuccessNav';
import ArrowRight from 'components/ArrowRight';
import validatePassWord from '../../utils/validate';
import encryptPassword from '../../utils/encrypt';
import './index.less';

@model('register')
class Register extends BaseView {
    constructor(props) {
        super(props);
        this.onSubmit = this.onSubmit.bind(this);
        this.onSendSms = this.onSendSms.bind(this);
    }
    onSubmit(e) {
        e.preventDefault();
        const { form: { validateFields }, register } = this.props;
        if (register.loading) {
            return;
        }
        validateFields((errors, { phone, password, code }) => {
            if (!errors) {
                this.dispatch({
                    type: 'register',
                    payload: {
                        phone,
                        password: encryptPassword(password),
                        code,
                    },
                });
            }
        });
    }
    onSendSms({ ticket, randstr }) {
        const { form: { validateFields } } = this.props;
        return new Promise((resolve) => {
            validateFields(['phone'], (errors, { phone }) => {
                if (!errors) {
                    this.dispatch({
                        type: 'sendSmsCode',
                        payload: {
                            phone,
                            ticket,
                            randstr,
                            type: 1,
                        },
                    })
                    .then(resolve)
                    .catch(resolve);
                } else {
                    resolve(false);
                }
            });
        });
    }
    onNavLogin() {
        window.location.href = './login.html';
    }
    renderForm() {
        const { register, form } = this.props;
        const { loading } = register;
        const { getFieldDecorator } = form;
        return (
            <Fragment>
                <div className="form-header">注册</div>
                <div className="f-l l-area">
                    <Form onSubmit={this.onSubmit}>
                        <Form.Item>
                            {getFieldDecorator('phone', {
                                rules: [{
                                    required: true,
                                    message: '请输入常用手机号',
                                }, {
                                    pattern: /^1\d{10}$/,
                                    message: '手机号码不合法',
                                }],
                            })(<Input placeholder="请输入常用手机号" />)}
                        </Form.Item>
                        <Form.Item>
                            {getFieldDecorator('code', {
                                rules: [{
                                    required: true,
                                    message: '请输入短信验证码',
                                }, {
                                    pattern: /^\d{6}$/,
                                    message: '验证码为6位数字',
                                }],
                            })(<Input placeholder="请输入短信验证码" className="inpt-code" />)}
                            <SendSmsButton
                                className="btn-send-code"
                                onSendSms={this.onSendSms}
                            >
                                获取验证码
                            </SendSmsButton>
                        </Form.Item>
                        <Form.Item>
                            {getFieldDecorator('password', {
                                rules: [{
                                    validator: validatePassWord,
                                }],
                            })(<Input placeholder="请输入密码" type="password" />)}
                            <Tooltip title="请输入6-20位密码，建议由数字、字母和符号组成">
                                <Icon
                                    type="question-circle"
                                    theme="filled"
                                    className="password-tips"
                                />
                            </Tooltip>
                        </Form.Item>
                        {/* <Form.Item className="register-rules-item">
                            {getFieldDecorator('isAgree')(
                                <Checkbox className="agree-rules">
                                    我已阅读并同意
                                </Checkbox>
                            )}
                            <a className="link-rules">
                                《福禄开放平台用户注册协议》
                            </a>
                        </Form.Item> */}
                        <Button
                            block
                            type="primary"
                            htmlType="submit"
                            style={{
                                marginTop: '25px',
                            }}
                            // disabled={!getFieldValue('isAgree')}
                            className="btn-form btn-register"
                            loading={loading}
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
                        <p>已有帐号:</p>
                        <a href="./login.html">
                            <span>立即登录</span>
                           <ArrowRight />
                        </a>
                    </div>
                    <ThirdPartyLoginView />
                </div>
            </Fragment>
        );
    }
    _render() {
        const { register } = this.props;
        const { registerSuccess } = register;
        return (
            <>
                <main className="main-box register">
                    <Header />
                    <div className="wrap-box">
                        <div className={`content-box ${ registerSuccess ? 'writing-mode-middle' : ''}`}>
                            {
                                registerSuccess ? <SuccessNav /> : this.renderForm()
                            }
                        </div>
                    </div>
                </main>
                <Footer />
            </>
        );
    }
}

export default Form.create()(Register);