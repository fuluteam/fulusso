import React from 'react';
import './index.less';

const OperationCard = ({ title, tips, unLoginTitle, unLoginTips, type, user, renderBtn }) => {
    const showTitle = user ? title : unLoginTitle || title;
    const showTips = user ? tips : unLoginTips || tips;
    return (
        <div className={`operation-card ${type}`}>
            <div class="icon-box">
                <div class="icon-inner-wrapper">
                    <i class="icon"></i>
                </div>
            </div>
            <div class="content-box">
                <p class="card-title">
                    {showTitle}
                    { type === 'e-app' && user ? <span className="bind-status">{ user.binding_status === 0 ? '未绑定' : '已绑定' }</span> : null }
                </p>
                <p class="card-tips">{showTips}</p>
            </div>
            <div class="btn-box">
                {
                    renderBtn()
                }
            </div>
        </div>
    );
};

export default OperationCard;
