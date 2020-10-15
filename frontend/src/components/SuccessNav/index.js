import React from 'react';
import Counter from '../Counter';
import './index.less';

const SuccessNav = ({ visiable = true, title = '注册' }) => {
    if (visiable) {
        return (
            <div className="register-success">
                <div className="icon" />
                <div className="tps">
                    您已{title}成功了哦~
                    <p>
                        页面将在 
                        <Counter
                            suffix="秒"
                            startCount={3}
                            onCountEnd={() => {
                                window.location.href = './login.html';
                            }}
                        />
                        后跳转至<a href="./login.html">登录界面</a></p>
                </div>
            </div>
        );
    }
    return null;
};

export default SuccessNav;
