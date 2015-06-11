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

    describe('Retrieve Token', function () {

        it('Returns null for a null pollId', function () {

            var nullPollId = null;

            var token = tokenService.retrieveToken(nullPollId);

            expect(token).toBe(null);
        });

        it('With a token in the route, returns the route token', function () {

            routeParams['tokenId'] = tokenValue;

            var token = tokenService.retrieveToken(pollId);

            expect(token).toBe(tokenValue);
        });

        it('With no route token, but a token in local storage, returns the local storage token', function() {
            routeParams['tokenId'] = null;
            localStorage[pollId] = { token: tokenValue };

            var token = tokenService.retrieveToken(pollId);

            expect(token).toBe(tokenValue);

        });

        it('With no route token, but an old style token in local storage, returns the local storage token', function () {
            routeParams['tokenId'] = null;
            localStorage[pollId] = tokenValue;

            var token = tokenService.retrieveToken(pollId);

            expect(token).toBe(tokenValue);

        });

        it('With no route token, nor a local storage token, returns null', function() {

            routeParams['tokenId'] = null;
            localStorage[pollId] = null;

            var token = tokenService.retrieveToken(pollId);

            expect(token).toBe(null);
        });
    });

});