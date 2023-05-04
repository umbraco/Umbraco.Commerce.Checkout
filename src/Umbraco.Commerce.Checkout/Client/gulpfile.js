const gulp = require('gulp');

const { paths } = require('./config');

const copy = () => gulp.src(paths.src).pipe(gulp.dest(paths.dest));

const css = () => {
  //return gulp.src(paths.js)
  //  .pipe(gulp.dest(`${paths.dest}/backoffice/js`));
}

gulp.task('copy', copy);

exports.build = gulp.task('build', gulp.parallel(copy));
