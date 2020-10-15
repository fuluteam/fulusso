/**
 * @desc 倒计时计数器组件，当前版本只支持秒倒计时，且终止秒数(endCount)需小于起始秒数(startCount)
 * @author zhangkegui@fulu.com
 * @date 2019-11-06
 * @version 1.0
 */
import React from 'react';

class Counter extends React.PureComponent {
    constructor(props) {
        super(props);
        this.state = {
           count: props.startCount || 0,
        };
        this.timer = null;
        this.onCount = this.onCount.bind(this);
    }
    componentDidMount() {
        this.onCount();
    }
    componentWillUnmount() {
        this.timer && clearTimeout(this.timer);
    }
    onCount() {
        const { count } = this.state;
        const { startCount, endCount = 0, onCountEnd } =this.props;
        if (count > endCount && endCount <= startCount) {
            this.timer = setTimeout(() => {
                this.setState({
                    count: count - 1
                }, this.onCount);
            }, 1000);
        } else if (typeof onCountEnd == 'function') { // 计数完毕，执行回调函数
            onCountEnd();
        }
    }
    render() {
        const { suffix = 's' } = this.props;
        const { count } = this.state;
        return (
            <span className="counter">{`${count}${suffix}`}</span>
        );
    }
}

export default Counter;