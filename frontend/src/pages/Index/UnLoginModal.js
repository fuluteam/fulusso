import React from 'react';
import { Button, Modal, Icon } from 'antd';
import PropTypes from 'prop-types';

const UnLoginModal = ({ visible, handleOk }) => {
    return (
        <Modal
            visible={visible}
            title="消息提示"
            width={400}
            onCancel={handleOk}
            onOk={handleOk}
            centered
            footer={[
                <Button
                    key="submit"
                    type="primary"
                    onClick={() => {
                        const btnLogin = document.querySelector('.btn-login');
                        if (btnLogin) {
                            btnLogin.click();
                        }
                    }}
                >
                    登录
                </Button>]}
        >
            <div style={{ fontSize: '16px', textAlign: 'center', margin: '20px' }}>
                <Icon type="info-circle" style={{ color: 'orange', marginRight: '10px' }} />请先登录开放平台！
            </div>
        </Modal>
    );
};

UnLoginModal.propTypes = {
    visible: PropTypes.bool,
    handleOk: PropTypes.func,
};

UnLoginModal.defaultProps = {
    visible: false,
    handleOk: () => {},
};

export default UnLoginModal;
