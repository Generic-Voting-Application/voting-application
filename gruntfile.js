module.exports = function(grunt) {
    grunt.initConfig({
    jshint: {
        all: [
            'VotingApplication/VotingApplication.Web/Scripts/Controllers/**/*.js',
            'VotingApplication/VotingApplication.Web/Scripts/Directives/**/*.js',
            'VotingApplication/VotingApplication.Web/Scripts/Modules/**/*.js',
            'VotingApplication/VotingApplication.Web/Scripts/Services/**/*.js'
        ],
        options: {
            jshintrc: true
        }
    }
});

grunt.loadNpmTasks('grunt-contrib-jshint');

grunt.registerTask('default', ['jshint']);

};
