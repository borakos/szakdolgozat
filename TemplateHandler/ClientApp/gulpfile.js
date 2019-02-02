const gulp = require('gulp');
const concat = require('gulp-concat');

gulp.task('concat', function() {
  return gulp.src([
    './dist/ClientApp/runtime.js',
    './dist/ClientApp/polyfills.js',
    './dist/ClientApp/scripts.js',
    './dist/ClientApp/main.js',
  ]).pipe(concat('app-nav.js', { newLine: ';' }))
    .pipe(gulp.dest('./dist/'));
});

gulp.task('default', ['concat']);