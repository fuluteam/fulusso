import React from 'react';
import { Button, message } from 'antd';
import CODE_OK from '../../utils/Constants';

class SendSmsButton extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            seconds: -1,
            loading: false,
        };
        this.startLoop = this.startLoop.bind(this);
        this.onSend = this.onSend.bind(this);
        this.secondCountDown = this.secondCountDown.bind(this);
        this.timer = null;
    }
    static getDerivedStateFromProps(nextProps) {
        const { visible = true, reset = false } = nextProps;
        if (!visible || reset) {
            return {
                seconds: -1,
                loading: false,
            };
        }
        return null;
    } 
    onSend() {
        const { captcha = true } = this.props;
        const { loading } = this.state;
        if (loading) {
            return;
        }
        if (captcha) { // 启用图片验证
            const captchaObj = new TencentCaptcha('2079932958', (res) => {
                if (res.ret === 0) {
                   this.onTriggerSend(res);
                }
            });
            captchaObj.show();
        } else {
            this.onTriggerSend();
        }
    }
    componentWillUnmount() {
        if (this.timer) {
            clearTimeout(this.timer);
        }
    }
    onTriggerSend(...args) {
        const { onSendSms, showError = true } = this.props;
        if (typeof onSendSms === 'function') {
            this.setState({
                loading: true,
            });
            onSendSms(...args).then((result) => {
                this.setState({
                    loading: false,
                });
                if (typeof result === 'object') {
                    if (result.code === CODE_OK) {
                        message.success('发送成功');
                        this.startLoop();
                    } else if (showError) {
                        message.error(result.message);
                    }
                }
            }).catch(() => {
                this.setState({
                    loading: false,
                });
            });
        }
    }
    startLoop() {
        this.setState({
            seconds: 60,
        }, () => {
            this.secondCountDown();
        });
    }
    secondCountDown() {
        const { seconds } = this.state;
        if (seconds < 0) {
            return;
        }
        this.timer = setTimeout(() => {
            this.setState({
                seconds: seconds -1,
            }, this.secondCountDown);
        }, 1000);
    }
    render() {
        const { children = '获取短信验证码', ...restProps } = this.props;
        const { seconds, loading } = this.state;
        const disabled = seconds >= 0;
        return (
            <Button
                onClick={this.onSend}
                disabled={disabled}
                loading={loading}
                className="btn-send-sms"
                {...restProps}
            >
                {disabled ? `${seconds}s` : children}
            </Button>
        );
    }
}

export default SendSmsButton;