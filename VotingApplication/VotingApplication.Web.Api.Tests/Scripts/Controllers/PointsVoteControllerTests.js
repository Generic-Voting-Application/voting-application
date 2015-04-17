'use strict';

describe('PointsVoteController', function () {

    beforeEach(module('GVA.Voting'));

    var scope;
    var mockngDialog;

    var voteCallback;

    beforeEach(inject(function ($rootScope, $controller) {

        scope = $rootScope.$new();
        scope.setVoteCallback = function (callback) { voteCallback = callback; };

        scope.poll = { Options: [] };

        mockngDialog = {
            open: function () { }
        };

        spyOn(scope, 'setVoteCallback').and.callThrough();
        spyOn(mockngDialog, 'open').and.callThrough();

        $controller('PointsVoteController', { $scope: scope, ngDialog: mockngDialog });
    }));


    it('Registers GetVotes with parent scope', function () {

        expect(scope.setVoteCallback).toHaveBeenCalled();
    });

    describe('Add Option', function () {

        var expectedTemplate = jasmine.objectContaining({
            template: '/Routes/AddOptionDialog'
        });

        var expectedController = jasmine.objectContaining({
            controller: 'AddVoterOptionDialogController'
        });

        it('Calls ngDialog to open the dialog', function () {

            scope.addOption();

            expect(mockngDialog.open).toHaveBeenCalled();
        });

        it('Calls ngDialog with the correct template', function () {
            scope.addOption();

            expect(mockngDialog.open).toHaveBeenCalledWith(expectedTemplate);
        });

        it('Calls ngDialog with the correct controller', function () {
            scope.addOption();

            expect(mockngDialog.open).toHaveBeenCalledWith(expectedController);
        });

        it('Calls passes the pollId to ngDialog', function () {
            scope.pollId = 67;

            var expectedData = jasmine.objectContaining({
                data: { pollId: 67 }
            });

            scope.addOption();

            expect(mockngDialog.open).toHaveBeenCalledWith(expectedData);
        });
    });

    describe('VoteCallback called', function () {

        it('Returns correct number of votes', function () {
            var options = [
                {
                    Id: 35,
                    voteValue: 1
                },
                {
                    Id: 34,
                    voteValue: 1
                },
                {
                    Id: 23,
                    voteValue: 1
                }
            ];

            var votes = voteCallback(options);

            expect(votes.length).toBe(3);
        });

        it('Returns only votes voted for', function () {
            var options = [
                {
                    Id: 35,
                    voteValue: 0
                },
                {
                    Id: 34,
                    voteValue: 0
                },
                {
                    Id: 23,
                    voteValue: 1
                }
            ];

            var votes = voteCallback(options);

            expect(votes.length).toBe(1);
            expect(votes[0].OptionId).toBe(23);
        });

        it('VoteValue is set correctly', function () {
            var option = [{
                Id: 35,
                voteValue: 4
            }];

            var votes = voteCallback(option);

            expect(votes[0].VoteValue).toBe(4);
        });

    });

    describe('Increase Vote', function () {

        it('Adds one to the current vote value', function () {

            scope.poll.MaxPerVote = 7;

            var option = {
                voteValue: 0
            };

            scope.increaseVote(option);

            expect(option.voteValue).toBe(1);
        });

        it('Does not allow increase beyond the MaxPerVote', function () {

            scope.poll.MaxPerVote = 2;

            var option = {
                voteValue: 2
            };

            scope.increaseVote(option);

            expect(option.voteValue).toBe(2);
        });

    });

    describe('Decrease Vote', function () {

        it('Subtracts one to the current vote value', function () {

            var option = {
                voteValue: 2
            };

            scope.decreaseVote(option);

            expect(option.voteValue).toBe(1);
        });

        it('Does not allow decrease below zero', function () {

            var option = {
                voteValue: 0
            };

            scope.decreaseVote(option);

            expect(option.voteValue).toBe(0);
        });

    });

    describe('Add Points Disabled', function () {

        it('Returns true given an option that has the maximum votes already allocated to it', function () {

            scope.poll.MaxPerVote = 3;

            var option = {
                voteValue: 3
            };

            var isDisabled = scope.addPointsDisabled(option);


            expect(isDisabled).toBe(true);
        });

        it('Returns true when there are no more points to allocate', function () {

            scope.poll.MaxPerVote = 10;
            scope.unallocatedPoints = function () { return 0; };

            var option = {
                voteValue: 3
            };

            var isDisabled = scope.addPointsDisabled(option);


            expect(isDisabled).toBe(true);
        });

        it('Returns false given an option that does not have the maximum votes already allocated to it and votes available to be allocated', function () {

            scope.poll.MaxPerVote = 10;
            scope.unallocatedPoints = function () { return 5; };

            var option = {
                voteValue: 3
            };

            var isDisabled = scope.addPointsDisabled(option);


            expect(isDisabled).toBe(false);
        });
    });

    describe('Unallocated Points Percentage', function () {

        it('Returns 0 when there are no points to allocate', function () {

            scope.poll.MaxPoints = 10;
            scope.poll.Options = [{ voteValue: 10 }];

            var percentage = scope.unallocatedPointsPercentage();

            expect(percentage).toBe(0);

        });

        it('Returns 100 when there are no points allocated', function () {

            scope.poll.MaxPoints = 10;
            scope.poll.Options = [{ voteValue: 0 }];

            var percentage = scope.unallocatedPointsPercentage();

            expect(percentage).toBe(100);

        });

        it('Returns correct value when points are partially allocated', function () {

            scope.poll.MaxPoints = 10;
            scope.poll.Options = [{ voteValue: 3 }];

            var percentage = scope.unallocatedPointsPercentage();

            expect(percentage).toBe(70);

        });
    });

    describe('Unallocated Points', function () {

        it('Returns Max Points when no points allocated', function () {

            scope.poll.MaxPoints = 10;

            var points = scope.unallocatedPoints();

            expect(points).toBe(10);
        });

        it('Returns zero when all points allocated', function () {

            scope.poll.MaxPoints = 10;
            scope.poll.Options = [{ voteValue: 10 }];

            var points = scope.unallocatedPoints();

            expect(points).toBe(0);
        });

        it('Returns correct value when points are partially allocated', function () {

            scope.poll.MaxPoints = 10;
            scope.poll.Options = [{ voteValue: 7 }];

            var points = scope.unallocatedPoints();

            expect(points).toBe(3);
        });
    });

    describe('Add Points Disabled', function () {

        it('Returns true given an option with no points allocated', function () {
            var option = {
                voteValue: 0
            };

            var isDisabled = scope.subtractPointsDisabled(option);


            expect(isDisabled).toBe(true);
        });

        it('Returns false given an option with points allocated', function () {
            var option = {
                voteValue: 3
            };

            var isDisabled = scope.subtractPointsDisabled(option);


            expect(isDisabled).toBe(false);
        });
    });

});