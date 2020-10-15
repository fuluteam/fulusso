import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { observer } from 'mobx-react';
import Store, { Register } from './Store';

@observer
class DynamicComponent extends Component {
    constructor(props) {
        super(props);
        this.state = {
            namespace: '',
            effects: {},
            loadFinish: false,
            dependencies: [],
            initialState: {},
        };
        this.load();
    }

    load() {
        const { resolve } = this.props;
        resolve.then(([m, ...dependencies]) => {
            const { state, namespace, effects } = m.default;
            const stateObj = {
                namespace,
                loadFinish: true,
                initialState: state,
                effects,
            };
            if (dependencies.length > 0) {
                stateObj.dependencies = dependencies;
            }
            if (dependencies.length > 0) {
                dependencies.forEach((dep) => {
                    const { state: st, namespace: ns, effects: efs } = dep.default;
                    this[ns] = Register(ns, st, efs);
                });
            }
            this[namespace] = Register(namespace, state, effects, dependencies);
            this.setState(stateObj);
        });
    }

    render() {
        const { loadFinish, initialState, effects, namespace, dependencies } = this.state;
        const { Target, resolve, ...resetProps } = this.props;
        if (loadFinish) {
            const store = Store.getInstance();
            const _props = {
                [namespace]: store[namespace] || {},
            };
            dependencies.forEach((dep) => {
                const { namespace: ns } = dep.default;
                _props[ns] = store[ns] || {};
            });
            return (
                <Target
                    {..._props}
                    namespace={namespace}
                    initialState={initialState}
                    effects={effects}
                    dependencies={dependencies}
                    {...resetProps}
                />
            );
        }
        return null;
    }
}

DynamicComponent.propTypes = {
    resolve: PropTypes.object,
    Target: PropTypes.element,
};

DynamicComponent.defaultProps = {
    resolve: {},
    Target: () => {},
};

const dynamic = (target, resolve, props) => {
    return (
        <DynamicComponent
            Target={target}
            resolve={resolve}
            {...props}
        />
    );
};

export default dynamic;
