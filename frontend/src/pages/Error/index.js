import React from 'react';
import BaseView from 'core/BaseView';
import model from 'core/Decorator';
import Header from 'components/Header';
import Footer from 'components/Footer';
import { getQueryString } from '../../utils/url';
import './index.less';

@model('error')
class Error extends BaseView {
    constructor(props) {
        super(props);
        this.errorId = getQueryString('errorId');
    }
    componentWillMount() {
        this.dispatch({
            type: 'fetchError',
            payload: {
                errorId: this.errorId,
            }
        });
    }
    _render() {
        const { error } = this.props;
        const { msg } = error;
        return (
            <>
                <main className="main-box error">
                    <Header />
                    <div className="wrap-box">
                        <div className="content-box">
                            <div className="error-icon"></div>
                            <h3>参数错误</h3>
                            <p>{msg}</p>
                        </div>
                    </div>
                </main>
                <Footer />
            </>
        );
    }
}

export default Error;