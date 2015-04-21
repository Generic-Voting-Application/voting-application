'use strict';

describe('VotingPageController', function () {

    beforeEach(module('GVA.Voting'));

    var scope;
    var mockIdentityService;
    var mockTokenService;
    var mockPollService;
    var mockVoteService;
    var mockRoutingService;

    var mockTokenValue = '4A9AFE2C-240B-4BFF-A569-0FEF5A78FC10';
    var mockPollValue = {
        MaxPoints: 5,
        MaxPerVote: 1,
        Options: [{ Id: 1 }, { Id: 2 }, { Id: 3 }, { Id: 4 }],
        NamedVoting: false
    };
    var mockTokenVotes = [{ OptionId: 1, VoteValue: 1 }, { OptionId: 3, VoteValue: 1 }];

    beforeEach(inject(function ($rootScope, $controller) {

        scope = $rootScope.$new();

        scope.poll = { Options: [] };

        mockIdentityService = {
            identity: {},
            registerIdentityObserver: function () { },
            openLoginDialog: function () { }
        };
        mockTokenService = { getToken: function (pollId, callback) { callback(mockTokenValue); } };
        mockPollService = { getPoll: function (pollId, callback) { callback(mockPollValue); } };
        mockVoteService = {
            getTokenVotes: function (pollId, token, callback) { callback(mockTokenVotes); },
            submitVote: function (pollId, votes, token, callback) { callback(); }
        };
        mockRoutingService = {
            getResultsPageUrl: function () { },
            navigateToResultsPage: function () { }
        };

        spyOn(mockIdentityService, 'registerIdentityObserver').and.callThrough();
        spyOn(mockIdentityService, 'openLoginDialog').and.callThrough();
        spyOn(mockTokenService, 'getToken').and.callThrough();
        spyOn(mockPollService, 'getPoll').and.callThrough();
        spyOn(mockVoteService, 'getTokenVotes').and.callThrough();
        spyOn(mockVoteService, 'submitVote').and.callThrough();
        spyOn(mockRoutingService, 'navigateToResultsPage').and.callThrough();

        $controller('VotingPageController', {
            $scope: scope,
            IdentityService: mockIdentityService,
            TokenService: mockTokenService,
            PollService: mockPollService,
            VoteService: mockVoteService,
            RoutingService: mockRoutingService
        });
    }));


    it('Registers callback with IdentityService', function () {

        expect(mockIdentityService.registerIdentityObserver).toHaveBeenCalled();
    });

    it('Gets token from TokenService and saves it into scope', function () {
        expect(mockTokenService.getToken).toHaveBeenCalled();

        expect(scope.token).toBe(mockTokenValue);
    });

    it('Gets poll from PollService and saves it into scope', function () {

        expect(mockPollService.getPoll).toHaveBeenCalled();

        expect(scope.poll).toBe(mockPollValue);
    });

    it('Gets votes for a given Token', function () {

        expect(mockVoteService.getTokenVotes).toHaveBeenCalled();
    });

    it('Sets the value for the vote into the options', function () {

        var expectedVoteUpdatedOptions = [
            { Id: 1, voteValue: 1 },
            { Id: 2, voteValue: 0 },
            { Id: 3, voteValue: 1 },
            { Id: 4, voteValue: 0 }
        ];

        expect(scope.poll.Options).toEqual(expectedVoteUpdatedOptions);

    });

    describe('Submit Vote', function () {

        it('Does not call Vote Service when options is null', function () {

            scope.submitVote(null);


            expect(mockVoteService.submitVote).not.toHaveBeenCalled();
        });

        it('Does not call Vote Service when there is no token', function () {

            scope.token = null;

            scope.submitVote([]);


            expect(mockVoteService.submitVote).not.toHaveBeenCalled();
        });

        it('Calls what has been set as the voteCallback when there are options to submit', function () {
            var called = false;

            scope.setVoteCallback(function () { called = true; return []; });

            scope.submitVote([]);


            expect(called).toBe(true);
        });

        it('Calls Vote Service when there are options to submit', function () {
            var called = false;

            scope.setVoteCallback(function () { called = true; return []; });

            scope.submitVote([]);


            expect(called).toBe(true);
        });

        it('Calls Routing Service to navigate to the results page when the Vote Service submit is successful', function () {
            scope.submitVote([]);

            expect(mockRoutingService.navigateToResultsPage).toHaveBeenCalled();
        });

        it('With no identity set for a Named Voting poll, calls the IdentityService to show the login dialog', function () {
            mockIdentityService.identity = null;
            scope.poll.NamedVoting = true;

            scope.submitVote([]);


            expect(mockIdentityService.openLoginDialog).toHaveBeenCalled();
        });
    });

    describe('Voter Option Added Event', function () {

        it('Gets poll from PollService and saves it into scope', function () {

            // Clear the spied calls, as it gets called during construction.
            mockPollService.getPoll.calls.reset();

            scope.$emit('voterOptionAddedEvent');

            expect(mockPollService.getPoll).toHaveBeenCalled();
            expect(scope.poll).toBe(mockPollValue);

        });

        it('Maintains selected values after poll reload', function () {
            var poll = {
                Options: [
                    { Id: 1, voteValue: 1 },
                    { Id: 2, voteValue: 2 },
                    { Id: 3, voteValue: 0 },
                    { Id: 4, voteValue: 0 },
                    { Id: 5, voteValue: 3 }
                ]
            };

            mockPollService.getPoll.and.callFake(function (pollId, callback) { callback(poll); });

            var selectedOptions = [
                { Id: 1, voteValue: 1 },
                { Id: 2, voteValue: 0 },
                { Id: 3, voteValue: 0 },
                { Id: 4, voteValue: 2 }
            ];

            var expectedOptions = [
                { Id: 1, voteValue: 1 },
                { Id: 2, voteValue: 0 },
                { Id: 3, voteValue: 0 },
                { Id: 4, voteValue: 2 },
                { Id: 5, voteValue: 0 }
            ];

            scope.poll.Options = selectedOptions;

            scope.$emit('voterOptionAddedEvent');


            expect(scope.poll.Options).toEqual(expectedOptions);
        });

    });

});