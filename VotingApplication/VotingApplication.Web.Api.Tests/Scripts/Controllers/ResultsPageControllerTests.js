'use strict';

describe('Results Page Controller', function () {

    beforeEach(module('GVA.Voting'));

    var scope;

    var mockVoteService;
    var mockPollService;

    var voteGetResultsPromise;
    var getPollPromise;

    beforeEach(function () {
        jasmine.clock().install();
    });

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        mockVoteService = {
            refreshLastChecked: function () { },
            getResults: function () { }
        };

        voteGetResultsPromise = $q.defer();
        spyOn(mockVoteService, 'getResults').and.callFake(function () { return voteGetResultsPromise.promise; });

        mockPollService = {
            getPoll: function () { }
        };
        getPollPromise = $q.defer();
        spyOn(mockPollService, 'getPoll').and.callFake(function () { return getPollPromise.promise; });

        $controller('ResultsPageController', { $scope: scope, VoteService: mockVoteService, PollService: mockPollService });
    }));

    afterEach(function () {
        jasmine.clock().uninstall();
    });

    it('Calls vote service to get results', function () {

        expect(mockVoteService.getResults).toHaveBeenCalled();
    });

    it('Calls vote service to get results after 3 seconds', function () {

        mockVoteService.getResults.calls.reset();

        expect(mockVoteService.getResults).not.toHaveBeenCalled();

        jasmine.clock().tick(3000);

        expect(mockVoteService.getResults).toHaveBeenCalled();
    });

    it('Removes the timer if the vote service returns an error code (400+) from get results', function () {

        var response = { data: {}, status: 404 };

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

        voteGetResultsPromise.resolve({});

        scope.$apply();
        jasmine.clock().tick(3000);

        mockVoteService.getResults.calls.reset();
        jasmine.clock().tick(3000);


        expect(mockVoteService.getResults).toHaveBeenCalled();

    });

    it('Sets voteCount to be number of votes cast', function () {
        var resultData = {
            Votes: [{}, {}, {}]
        };

        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.voteCount).toBe(3);
    });

    it('Sets Winner to the single winning option', function () {
        var resultData = {
            Winners: [{ Name: 'Winning option' }]
        };

        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.winner).toBe('Winning option');
    });

    it('Sets Winner to the combined list of all winners', function () {
        var resultData = {
            Winners: [{ Name: 'Winning option' }, { Name: 'Another Winning option' }]
        };

        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.winner).toBe('Winning option, Another Winning option');
    });

    it('Sets plural to empty if there is a single winner', function () {
        var resultData = {
            Winners: [{ Name: 'Winning option' }]
        };

        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.plural).toBe('');
    });

    it('Sets plural if there is more than one winner', function () {
        var resultData = {
            Winners: [{ Name: 'Winning option' }, { Name: 'Another Winning option' }]
        };

        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.plural).toBe('s (Draw)');
    });

    it('Sets chart data if there are Results', function () {
        var resultData = {
            Results: [
            {
                Choice: { Name: 'Choice 1' },
                Sum: 1,
                Voters: [{ Name: 'Bob', Value: 1 }]
            },
            {
                Choice: { Name: 'Choice 2' },
                Sum: 3,
                Voters: [{ Name: 'Bob', Value: 2 }, { Name: 'Derek', Value: 1 }]
            }]
        };
        var response = { data: resultData };

        voteGetResultsPromise.resolve(response);

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

        scope.voteCount = 5;
        scope.winner = 'A winner is you!';
        scope.plural = '';
        scope.chartData = chartData;

        scope.$apply();
        jasmine.clock().tick(3000);


        expect(scope.voteCount).toBe(5);
        expect(scope.winner).toBe('A winner is you!');
        expect(scope.plural).toBe('');
        expect(scope.chartData).toBe(chartData);
    });

    it('Calls poll service to get poll expiry', function () {

        expect(mockPollService.getPoll).toHaveBeenCalled();
    });

    it('Leaves hasExpired false if poll never expires', function () {

        var pollData = {
            ExpiryDate: undefined
        };

        mockPollService.getPoll.and.callFake(function (polllId, callback) { return callback(pollData); });

        scope.$apply();
        expect(scope.hasExpired).toBe(false);
    });

    it('Leaves hasExpired false if expiry date is after now', function () {

        var baseTime = new Date(2015, 1, 1);
        jasmine.clock().mockDate(baseTime);
        
        var pollData = {
            data:
                {
                    ExpiryDateUtc: new Date(2015, 2, 1)
                }
        };

        mockPollService.getPoll.and.callFake(function (polllId, callback) { return callback(pollData); });

        scope.$apply();
        expect(scope.hasExpired).toBe(false);
    });

    it('Sets hasExpired to true if expiry date is in the past', function () {

        var baseTime = new Date(2015, 1, 1);
        jasmine.clock().mockDate(baseTime);

        var pollData = {
            data:
                {
                    ExpiryDateUtc: new Date(2014, 12, 25)
                }
        };

        getPollPromise.resolve(pollData);

        scope.$apply();
        expect(scope.hasExpired).toBe(true);
    });

    describe('GVA Expired Callback', function () {

        it('Sets hasExpired to true', function () {

            scope.hasExpired = false;

            scope.gvaExpiredCallback();

            expect(scope.hasExpired).toBe(true);
        });

    });
});