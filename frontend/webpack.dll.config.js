const path = require('path');
const webpack = require('webpack');
const UglifyJsPlugin =  require('uglifyjs-webpack-plugin');

module.exports = {
    entry: {
        base: ['react', 'react-dom', 'mobx', 'mobx-react'],
    },
    output: {
        path: path.join(__dirname, 'src/assets/js'),
        filename: '[name].dll.js',
        library: '[name]_library'
    },
    plugins: [
        new webpack.DllPlugin({
            path: path.join(__dirname, 'src/assets/js', '[name]-manifest.json'),
            name: '[name]_library'
        }),
    ],
    optimization: {
        minimizer: [
            new UglifyJsPlugin({
                cache: true,
                parallel: true,
                uglifyOptions: {
                  output: {
                    beautify: false,
                    comments: false,
                  },
                  compress: {
                    drop_console: true,
                    collapse_vars: true,
                    reduce_vars: true,
                  }
                }
            }),
        ],
    }
};