const gulp = require('gulp');
const rename = require('gulp-rename');
const concat = require('gulp-concat');
const postcss = require('gulp-postcss');

const argv = require('minimist')(process.argv.slice(2));

const srcPath = './src';
const backofficePath = './src/backoffice';
const outputPath = argv['output-path'] || require('./config.outputPath.js').outputPath;

const copy = () => {
  return gulp.src([`${srcPath}/**/*.*`, `!${srcPath}/css/**/*.*`, `!${backofficePath}/**/*.js`])
    .pipe(gulp.dest(outputPath))
};

const fe_css = () => {
  return gulp.src('./src/css/main.css') 
    .pipe(postcss([
      require('tailwindcss'),
      require('autoprefixer'),
      require('cssnano')({
       preset: 'default',
      })
    ]))
    .pipe(rename('uccheckout.css'))
    .pipe(gulp.dest(`${outputPath}/css`));
}

const be_js = () => {
  return gulp.src([`${backofficePath}/**/*.js`])
    .pipe(concat(`uccheckout.js`))
    .pipe(gulp.dest(`${outputPath}/backoffice/js`));
}

exports.build = gulp.task('build', gulp.parallel(copy, be_js, fe_css));
