const { join, resolve } = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const AntdIconReducePlugin = require('antd-icon-reduce-plugin');
const manifestBaseDll = require('./src/assets/js/base-manifest.json');
const { createHtmlPages, createEntry } = require('./page.config');
const packageJson = require('./package.json');
const buildVersion = packageJson.version;

const DEBUG = !process.argv.find((a) => {
  	return a === '--mode=production';
});

module.exports = {
	entry: createEntry(),
	output: {
		path: join(__dirname, 'dist'),
		publicPath: '/',
		filename: 'resources/js/[name]-[hash:10]-' + buildVersion + '.js',
		chunkFilename: 'resources/js/[name]-[contenthash:10]-' + buildVersion + '.js',
	},
	module: {
		rules: [{
			test: /\.jsx?$/,
			use: ["antd-icon-reduce-loader", {
				loader: 'babel-loader',
				options: {
					extends: join(__dirname, '.babelrc'),
					cacheDirectory: true,
				},
			}],
			exclude: /node_modules/,
		}, {
			test: /\.css$/,
			use: [ ...(DEBUG ? ['style-loader'] : [MiniCssExtractPlugin.loader]), 'css-loader', 'postcss-loader'],
		}, {
			test: /\.less$/,
			use: [...(DEBUG ? ['style-loader'] : [MiniCssExtractPlugin.loader]), 'css-loader', 'postcss-loader', {
				loader: 'less-loader',
				options: {
					javascriptEnabled: true,
				},
			}],
		}, {
			test: /\.(png|jpe?g|gif)$/,
			use: [{
				loader: 'url-loader',
				options: {
				name: '[name].[ext]',
				limit: 10000,
				outputPath: 'resources/images/',
				},
			}],
		}, {
			test: /\.svg(\?v=\d+\.\d+\.\d+)?$/,
			use: [{
				loader: 'babel-loader',
			}, {
				loader: '@svgr/webpack',
			}],
		}]
	},
	plugins: [
		new AntdIconReducePlugin({
			iconFilePath: resolve(__dirname, 'ant-icon.js'),
		}),
		new webpack.HashedModuleIdsPlugin(),
		new webpack.DllReferencePlugin({
			context: __dirname,
			manifest: manifestBaseDll,
		}),
			...createHtmlPages(),
			...(DEBUG ? [] : [
			new MiniCssExtractPlugin({
				filename: 'resources/css/[name]-[contenthash:10]-' + buildVersion + '.css',
				chunkFilename: 'resources/css/[name]-[contenthash:10]' + buildVersion + '.css',
			}),
		]),
		new CopyPlugin([{
			context: __dirname,
			from: join(__dirname, 'src/configs/configs.js'),
			to: join(__dirname, 'dist/resources/js'),
		}, {
			from: 'src/assets/js/*.js',
			to: join(__dirname, 'dist/resources/js'),
			flatten: true,
		}, {
			from: 'src/assets/*.html',
			to: join(__dirname, 'dist'),
			flatten: true,
		}])
	],
	resolve: {
		alias: {
			core: '../../core',
			assets: '../../assets',
			components: '../../components',
		},
		extensions: ['.js', '.jsx', '.json'],
		modules: [
			join(__dirname, 'build'),
			join(__dirname, 'node_modules'),
		],
	},
};
