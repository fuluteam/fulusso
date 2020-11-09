import React from 'react';
import PropTypes from 'prop-types';
import { getQueryString } from '../../utils/url';

class SuccessTips extends React.PureComponent {
    constructor(props) {
        super(props);
        this.state = {
            time: 3,
        };
        this.timer = null;
        this.timeLoop = this.timeLoop.bind(this);
    }

    componentDidMount() {
        this.timeLoop();
    }

    componentWillUnmount() {
        if (this.timer) {
            clearTimeout(this.timer);
        }
    }

    timeLoop() {
        const { time } = this.state;
        if (time >= 1) {
            this.timer = setTimeout(() => {
                this.setState({
                    time: time - 1,
                });
                this.timeLoop();
            }, 1000);
        } else {
            this.redirectPage();
        }
    }

    redirectPage() {
        const returnUrl = getQueryString('ReturnUrl');
        if (returnUrl) {
            window.location.href = returnUrl;
        } else {
            window.location.href = './index.html';
        }
    }

    render() {
        const { time } = this.state;
        const { msg } = this.props;
        return (
            <div className="bind-success">
                <div className="bind-success-img" />
                <div className="bind-success-word">
                    <p>{msg}</p>
                </div>
                <div className="redirect-to-login">
                    <p>
                        页面将在&nbsp;
                        <span>{time}</span> 秒后跳转&nbsp;
                        <a
                            href="javascript:void(0);"
                            onClick={this.redirectPage}
                        >点我直接跳转</a>
                    </p>
                </div>
            </div>
        );
    }
}

SuccessTips.propTypes = {
    msg: PropTypes.string,
};

SuccessTips.defaultProps = {
    msg: '您已绑定成功了哦',
};

export default SuccessTips;
