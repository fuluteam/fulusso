const { join } = require('path');
const webpack = require('webpack');
const merge = require('webpack-merge');
const ip = require('ip');
const commonConfig = require('./webpack.config.common');

module.exports = merge.smart(commonConfig, {
	devServer: {
		host: ip.address(),
		port: 10010,
		hot: true,
		inline: true,
		index: 'index.html',
		compress: true,
		historyApiFallback: true,
		headers: {
			'Access-Control-Allow-Origin': '*',
		},
		open: true,
		openPage: 'index.html',
		publicPath: '/',
		contentBase: join(__dirname, 'dist'),
	},
	plugins: [
		new webpack.HotModuleReplacementPlugin(),
	],
	devtool: "inline-source-map",
});
