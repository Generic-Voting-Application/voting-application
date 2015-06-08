'use strict';

describe('TokenService', function () {

    beforeEach(module('GVA.Common'));

    var tokenService;
    var routeParams;
    var localStorage;
    var httpBackend;
    var rootScope;

    var pollId = '7C7CE5F8-873D-4F1F-AF3F-D24769813ABC';
    var tokenValue = '7773206A-0A51-4381-A4F1-A53A3201F0DE';

    beforeEach(inject(function (TokenService, $routeParams, $localStorage, $httpBackend, $rootScope) {
        tokenService = TokenService;
        routeParams = $routeParams;
        localStorage = $localStorage;
        httpBackend = $httpBackend;
        rootScope = $rootScope;
    }));

    describe('Get Token', function () {

        it('Returns a null token for a null pollId', function () {

            var nullPollId = null;

            var promise = tokenService.getToken(nullPollId);

            promise
                .then(function () {
                    fail('Expected the promise to be rejected, not resolved.');
                });

            rootScope.$apply();
            // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
            expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
        });

        it('With a token in the route, returns the route token', function () {

            routeParams['tokenId'] = tokenValue;

            var token;

            var promise = tokenService.getToken(pollId);

            promise.then(function (data) {
                token = data;
            });

            rootScope.$apply();
            expect(token).toBe(tokenValue);
        });

        it('With no route token, but a token in local storage, returns the local storage token', function () {
            routeParams['tokenId'] = null;
            localStorage[pollId] = { token: tokenValue };

            var token;

            var promise = tokenService.getToken(pollId);

            promise.then(function (data) {
                token = data;
            });

            rootScope.$apply();
            expect(token).toBe(tokenValue);
        });

        it('With no route token, nor a local storage token, makes http call to get token', function () {
            routeParams['tokenId'] = null;
            localStorage[pollId] = null;

            var expectedUrl = '/api/poll/' + pollId + '/token';

            httpBackend.expect(
                    'GET',
                    expectedUrl
                    ).respond(200, '');

            tokenService.getToken(pollId);

            httpBackend.flush();

            httpBackend.verifyNoOutstandingExpectation();
            httpBackend.verifyNoOutstandingRequest();

            // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
            expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
        });

    });

});