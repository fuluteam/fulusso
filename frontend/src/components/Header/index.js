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
                    typeof renderStatus == 'function' && renderStatus()
                }
            </div>
        </header>
    );
};

export default Header;