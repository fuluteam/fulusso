/* eslint-disable import/no-extraneous-dependencies */
const path = require('path');
const fs = require('fs');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const pagesBathPath = path.resolve(__dirname, 'src/pages');

const APP_NAME = '葫芦藤安全中心';

const DEBUG = !process.argv.find((a) => {
    return a === '--mode=production';
});

const pageTitleMap = {
    login: '登录',
    register: '注册',
    resetPassword: '找回密码',
    bindAccount: '用户绑定',
    index: '安全中心',
};

function toLowerFirstLetter(str) {
    return str && str.slice(0, 1).toLowerCase() + str.slice(1);
}

function htmlPluginCreator(pageName) {
    const name = toLowerFirstLetter(pageName);
    return new HtmlWebpackPlugin({
        filename: `${name}.html`,
        template: path.join(__dirname, 'src', 'index.ejs'),
        chunks: [name],
        inject: true,
        title: pageTitleMap[name] ? `${APP_NAME}-${pageTitleMap[name]}` : APP_NAME,
        description: APP_NAME,
        keywords: APP_NAME,
        favicon: path.join(__dirname, 'src', 'favicon.ico'),
        minify: DEBUG ? {} : {
            collapseBooleanAttributes: true,
            collapseInlineTagWhitespace: true,
            collapseWhitespace: true,
            removeComments: true,
            removeEmptyAttributes: true,
            removeRedundantAttributes: true,
            removeStyleLinkTypeAttributes: true,
            removeScriptTypeAttributes: true,
            trimCustomFragments: true,
            minifyJS: true,
            minifyCSS: true,
            minifyURLs: true,
        },
    });
}

function readPages() {
    const dirStat = fs.statSync(pagesBathPath);
    if (dirStat.isDirectory) {
        return fs.readdirSync(pagesBathPath);
    } else {
        throw new Error('请确保项目包含src/pages目录');
    }
}

function createHtmlPages() {
    return readPages().map((...args) => htmlPluginCreator(...args));
}

function createEntry() {
    return readPages().reduce((obj, pageName) => {
        const name = toLowerFirstLetter(pageName);
        obj[name] = `./src/pages/${pageName}/entry.js`;
        return obj;
    }, {});
}

module.exports = {
    createEntry,
    createHtmlPages,
};
