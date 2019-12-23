var webpack = require('webpack');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var path = require("path");

module.exports = env => {

	return{
		devtool: 'eval',
		entry: [
			'babel-polyfill',
			'react-hot-loader/patch',
			'./index.tsx'
		],
		output: {
			filename: 'bundle.js',
			publicPath: "/",
			path: path.resolve(__dirname, "dist")
		},
		context: path.resolve(__dirname, 'src'),
		resolve: {
			extensions: ['.ts', '.tsx', '.js', 'json', '.jsx']
		},
		module: {
			loaders: [
				{
					test: /\.(ts|tsx)$/,
					loader: ['react-hot-loader/webpack','babel-loader', 'ts-loader']
				},
				{
					test: /\.css$/,
					loader: ['style-loader', 'css-loader']
				},
				{
					test: /\.(gif|png|jpg|jpeg|woff|woff2|eot|ttf|svg)$/,
					loader: [ 'url-loader']
				}		
			]
		},
		plugins: [
			new webpack.HotModuleReplacementPlugin(),
			new webpack.NamedModulesPlugin(),
			new HtmlWebpackPlugin({
				inject: true,
				template: 'index.html'
			}),
			new webpack.EnvironmentPlugin({
				NODE_ENV: env.NODE_ENV || 'development',
				REACT_APP_SECURITY_API_URL: env.REACT_APP_SECURITY_API_URL || 'http://localhost:8090/api',
				REACT_APP_SALES_API_URL: env.REACT_APP_SALES_API_URL || 'http://localhost:8091/api'
				})
		],
		devServer: {
			hot: true,
			inline: true,
			host: "localhost",
			port: "8092",
			open :true,
			historyApiFallback: true		
			//Enable this if you want to never refresh (this allows hot-reloading app.tsx, but won't auto-refresh if you change index.tsx)
			//hotOnly: true
		}	
	}
};