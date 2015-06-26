'use strict';

describe('VotingPageController', function () {

    beforeEach(module('GVA.Voting'));

    var scope;
    var mockIdentityService;
    var mockTokenService;
    var mockPollService;
    var mockVoteService;
    var mockRoutingService;
    var mockRouteParams;

    var setTokenPromise;
    var getTokenPromise;
    var getManageIdPromise;
    var submitVotePromise;
    var getTokenVotesPromise;
    var getPollPromise;

    var mockTokenValue = '4A9AFE2C-240B-4BFF-A569-0FEF5A78FC10';
    var mockPollValue = {
        MaxPoints: 5,
        MaxPerVote: 1,
        Choices: [{ Id: 1 }, { Id: 2 }, { Id: 3 }, { Id: 4 }],
        NamedVoting: false,

        TokenGuid: mockTokenValue
    };

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        scope.poll = { Choices: [] };

        getTokenPromise = $q.defer();
        getManageIdPromise = $q.defer();
        setTokenPromise = $q.defer();

        mockRouteParams = { pollId: '5130518A-4DA5-4FAA-B8FC-0242C9CAA079' };

        mockIdentityService = {
            identity: { name: 'Bob' },
            registerIdentityObserver: function () { },
            openLoginDialog: function () { }
        };
        mockTokenService = {
            setToken: function() {
                return setTokenPromise.promise;
            },
            retrieveToken: function () {
                return mockTokenValue;
            },
            getManageId: function () {
                return getManageIdPromise.promise;
            }
        };

        mockPollService = {
            getPoll: function () { }
        };
        getPollPromise = $q.defer();
        spyOn(mockPollService, 'getPoll').and.callFake(function () { return getPollPromise.promise; });

        mockVoteService = {
            getTokenVotes: function () { },
            submitVote: function () { }
        };
        submitVotePromise = $q.defer();
        getTokenVotesPromise = $q.defer();
        spyOn(mockVoteService, 'submitVote').and.callFake(function () { return submitVotePromise.promise; });
        spyOn(mockVoteService, 'getTokenVotes').and.callFake(function () { return getTokenVotesPromise.promise; });

        mockRoutingService = {
            getResultsPageUrl: function () { },
            navigateToResultsPage: function () { }
        };

        spyOn(mockIdentityService, 'registerIdentityObserver').and.callThrough();
        spyOn(mockIdentityService, 'openLoginDialog').and.callThrough();
        spyOn(mockTokenService, 'retrieveToken').and.callThrough();
        spyOn(mockTokenService, 'setToken').and.callThrough();
        spyOn(mockRoutingService, 'navigateToResultsPage').and.callThrough();

        $controller('VotingPageController', {
            $scope: scope,
            IdentityService: mockIdentityService,
            TokenService: mockTokenService,
            PollService: mockPollService,
            VoteService: mockVoteService,
            RoutingService: mockRoutingService,
            $routeParams: mockRouteParams
        });
    }));

    it('Registers callback with IdentityService', function () {

        expect(mockIdentityService.registerIdentityObserver).toHaveBeenCalled();
    });

    it('Gets poll from PollService and saves it into scope', function () {

        getPollPromise.resolve(mockPollValue);
        setTokenPromise.resolve();
        scope.$apply();

        expect(mockPollService.getPoll).toHaveBeenCalled();

        expect(scope.poll).toBe(mockPollValue);
    });

    it('Gets poll from PollService and saves the token', function () {

        getPollPromise.resolve(mockPollValue);
        setTokenPromise.resolve();
        scope.$apply();

        expect(mockTokenService.setToken).toHaveBeenCalled();
    });

    it('Gets votes for a given Token', function () {

        getPollPromise.resolve({ data: mockPollValue });
        setTokenPromise.resolve();
        scope.$apply();

        expect(mockVoteService.getTokenVotes).toHaveBeenCalled();
    });

    it('Sets the value for the vote into the choices', function () {
        var tokenVotes = [
            { ChoiceId: 1, VoteValue: 1 },
            { ChoiceId: 3, VoteValue: 1 }
        ];


        var tokenResponse = { data: tokenVotes };
        getTokenVotesPromise.resolve(tokenResponse);
        getPollPromise.resolve(mockPollValue);


        var expectedVoteUpdatedChoices = {
            data: [
                { Id: 1, voteValue: 1 },
                { Id: 2, voteValue: 0 },
                { Id: 3, voteValue: 1 },
                { Id: 4, voteValue: 0 }
            ]
        };
        setTokenPromise.resolve();

        scope.$apply();
        expect(scope.poll.Choices).toEqual(expectedVoteUpdatedChoices.data);
    });

    describe('Submit Vote', function () {

        beforeEach(function () {
            setTokenPromise.resolve();
            getPollPromise.resolve(mockPollValue);
            scope.$apply();
        });

        it('Does not call Vote Service when choices is null', function () {

            scope.submitVote(null);


            expect(mockVoteService.submitVote).not.toHaveBeenCalled();
        });

        it('Does not call Vote Service when there is no token', function () {

            scope.token = null;

            scope.submitVote([]);


            expect(mockVoteService.submitVote).not.toHaveBeenCalled();
        });

        it('Calls what has been set as the voteCallback when there are choices to submit', function () {
            var called = false;

            scope.setVoteCallback(function () { called = true; return []; });

            scope.submitVote([]);


            expect(called).toBe(true);
        });

        it('Calls Vote Service when there are choices to submit', function () {
            var called = false;

            scope.setVoteCallback(function () { called = true; return []; });

            scope.submitVote([]);


            expect(called).toBe(true);
        });

        it('Calls Routing Service to navigate to the results page when the Vote Service submit is successful', function () {
            submitVotePromise.resolve();

            scope.submitVote([]);


            scope.$apply();
            expect(mockRoutingService.navigateToResultsPage).toHaveBeenCalled();
        });

        it('With no identity set for a Named Voting poll, calls the IdentityService to show the login dialog', function () {
            mockIdentityService.identity = null;
            scope.poll.NamedVoting = true;

            scope.submitVote([]);


            expect(mockIdentityService.openLoginDialog).toHaveBeenCalled();
        });
    });

    describe('Voter Choice Added Event', function () {

        it('Gets poll from PollService and saves it into scope', function () {

            getPollPromise.resolve(mockPollValue);
            setTokenPromise.resolve();
            scope.$apply();

            // Clear the spied calls, as it gets called during construction.
            mockPollService.getPoll.calls.reset();

            scope.$emit('voterChoiceAddedEvent');

            expect(mockPollService.getPoll).toHaveBeenCalled();
            expect(scope.poll).toBe(mockPollValue);

        });

        it('Maintains selected values after poll reload', function () {
            var poll = {
                Choices: [
                    { Id: 1, voteValue: 1 },
                    { Id: 2, voteValue: 2 },
                    { Id: 3, voteValue: 0 },
                    { Id: 4, voteValue: 0 },
                    { Id: 5, voteValue: 3 }
                ]
            };

            var selectedChoices = [
                { Id: 1, voteValue: 1 },
                { Id: 2, voteValue: 0 },
                { Id: 3, voteValue: 0 },
                { Id: 4, voteValue: 2 }
            ];

            var expectedChoices = [
                { Id: 1, voteValue: 1 },
                { Id: 2, voteValue: 0 },
                { Id: 3, voteValue: 0 },
                { Id: 4, voteValue: 2 },
                { Id: 5, voteValue: 0 }
            ];

            scope.poll.Choices = selectedChoices;

            getPollPromise.resolve(poll);
            scope.$emit('voterChoiceAddedEvent');
            setTokenPromise.resolve();

            scope.$apply();
            expect(scope.poll.Choices).toEqual(expectedChoices);
        });

    });

    describe('Clear Vote', function () {

        beforeEach(function () {
            getPollPromise.resolve(mockPollValue);
            setTokenPromise.resolve();
            scope.$apply();
        });

        it('Calls Vote Service to clear the vote', function () {

            scope.clearVote();

            expect(mockVoteService.submitVote).toHaveBeenCalled();
        });

        it('Calls Vote Service with no votes', function () {

            scope.clearVote();

            var expectedBallot = {
                VoterName: 'Bob',
                Votes: []
            };

            expect(mockVoteService.submitVote).toHaveBeenCalledWith(jasmine.any(String), expectedBallot, jasmine.any(String));
        });

        it('Calls Routing Service to navigate to the results page when the Vote Service submit is successful', function () {

            submitVotePromise.resolve();

            scope.clearVote();


            scope.$apply();
            expect(mockRoutingService.navigateToResultsPage).toHaveBeenCalled();
        });

    });

});