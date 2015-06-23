'use strict';

describe('Results Page Controller', function () {

    beforeEach(module('GVA.Voting'));

    var scope;

    var errors;

    var mockVoteService;
    var mockPollService;
    var mockRoutingService;

    var voteGetResultsPromise;
    var getPollPromise;

    var getPollResponse = { ExpiryDateUtc: null };
    var getElectionPollResponse = { ElectionMode: true };

    beforeEach(function () {
        jasmine.clock().install();
    });

    beforeEach(inject(function ($rootScope, $q, $controller, Errors) {

        scope = $rootScope.$new();

        errors = Errors;

        mockVoteService = {
            refreshLastChecked: function () { },
            getResults: function () { }
        };

        mockRoutingService = {
            navigateToVotePage: function () { },
            getVotePageUrl : function() { }
        };

        voteGetResultsPromise = $q.defer();
        spyOn(mockVoteService, 'getResults').and.callFake(function () { return voteGetResultsPromise.promise; });

        mockPollService = {
            getPoll: function () { }
        };
        getPollPromise = $q.defer();
        spyOn(mockPollService, 'getPoll').and.callFake(function () { return getPollPromise.promise; });

        spyOn(mockRoutingService, 'navigateToVotePage').and.callThrough();

        $controller('ResultsPageController', {
            $scope: scope,
            VoteService: mockVoteService,
            PollService: mockPollService,
            RoutingService : mockRoutingService
        });
    }));

    afterEach(function () {
        jasmine.clock().uninstall();
    });

    it('Calls vote service to get results when getPoll is successful', function () {
        getPollPromise.resolve(getPollResponse);
        scope.$apply();

        expect(mockVoteService.getResults).toHaveBeenCalled();
    });

    it('Does not calls vote service to get results when getPoll fails', function () {
        getPollPromise.reject(errors.PollNotFound);
        scope.$apply();

        expect(mockVoteService.getResults).not.toHaveBeenCalled();
    });

    it('Calls vote service to get results after 3 seconds when getPoll is successful', function () {

        getPollPromise.resolve(getPollResponse);
        scope.$apply();

        mockVoteService.getResults.calls.reset();

        expect(mockVoteService.getResults).not.toHaveBeenCalled();

        jasmine.clock().tick(3000);

        expect(mockVoteService.getResults).toHaveBeenCalled();
    });

    it('Does not calls vote service to get results after 3 seconds when getPoll fails', function () {

        getPollPromise.reject(errors.PollNotFound);
        scope.$apply();

        mockVoteService.getResults.calls.reset();

        expect(mockVoteService.getResults).not.toHaveBeenCalled();

        jasmine.clock().tick(3000);

        expect(mockVoteService.getResults).not.toHaveBeenCalled();
    });

    it('Removes the timer if the vote service returns an error code (400+) from get results', function () {

        var response = { data: {}, status: 404 };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.reject(response);

        scope.$apply();
        jasmine.clock().tick(3000);

        // First tick calls getResults, which then fails, and removes the timer.
        // Second tick should therefore not call it.

        mockVoteService.getResults.calls.reset();
        jasmine.clock().tick(3000);


        expect(mockVoteService.getResults).not.toHaveBeenCalled();

    });

    it('Does not remove the timer if the vote service returns a success from get results', function () {
        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve({});

        scope.$apply();
        jasmine.clock().tick(3000);

        mockVoteService.getResults.calls.reset();
        jasmine.clock().tick(3000);


        expect(mockVoteService.getResults).toHaveBeenCalled();

    });

    it('Sets hasVotes to true if there is a winner', function () {
        var resultData = { Winners: ['One'] };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.hasVotes).toBe(true);
    });

    it('Sets hasVotes to false if there are no winners', function () {
        var resultData = { Winners: [] };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.hasVotes).toBe(false);
    });

    it('Sets Winner to the single winning option', function () {
        var resultData = {
            Winners: ['Winning option']
        };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.winner).toBe('Winning option');
    });

    it('Sets Winner to the combined list of all winners', function () {
        var resultData = {
            Winners: ['Winning option', 'Another Winning option']
        };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.winner).toBe('Winning option, Another Winning option');
    });

    it('Sets plural to empty if there is a single winner', function () {
        var resultData = {
            Winners: [{ Name: 'Winning option' }]
        };

        var response = { data: resultData };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.plural).toBe('');
    });

    it('Sets plural if there is more than one winner', function () {
        var resultData = {
            Winners: ['Winning option', 'Another Winning option']
        };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.plural).toBe('s (Draw)');
    });

    it('Sets chart data if there are Results', function () {
        var resultData = {
            Results: [
            {
                ChoiceName: 'Choice 1',
                Sum: 1,
                Voters: [{ Name: 'Bob', Value: 1 }]
            },
            {
                ChoiceName: 'Choice 2',
                Sum: 3,
                Voters: [{ Name: 'Bob', Value: 2 }, { Name: 'Derek', Value: 1 }]
            }]
        };

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve(resultData);

        var expectedChartData = [
            {
                Name: 'Choice 1',
                Sum: 1,
                Voters: [{ Name: 'Bob', Value: 1 }]
            },
            {
                Name: 'Choice 2',
                Sum: 3,
                Voters: [{ Name: 'Bob', Value: 2 }, { Name: 'Derek', Value: 1 }]
            }];


        scope.$apply();
        jasmine.clock().tick(3000);

        expect(scope.chartData).toEqual(expectedChartData);
    });

    it('Given a response with no data, does not update any values', function () {
        var response = { data: null };

        voteGetResultsPromise.resolve(response);

        var chartData = [
            {
                Name: 'Choice 1',
                Sum: 1,
                Voters: [{ Name: 'Bob', Value: 1 }]
            },
            {
                Name: 'Choice 2',
                Sum: 3,
                Voters: [{ Name: 'Bob', Value: 2 }, { Name: 'Derek', Value: 1 }]
            }];

        scope.hasVotes = false;
        scope.winner = 'A winner is you!';
        scope.plural = '';
        scope.chartData = chartData;

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.hasVotes).toBe(false);
        expect(scope.winner).toBe('A winner is you!');
        expect(scope.plural).toBe('');
        expect(scope.chartData).toBe(chartData);
    });

    it('Calls poll service to get poll expiry', function () {

        expect(mockPollService.getPoll).toHaveBeenCalled();
    });

    it('Leaves hasExpired false if poll never expires', function () {

        getPollPromise.resolve(getPollResponse);

        scope.$apply();
        expect(scope.hasExpired).toBe(false);
    });

    it('Leaves hasExpired false if expiry date is after now', function () {

        var baseTime = new Date(2015, 1, 1);
        jasmine.clock().mockDate(baseTime);

        var pollData = {
            ExpiryDate: new Date(2015, 2, 1)
        };

        getPollPromise.resolve(pollData);

        scope.$apply();
        expect(scope.hasExpired).toBe(false);
    });

    it('Sets hasExpired to true if expiry date is in the past', function () {

        var baseTime = new Date(2015, 1, 1);
        jasmine.clock().mockDate(baseTime);

        var pollData = {
            ExpiryDateUtc: new Date(2014, 12, 25)
        };

        getPollPromise.resolve(pollData);

        scope.$apply();
        expect(scope.hasExpired).toBe(true);
    });

    it('Sets Error Text to getPoll error when PollService returns an error', function () {

        getPollPromise.reject(errors.PollNotFound);
        scope.$apply();

        expect(scope.errorText).toBe(errors.PollNotFound.Text);
    });

    it('Sets hasError to true when PollService returns an error', function () {

        getPollPromise.reject(errors.PollNotFound);
        scope.$apply();

        expect(scope.hasError).toBe(true);
    });

    it('loaded is false before PollService returns', function () {

        expect(scope.loaded).toBe(false);
    });

    it('loaded is true when PollService returns an error', function () {

        getPollPromise.reject(errors.PollNotFound);
        scope.$apply();

        expect(scope.loaded).toBe(true);
    });

    it('loaded is true when VoteService returns results', function () {

        getPollPromise.resolve(getPollResponse);
        voteGetResultsPromise.resolve({ data: null });
        scope.$apply();

        expect(scope.loaded).toBe(true);
    });

    it('Redirects to vote page on election polls', function () {

        getPollPromise.resolve(getElectionPollResponse);
        voteGetResultsPromise.reject(errors.IncorrectPollOrder);

        scope.$apply();

        expect(mockRoutingService.navigateToVotePage).toHaveBeenCalled();

    });


    describe('GVA Expired Callback', function () {

        it('Sets hasExpired to true', function () {

            scope.hasExpired = false;

            scope.gvaExpiredCallback();

            expect(scope.hasExpired).toBe(true);
        });

    });
});