// Include gulp
var gulp = require('gulp'); 

// Include Plugins
var jshint = require('gulp-jshint');
var uglify = require('gulp-uglify');
var rjs = require('gulp-requirejs');

// Lint Task
// gulp.task('lint', function() {
//     return gulp.src('Scripts/**/*.js')
//         .pipe(jshint())
//         .pipe(jshint.reporter('default'));
// });

gulp.task('requirejsBuildCreate', function() {
    rjs({
        baseUrl: './Scripts/',
        name: 'Create',
        out: 'Create.js',
        mainConfigFile: './Scripts/Config.js'
    })
    .pipe(uglify())
    .pipe(gulp.dest('./Scripts/Min/'));
});

gulp.task('requirejsBuildManage', function() {
    rjs({
        baseUrl: './Scripts/',
        name: 'Manage',
        out: 'Manage.js',
        mainConfigFile: './Scripts/Config.js'
    })
    //.pipe(uglify())
    .pipe(gulp.dest('./Scripts/Min/'));
});

gulp.task('requirejsBuildPoll', function() {
    rjs({
        baseUrl: './Scripts/',
        name: 'Poll',
        out: 'Poll.js',
        mainConfigFile: './Scripts/Config.js',
        include: ['BasicVote', 'RankedVote', 'PointsVote']
    })
    .pipe(uglify())
    .pipe(gulp.dest('./Scripts/Min/'));
});

// Deploy
gulp.task('Deploy', [/*'lint',*/ 'requirejsBuildCreate', 'requirejsBuildManage', 'requirejsBuildPoll']);