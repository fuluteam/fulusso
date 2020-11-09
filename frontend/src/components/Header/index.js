import React from 'react';
import './index.less';

const Header = ({ renderStatus, className }) => {
    return (
        <header className={className ? `hd ${className}` : "hd"}>
            <div className="wrap">
                <a href="./login.html" className="logo-link">
                    <div className="logo" />
                </a>
                {
                    typeof renderStatus == 'function' ? renderStatus() : <a href="./index.html" className="nav-to-center">用户中心</a>
                }
            </div>
        </header>
    );
};

export default Header;