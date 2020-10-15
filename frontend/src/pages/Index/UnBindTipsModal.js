import React from 'react';
import PropTypes from 'prop-types';
import { Button, Modal, Icon } from 'antd';

const UnBindTipsModal = ({ visible, onCancel }) => {
    return (
        <Modal
            width={400}
            title="消息提示"
            visible={visible}
            onCancel={onCancel}
            centered
            footer={
                [
                    <Button key="submit" type="primary" onClick={onCancel}>
                        绑定
                    </Button>,
                ]
            }
        >
            <p style={{ textAlign: 'center', fontSize: '16px', margin: '20px' }}>
                <Icon
                    type="exclamation-circle"
                    style={{
                        position: 'relative',
                        top: '2px',
                        color: '#ff9200',
                        fontSize: '20px',
                        marginRight: '6px',
                    }}
                />
                请先绑定电子如意令！
            </p>
        </Modal>
    );
};

UnBindTipsModal.propTypes = {
    visible: PropTypes.bool,
    onCancel: PropTypes.func,
};

UnBindTipsModal.defaultProps = {
    visible: false,
    onCancel: () => {},
};

export default UnBindTipsModal;
