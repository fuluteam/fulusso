import React from 'react';
import { Form, Input, Button, message } from 'antd';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import SendSmsButton from 'components/SendSmsButton';
import SuccessTips from './SuccessTips';
import UserRulesModal from '../Register/UserRulesModal';
import CODE_OK from '../../utils/Constants';
import encrypt from '../../utils/encrypt';
import './index.less';

const MODE_REGISTER = 'register';
const MODE_NORMAL = 'normal';

@model('bindAccount')
class BindAccount extends BaseView {
    constructor(props) {
        super(props);
        this.state = {
            showRulesModal: false,
            showSuccessTips: false,
            bindType: MODE_REGISTER,
        };
        this.onSubmit = this.onSubmit.bind(this);
        this.onSendSms = this.onSendSms.bind(this);
        this.checkPhone = this.checkPhone.bind(this);
        this.onBindChange = this.onBindChange.bind(this);
        this.onToggleBindType = this.onToggleBindType.bind(this);
        this.onToggleRulesModal = this.onToggleRulesModal.bind(this);
    }
    onToggleRulesModal() {
        this.setState({
            showRulesModal: !this.state.showRulesModal,
        });
    }
    onToggleBindType() {
        this.setState({
            bindType: this.state.bindType === MODE_NORMAL ? MODE_REGISTER : MODE_NORMAL,
        });
    }
    onSubmit(e) {
        e.preventDefault();
        const { form: { validateFields } } = this.props;
        validateFields((errors, { isRemember, ...values }) => {
            if (!errors) {
                const { bindType } = this.state;
                const payload = {};
                if (bindType === MODE_REGISTER) { // 短信绑定
                    payload.Phone = values.phone;
                    payload.Code = values.code;
                } else {
                    payload.username = values.phone2;
                    payload.password = encrypt(values.password);
                }
                this.onLogin(payload, bindType);
            }
        });
    }

    onLogin(payload, bindType) {
        this.dispatch({
            type: bindType === MODE_REGISTER ? 'smsBind' : 'accountBind',
            payload,
        }).then(({ code, message: msg }) => {
            if (code == CODE_OK) {
                this.setState({
                    showSuccessTips: true,
                });
            } else {
                message.error(msg);
            }
        });
    }

    onBindChange(type) {
        this.setState({
            bindType: type,
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
    getContentCls(contentType = MODE_REGISTER) {
        const { bindType } = this.state;
        if (bindType === contentType) {
            return 'bind-content active';
        }
        return 'bind-content';
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
    renderContent() {
        const { bindType } = this.state;
        const { form, bindAccount } = this.props;
        const { loading } = bindAccount;
        const { getFieldDecorator } = form;
        return (
            <>
                <div className="bind-account-title">请关联你的葫芦藤通行证账号</div>
                <div className="bind-account-box">
                    <div className={this.getContentCls()}>
                        <div className="bind-select-header" onClick={this.onToggleBindType}>
                            <i className="check-icon"></i>
                            <div className="tips">
                                <p>绑定葫芦藤通行证账号，若未注册，则自动注册并绑定</p>
                                <p className="sub">绑定后，使用或者通行证账号均可登录到你现有的账号。</p>
                            </div>
                        </div>
                        {
                            bindType === MODE_REGISTER ?
                                <div className="bind-main">
                                    <Form onSubmit={this.onSubmit}>
                                        <Form.Item>
                                            {getFieldDecorator('phone', {
                                                rules: [{
                                                    required: true,
                                                    message: '请输入手机号',
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
                                                beforeSend={this.checkPhone}
                                            >
                                                获取验证码
                                            </SendSmsButton>
                                        </Form.Item>
                                        <Form.Item>
                                            <Button
                                                type="primary"
                                                htmlType="submit"
                                                loading={loading}
                                            >
                                                创建并绑定
                                            </Button>
                                        </Form.Item>
                                        {/* <Form.Item>
                                            <div style={{ marginTop: '-15px', color: '#000' }}>
                                                注册代表你已同意
                                                <a
                                                    onClick={this.onToggleRulesModal}
                                                    className="link-user-rules"
                                                >
                                                    《福禄开放平台用户注册协议》
                                                </a>
                                            </div>
                                        </Form.Item> */}
                                    </Form>
                                </div>
                            : null    
                        }
                    </div>
                    <div className={this.getContentCls(MODE_NORMAL)}>
                        <div className="bind-select-header" onClick={this.onToggleBindType}>
                            <i className="check-icon"></i>
                            <div className="tips">
                                <p>你还可以使用已注册的通行证账号进行绑定</p>
                            </div>
                        </div>
                        {
                            bindType === MODE_NORMAL ?
                                <div className="bind-main">
                                    <Form onSubmit={this.onSubmit}>
                                        <Form.Item>
                                            {getFieldDecorator('phone2', {
                                                rules: [{
                                                    required: true,
                                                    message: '请输入手机号',
                                                }, {
                                                    pattern: /^1\d{10}$/,
                                                    message: '手机号码不合法',
                                                }],
                                            })(<Input placeholder="请输入常用手机号" />)}
                                        </Form.Item>
                                        <Form.Item>
                                            {getFieldDecorator('password', {
                                                rules: [{
                                                    required: true,
                                                    message: '请输入密码',
                                                }],
                                            })(<Input placeholder="请输入密码" type="password" />)}
                                        </Form.Item>
                                        <Form.Item>
                                            <Button
                                                type="primary"
                                                htmlType="submit"
                                                loading={loading}
                                            >
                                                绑定
                                            </Button>
                                        </Form.Item>
                                    </Form>
                                </div>
                            : null
                        }
                    </div>
                </div>
            </>
        );
    }
    _render() {
        const { showSuccessTips, showRulesModal } = this.state;
        return (
            <>
                <main className="main-box bind-account">
                    <Header />
                    <div className="wrap-box">
                        <div className={showSuccessTips ? 'content-box bind-success-box' : 'content-box'}>
                            {
                                showSuccessTips ? <SuccessTips /> : this.renderContent()
                            }
                        </div>
                    </div>
                </main>
                <Footer />
                <UserRulesModal
                    visible={showRulesModal}
                    onClose={this.onToggleRulesModal}
                />
            </>
        );
    }
}

export default Form.create()(BindAccount);