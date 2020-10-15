/**
 * @desc 设置默认交易验证方式弹窗组件
 * @author zhangkegui@fulu.com
 * @date 2019-11-14
 * @version 1.0
 */
import React from 'react';
import { Button, Modal, Form, Radio, message } from 'antd';
import PropTypes from 'prop-types';
import CODE_OK from '../../utils/Constants';

const ValidatorModal = ({ visible, onCancel, form, dispatch, user, loading }) => {
    const onCloseModal = (isSuccess = false) => {
        if (loading) {
            return;
        }
        onCancel();
        if (!isSuccess) { // 没有成功设置交易方式，则重置选项
            form.setFieldsValue({
                validateModePriority: user.validateModePriority,
            });
        }
    };
    const onSubmit = () => {
        form.validateFields((errors, values) => {
            if (!errors) {
                dispatch({
                    type: 'changeValidateMode',
                    payload: values,
                }).then(({ code }) => {
                    if (code === CODE_OK) {
                        message.success('设置成功');
                        onCloseModal(true);
                    }
                });
            }
        });
    };
    return (
        <Modal
            visible={visible}
            title="默认验证方式"
            width={450}
            onCancel={() => {
                onCloseModal();
            }}
            wrapClassName="validator-modal"
            centered
            footer={[
                <Button
                    key="submit"
                    type="primary"
                    loading={loading}
                    onClick={onSubmit}
                >
                    确定
                </Button>,
                <Button
                    disabled={loading}
                    onClick={() => {
                        onCloseModal();
                    }}
                >
                    取消
                </Button>,
            ]}
        >
            <div>
                <Form onSubmit={onSubmit}>
                    <Form.Item style={{ margin: '20px 0 10px 15px' }}>
                        {form.getFieldDecorator('validateModePriority', {
                            rules: [{
                                required: true,
                                message: '请选择交易验证方式',
                            }],
                            initialValue: user && user.validateModePriority,
                        })(<Radio.Group>
                            <Radio value={1}>电子如意令（绑定如意令后，默认使用如意令）</Radio>
                            <Radio value={0}>短信验证</Radio>
                        </Radio.Group>)}
                    </Form.Item>
                </Form>
            </div>
        </Modal>
    );
};

ValidatorModal.propTypes = {
    visible: PropTypes.bool,
    onCancel: PropTypes.func,
    form: PropTypes.object,
    user: PropTypes.object,
    dispatch: PropTypes.func,
    loading: PropTypes.bool,
};

ValidatorModal.defaultProps = {
    visible: false,
    dispatch: () => {},
    onCancel: () => {},
    form: {},
    user: {},
    loading: false,
};

export default Form.create()(ValidatorModal);
