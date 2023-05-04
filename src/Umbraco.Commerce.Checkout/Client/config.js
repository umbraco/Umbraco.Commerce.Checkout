const srcPath = './src';
const backofficePath = './src/backoffice';

var argv = require('minimist')(process.argv.slice(2));

var outputPath = argv['output-path'];
if (!outputPath) {
  outputPath = require('./config.outputPath.js').outputPath;
}

exports.paths = {
  src: [`${srcPath}/**/*.*`, `!${srcPath}/css/**/*.*`],
  dest: outputPath
};

exports.config = {
  prod: argv["prod"] || false,
};
