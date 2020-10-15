import dynamic from './dynamic';

const model = (...modelArr) => {
    return (target) => {
        return (props) => {
            if (modelArr.length === 0) {
                return null;
            }
            const resolve = Promise.all(modelArr.reduce((arr, modelItem) => {
                arr.push(import(`../models/${modelItem}`));
                return arr;
            }, []));
            return (
                dynamic(target, resolve, props)
            );
        };
    };
};

export default model;
