import { Component } from 'react';
import { observer } from 'mobx-react';
import PropTypes from 'prop-types';
import { Register } from './Store';

@observer
class BaseView extends Component {
    constructor(props) {
        super(props);
        const { namespace, initialState, effects, dependencies = [] } = props;
        if (dependencies.length > 0) {
            dependencies.forEach((dep) => {
                const { state, namespace: ns, effects: efs } = dep.default;
                this[ns] = Register(ns, state, efs);
            });
        }
        this.namespace = namespace;
        this[namespace] = Register(namespace, initialState, effects, dependencies);
        this.dispatch = ({ type, payload }) => {
            // 调用方是否是依赖的model中的方法
            let isDependencies = typeof effects[type] !== 'function';
            let realType = type;
            let realNs = namespace;
            if (type.indexOf('/') > 0) {
                const [n, t] = type.split('/');
                realType = t;
                realNs = n;
                if (n === namespace) {
                    isDependencies = false;
                }
            }
            if (!isDependencies) {
                return effects[realType](payload);
            }
            const dependency = dependencies.find((dep) => {
                const { namespace: ns } = dep.default;
                return ns === realNs;
            });
            if (dependency) {
                const { effects: efs } = dependency.default;
                return efs[realType](payload);
            }
        };
    }

    _render() {
        return null;
    }

    render() {
        return this._render();
    }
}

BaseView.propTypes = {
    namespace: PropTypes.string,
    initialState: PropTypes.object,
    effects: PropTypes.object,
    dependencies: PropTypes.array,
};

BaseView.defaultProps = {
    namespace: '',
    initialState: {},
    effects: {},
    dependencies: [],
};

export default BaseView;
