module.exports = function(grunt) {
    grunt.initConfig({
    jshint: {
        all: [
            'VotingApplication/VotingApplication.Web/Scripts/**/*.js',
            '!VotingApplication/VotingApplication.Web/Scripts/Lib/**/*.js',
			'VotingApplication.Web.Api.Tests/Scripts/**/*.js',
			'!VotingApplication.Web.Api.Tests/Scripts/Lib/**/*.js'
			
        ],
        options: {
            jshintrc: true,
            force: false
        }
    }
});

grunt.loadNpmTasks('grunt-contrib-jshint');

grunt.registerTask('default', ['jshint']);

};
