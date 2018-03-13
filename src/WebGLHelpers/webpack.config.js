var path = require("path");
var webpack = require("webpack");
var fableUtils = require("fable-utils");
var fs = require('fs');
var copyWebpackPlugin = require('copy-webpack-plugin');

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

var babelOptions = fableUtils.resolveBabelOptions({
  presets: [
    ["env", {
      "targets": {
        "browsers": ["last 2 versions"]
      },
      "modules": false
    }]
  ],
  plugins: ["transform-runtime"]
});

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var outPath = resolve('../../Build/Server');

module.exports = {
  devtool: "source-map",
  entry: resolve('./WebGLHelpers.fsproj'),
  output: {
    filename: 'scripts/app.js',
    path: outPath + "/Client",
  },
  resolve: {
    modules: [ resolve("./node_modules")]
  }, 
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules[\\\/](?!fable-)/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      }
    ]
  },
    devServer: {
        contentBase: "./Client",
        port: 8081
    },

    plugins: isProduction ? [
        new copyWebpackPlugin([
            { from: 'Client/**/*.*', to: outPath }
        ])
    ] : [
    ]
};