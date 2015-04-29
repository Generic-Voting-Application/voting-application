'use strict';

describe('Vote Service', function () {

    beforeEach(module('GVA.Voting'));

    var voteService;
    var rootScope;
    var httpBackend;

    beforeEach(inject(function (VoteService, $rootScope, $httpBackend) {
        voteService = VoteService;
        rootScope = $rootScope;
        httpBackend = $httpBackend;
    }));

    afterEach(function() {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();

        // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
        expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
    });

    describe('Get Results', function () {

        it('Returns a failure for a null pollId', function () {
            var pollId = null;

            httpBackend.when(
                    'GET',
                    '/api/poll/' + pollId + '/results?lastRefreshed=0'
                    ).respond(404, '');

            var promise = voteService.getResults(pollId);

            httpBackend.flush();

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Makes http call to get results', function () {

            var baseTime = new Date(2013, 9, 23);
            jasmine.clock().mockDate(baseTime);

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';

            var expectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.expect(
                    'GET',
                    expectedUrl
                    ).respond(200, '');


            voteService.getResults(pollId);

            httpBackend.flush();
        });

        it('Passes the last requested datetime on multiple calls', function () {

            var baseTime = new Date(2013, 9, 23); // 1382482800000
            jasmine.clock().mockDate(baseTime);

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var initialExpectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=0';
            httpBackend.expect(
                    'GET',
                    initialExpectedUrl
                    ).respond(200, '');

            voteService.getResults(pollId);
            httpBackend.flush();



            var subesquentExpectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=1382482800000';
            httpBackend.expect(
                    'GET',
                    subesquentExpectedUrl
                    ).respond(200, '');


            voteService.getResults(pollId);

            httpBackend.flush();
        });
    });
});