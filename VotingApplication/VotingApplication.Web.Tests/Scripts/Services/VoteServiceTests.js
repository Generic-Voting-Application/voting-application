'use strict';

describe('Vote Service', function () {

    beforeEach(module('GVA.Voting'));

    var voteService;
    var rootScope;
    var httpBackend;
    var errors;

    beforeEach(inject(function (VoteService, $rootScope, $httpBackend, Errors) {
        voteService = VoteService;
        rootScope = $rootScope;
        httpBackend = $httpBackend;
        errors = Errors;
    }));

    afterEach(function () {
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

            spyOn(Date, 'now').and.callFake(function () { return 1382482800000; });

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

        it('Returns just the data for a success', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            var expectedResponseData = 'SomeData';


            httpBackend
                .expectGET(expectedUrl)
                .respond(200, expectedResponseData);


            var responseData = null;

            voteService.getResults(pollId, ballotTokenGuid)
                .then(function (data) {
                    responseData = data;
                });

            httpBackend.flush();

            expect(responseData).toEqual(expectedResponseData);
        });

        it('Returns the correct error when the poll is not found', function () {

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';

            var url = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.whenGET(url).respond(404, '');

            var response = null;
            voteService
                .getResults(pollId)
                .catch(function (error) { response = error; });

            httpBackend.flush();

            expect(response).toBe(errors.PollNotFound);
        });

        it('Returns the correct error when the poll is invite only and an invalid token is supplied', function () {

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';

            var url = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.whenGET(url).respond(401, '');

            var response = null;
            voteService
                .getResults(pollId)
                .catch(function (error) { response = error; });

            httpBackend.flush();

            expect(response).toBe(errors.PollInviteOnlyNoToken);
        });

        it('Returns the readableMessage property in unknown error cases', function () {

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';

            var url = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.whenGET(url).respond(499, '');

            var response = null;
            voteService
                .getResults(pollId)
                .catch(function (error) { response = error; });

            httpBackend.flush();

            var expectedErrorMessage = 'An error has occured';
            expect(response).toBe(expectedErrorMessage);
        });

        it('Given no token, makes request without X-TokenGuid header', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';

            var expectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.expectGET(
                    expectedUrl,
                    function (headers) {
                        return headers['X-TokenGuid'] === undefined;
                    }
                ).respond(200);

            voteService.getResults(pollId);

            httpBackend.flush();
        });

        it('Given a token, makes request with the X-TokenGuid header', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId + '/results?lastRefreshed=0';

            httpBackend.expectGET(
                    expectedUrl,
                    function (headers) {
                        return headers['X-TokenGuid'] === ballotTokenGuid;
                    }
                ).respond(200);

            voteService.getResults(pollId, ballotTokenGuid);

            httpBackend.flush();
        });
    });

    describe('Submit Vote', function () {

        it('Returns a failure for a null pollId', function () {
            var pollId = null;


            var promise = voteService.submitVote(pollId);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Returns a failure for null votes', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var votes = null;


            var promise = voteService.submitVote(pollId, votes);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Returns a failure for a null token', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var votes = [];
            var token = null;

            var promise = voteService.submitVote(pollId, votes, token);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Makes http call to submit vote', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var votes = [];
            var token = '64C66CBC-5A5F-481E-9546-26DCE015CD40';

            var expectedUrl = '/api/poll/' + pollId + '/token/' + token + '/vote';

            httpBackend.expect(
                    'PUT',
                    expectedUrl
                    ).respond(200, '');

            voteService.submitVote(pollId, votes, token);

            httpBackend.flush();
        });

    });

    describe('Add Voter Choice', function () {

        it('Makes http call to add choice', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var newChoice = {};

            var expectedUrl = '/api/poll/' + pollId + '/choice';

            httpBackend.expect(
                    'POST',
                    expectedUrl
                    ).respond(200, '');

            voteService.addVoterChoice(pollId, newChoice);

            httpBackend.flush();

        });

    });

    describe('Get Token Votes', function () {

        it('Returns a failure for a null pollId', function () {

            var pollId = null;


            var promise = voteService.getTokenVotes(pollId);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Returns a failure for a null tokenId', function () {

            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var token = null;

            var promise = voteService.getTokenVotes(pollId, token);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Makes http call to get votes for a token', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var token = '64C66CBC-5A5F-481E-9546-26DCE015CD40';

            var expectedUrl = '/api/poll/' + pollId + '/token/' + token + '/vote';

            httpBackend.expect(
                'GET',
                expectedUrl
            ).respond(200, '');

            voteService.getTokenVotes(pollId, token);

            httpBackend.flush();
        });
    });
});