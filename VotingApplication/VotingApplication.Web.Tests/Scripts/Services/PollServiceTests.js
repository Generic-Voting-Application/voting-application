'use strict';

describe('Poll Service', function () {

    beforeEach(module('GVA.Poll'));

    var pollService;

    var accountService;
    var httpBackend;
    var rootScope;

    beforeEach(inject(function (PollService, AccountService, $httpBackend, $rootScope) {
        pollService = PollService;
        accountService = AccountService;
        httpBackend = $httpBackend;
        rootScope = $rootScope;
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

    });

});