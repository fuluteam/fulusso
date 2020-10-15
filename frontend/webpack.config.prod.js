const { join } = require('path');
const merge = require('webpack-merge');
const CleanPlugin = require('clean-webpack-plugin');
const AssetsPlugin = require('assets-webpack-plugin');
const commonConfig = require('./webpack.config.common');

module.exports = merge.smart(commonConfig, {
	plugins: [
		new CleanPlugin({
			cleanOnceBeforeBuildPatterns: join(__dirname, 'dist'),
		}),
		new AssetsPlugin({
			path: join(__dirname, 'build'),
			includeManifest: join(__dirname, 'build/manifest'),
		}),
	],
});
