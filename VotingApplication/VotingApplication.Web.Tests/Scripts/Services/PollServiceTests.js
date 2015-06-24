'use strict';

describe('Poll Service', function () {

    beforeEach(module('GVA.Poll'));

    var pollService;

    var accountService;
    var httpBackend;
    var rootScope;
    var errors;

    beforeEach(inject(function (PollService, AccountService, Errors, $httpBackend, $rootScope) {
        pollService = PollService;
        accountService = AccountService;
        httpBackend = $httpBackend;
        rootScope = $rootScope;
        errors = Errors;
    }));

    afterEach(function () {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();

        // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
        expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
    });

    describe('Copy Poll', function () {

        it('Adds account token to Authorization header', function () {

            var pollId = null;
            var tokenValue = 'Some long token value...';

            accountService.account = { token: tokenValue };

            var expectedUrl = '/api/dashboard/copy';
            var expectedAuthorizationHeader = 'Bearer ' + tokenValue;
            var expectedData = '{"UUIDToCopy":null}';

            httpBackend.expect(
                   'POST',
                   expectedUrl,
                   expectedData,
                   function (headers) {
                       return headers['Authorization'] === expectedAuthorizationHeader;
                   }
               ).respond(200);

            pollService.copyPoll(pollId);

            httpBackend.flush();
        });

        it('Adds the pollId to the request', function () {
            var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
            var expectedUrl = '/api/dashboard/copy';

            var expectedData = '{"UUIDToCopy":"' + pollId + '"}';

            accountService.account = { token: null };

            httpBackend.expect(
                   'POST',
                   expectedUrl,
                   expectedData
               ).respond(200);

            pollService.copyPoll(pollId);

            httpBackend.flush();
        });
    });

    describe('Get User Polls', function () {

        it('Adds account token to Authorization header', function () {

            var tokenValue = 'Some long token value...';

            accountService.account = { token: tokenValue };

            var expectedUrl = '/api/dashboard/polls';
            var expectedAuthorizationHeader = 'Bearer ' + tokenValue;
            var expectedData = null;

            httpBackend.expect(
                   'GET',
                   expectedUrl,
                   expectedData,
                   function (headers) {
                       return headers['Authorization'] === expectedAuthorizationHeader;
                   }
               ).respond(200);

            pollService.getUserPolls();

            httpBackend.flush();
        });
    });

    describe('Create Poll', function () {

        it('Given an account token, it adds it to the Authorization header', function () {

            var newQuestion = null;
            var tokenValue = 'Some long token value...';

            accountService.account = { token: tokenValue };

            var expectedUrl = 'api/poll';
            var expectedAuthorizationHeader = 'Bearer ' + tokenValue;
            var expectedData = '{"PollName":null}';

            httpBackend.expect(
                   'POST',
                   expectedUrl,
                   expectedData,
                   function (headers) {
                       return headers['Authorization'] === expectedAuthorizationHeader;
                   }
               ).respond(200);

            pollService.createPoll(newQuestion);

            httpBackend.flush();
        });

        it('Given an empty account, it adds null to the Authorization header', function () {

            var newQuestion = null;
            accountService.account = null;

            var expectedUrl = 'api/poll';
            var expectedAuthorizationHeader = 'Bearer ' + null;
            var expectedData = '{"PollName":null}';

            httpBackend.expect(
                   'POST',
                   expectedUrl,
                   expectedData,
                   function (headers) {
                       return headers['Authorization'] === expectedAuthorizationHeader;
                   }
               ).respond(200);

            pollService.createPoll(newQuestion);

            httpBackend.flush();
        });

        it('Adds the new question to the request', function () {

            var newQuestion = 'Aren\'t you a little short for a StormTrooper?';
            var tokenValue = 'Some long token value...';

            accountService.account = { token: tokenValue };

            var expectedUrl = 'api/poll';
            var expectedAuthorizationHeader = 'Bearer ' + tokenValue;
            var expectedData = '{"PollName":"' + newQuestion + '"}';

            httpBackend.expect(
                   'POST',
                   expectedUrl,
                   expectedData,
                   function (headers) {
                       return headers['Authorization'] === expectedAuthorizationHeader;
                   }
               ).respond(200);

            pollService.createPoll(newQuestion);

            httpBackend.flush();
        });

    });

    describe('Get Poll', function () {

        it('Returns a failure for a null pollId', function () {
            var pollId = null;

            var promise = pollService.getPoll(pollId);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
        });

        it('Given no token, makes request without X-TokenGuid header', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.expectGET(
                    expectedUrl,
                    function (headers) {
                        return headers['X-TokenGuid'] === undefined;
                    }
                ).respond(200);

            pollService.getPoll(pollId);

            httpBackend.flush();
        });

        it('Given a token, makes request with the X-TokenGuid header', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.expectGET(
                    expectedUrl,
                    function (headers) {
                        return headers['X-TokenGuid'] === ballotTokenGuid;
                    }
                ).respond(200);

            pollService.getPoll(pollId, ballotTokenGuid);

            httpBackend.flush();
        });

        it('Returns just the data for a success', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            var expectedResponseData = 'SomeData';


            httpBackend.whenGET(
                    expectedUrl
                ).respond(200, expectedResponseData);


            var responseData = null;

            pollService.getPoll(pollId, ballotTokenGuid)
                .then(function (data) {
                    responseData = data;
                });

            httpBackend.flush();

            expect(responseData).toEqual(expectedResponseData);
        });

        it('Rejects the promise when there is an error', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.whenGET(
                    expectedUrl
                ).respond(404);


            var prom = pollService.getPoll(pollId, ballotTokenGuid);

            httpBackend.flush();

            expect(prom.$$state.status).toBe(2);
        });

        it('Returns the correct error when the poll is not found', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.whenGET(
                    expectedUrl
                ).respond(404);


            var errorMessage = null;

            pollService.getPoll(pollId, ballotTokenGuid)
                .catch(function (message) {
                    errorMessage = message;
                });

            httpBackend.flush();


            expect(errorMessage).toBe(errors.PollNotFound);
        });

        it('Returns the correct error when the poll is invite only and an invalid token is supplied', function () {
            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.whenGET(
                    expectedUrl
                ).respond(401);


            var errorMessage = null;

            pollService.getPoll(pollId, ballotTokenGuid)
                .catch(function (message) {
                    errorMessage = message;
                });

            httpBackend.flush();


            expect(errorMessage).toBe(errors.PollInviteOnlyNoToken);
        });

        it('Returns the readableMessage property in unknown error cases', function () {

            var pollId = '5CE7BB52-5058-4033-B2A4-5C4214AC4AFA';
            var ballotTokenGuid = '0310812A-05D3-41BA-AA34-CF7BF8330B67';

            var expectedUrl = '/api/poll/' + pollId;

            httpBackend.whenGET(
                    expectedUrl
                ).respond(400);

            var errorMessage = null;

            pollService.getPoll(pollId, ballotTokenGuid)
                .catch(function (message) {
                    errorMessage = message;
                });

            httpBackend.flush();


            var expectedErrorMessage = 'An error has occured';
            expect(errorMessage).toBe(expectedErrorMessage);
        });
    });
});