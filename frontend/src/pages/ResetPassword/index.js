import React from 'react';
import { Form, Input, Icon, Button, message } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import SendSmsButton from 'components/SendSmsButton';
import SuccessNav from 'components/SuccessNav';
import validatePassWord from '../../utils/validate';
import encryptPassword from '../../utils/encrypt';
import CODE_OK from '../../utils/Constants';
import './index.less';

@model('resetPassword', 'register')
class ResetPassword extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            showNext: false,
            showSuccess: false,
            showPassWord: false,
            validCode: '',
        };
        this.onCancel = this.onCancel.bind(this);
        this.onSubmit = this.onSubmit.bind(this);
        this.onSendSms = this.onSendSms.bind(this);
        this.checkPhone = this.checkPhone.bind(this);
        this.onTogglePwdType = this.onTogglePwdType.bind(this);
        this.onCheckCodeValid = this.onCheckCodeValid.bind(this);
    }
    onSubmit(e) {
        e.preventDefault();
        const { form } = this.props;
        form.validateFields((errors, { password }) => {
            if (!errors) {
                const { validCode } = this.state;
                this.dispatch({
                    type: 'resetPassword',
                    payload: {
                        ticket: validCode,
                        password: encryptPassword(password),
                    }
                }).then((result) => {
                    if (result) {
                        if (result.code == CODE_OK) {
                            this.setState({
                                showSuccess: true,
                            });
                        } else {
                            message.error(result.message);
                        }
                    }
                });
            }
        });
    }
    onCancel() {
        this.setState({
            showNext: false,
        });
        const { form } = this.props;
        form.resetFields(['code']);
        form.setFieldsValue({
            password: this.state.ps,
            validCode: '',
        });
    }
    getSubstrPhone() {
        const { form } = this.props;
        const phone = form.getFieldValue('phone');
        if (phone) {
            return `:${phone.slice(0,3)}****${phone.slice(8)}`;
        }
    }
    onCheckCodeValid() {
        const { form, loading } = this.props;
        if (loading) {
            return;
        }
        form.validateFields(['phone', 'code'], (errors, { phone, code }) => {
            if (!errors) {
                this.dispatch({
                    type: 'checkCodeValid',
                    payload: {
                        phone,
                        code,
                    }
                }).then((result) => {
                    if (result) {
                        if (result.code == CODE_OK) {
                            this.setState({
                                showNext: true,
                                validCode: result.data,
                            });
                        } else {
                            message.error(result.message);
                        }
                    }
                });
            }
        });
    }
    onSendSms({ ticket, randstr }) {
        const { form: { getFieldValue } } = this.props;
        return new Promise((resolve) => {
            this.dispatch({
                type: 'register/sendSmsCode',
                payload: {
                    phone: getFieldValue('phone'),
                    ticket,
                    randstr,
                    type: 3,
                },
            })
            .then(resolve)
            .catch(resolve);
        });
    }
    checkPhone() {
        const { form: { validateFields } } = this.props;
        return new Promise((resolve) => {
            validateFields(['phone'], (err) => {
                if (err) {
                    return resolve();
                }
                return resolve(true);
            });
        });
        
    }
    onTogglePwdType() {
        this.setState({
            showPassWord: !this.state.showPassWord,
        });
    }
    renderNextContent() {
        const { showPassWord } = this.state;
        const { form } = this.props;
        const { getFieldDecorator } = form;
        return (
            <>
                <Form.Item label="手机号" colon={false} className="show-phone-item">
                    <span style={{ fontSize: '16px' }}>{this.getSubstrPhone()}</span>
                </Form.Item>
                <Form.Item>
                    {getFieldDecorator('password', {
                        rules: [{
                            validator: validatePassWord,
                        }],
                    })(<Input placeholder="请输入新密码" type={showPassWord ? 'input' : 'password'} />)}
                    <Icon
                        className="pwd-icon"
                        type={showPassWord ? 'eye' : 'eye-invisible'}
                        theme="filled"
                        onClick={this.onTogglePwdType}
                    />
                </Form.Item>
            </>
        );
    }
    _render() {
        const { showNext, showSuccess } = this.state;
        const { form, resetPassword } = this.props;
        const { loading } = resetPassword;
        const { getFieldDecorator } = form;
        const clsArr = ['content-box'];
        if (showSuccess) {
            clsArr.push('writing-mode-middle');
        }
        if (showNext) {
            clsArr.push('validate-mode');
        }
        return (
            <>
                <main className="main-box reset-password">
                    <Header />
                    <div className="wrap-box">
                        <div className={clsArr.join(' ')}>
                            {
                                showSuccess ?
                                    <SuccessNav title="重置" />
                                    : 
                                    <>
                                        <div className="form-header">找回密码</div>
                                        <div className="clip-hidden">
                                            <input type="text" />
                                            <input type="password" />
                                        </div>
                                        <Form onSubmit={this.onSubmit}>
                                            <div className={showNext ? 'clip-hidden' : ''}>
                                                <Form.Item>
                                                    {getFieldDecorator('phone', {
                                                        rules: [{
                                                            required: true,
                                                            message: '请输入手机号',
                                                        }, {
                                                            pattern: /^1\d{10}$/,
                                                            message: '手机号码不合法',
                                                        }],
                                                    })(<Input placeholder="请输入手机号" />)}
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
                                                    })(<Input placeholder="请输入短信验证码" className="inpt-code" maxLength="6" />)}
                                                    <SendSmsButton
                                                        className="btn-send-code"
                                                        onSendSms={this.onSendSms}
                                                        beforeSend={this.checkPhone}
                                                        reset={showNext}
                                                    >
                                                        获取验证码
                                                    </SendSmsButton>
                                                </Form.Item>
                                                <Button
                                                    type="primary"
                                                    className="btn-next"
                                                    onClick={this.onCheckCodeValid}
                                                    loading={loading}
                                                >
                                                    找回密码
                                                </Button>
                                            </div>
                                            <div className={showNext ? '' : 'clip-hidden'}>
                                                {this.renderNextContent()}
                                                <div className="footer-btns">
                                                    <Button type="primary" htmlType="submit" loading={loading}>确定</Button>
                                                    <Button onClick={this.onCancel} disabled={loading}>取消</Button>
                                                </div>
                                            </div>        
                                        </Form>
                                    </>
                            }
                        </div>
                    </div>
                </main>
                <Footer /> 
            </>
        );
    }
}

export default Form.create()(ResetPassword);