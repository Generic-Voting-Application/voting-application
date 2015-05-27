'use strict';

describe('MultiVoteController', function () {

    beforeEach(module('GVA.Voting'));

    var scope;
    var mockngDialog;

    var voteCallback;

    beforeEach(inject(function ($rootScope, $controller) {

        scope = $rootScope.$new();
        scope.setVoteCallback = function (callback) { voteCallback = callback; };

        mockngDialog = {
            open: function () { }
        };

        spyOn(scope, 'setVoteCallback').and.callThrough();
        spyOn(mockngDialog, 'open').and.callThrough();

        $controller('MultiVoteController', { $scope: scope, ngDialog: mockngDialog });
    }));


    it('Registers GetVotes with parent scope', function () {

        expect(scope.setVoteCallback).toHaveBeenCalled();
    });

    describe('Add Choice', function () {

        var expectedTemplate = jasmine.objectContaining({
            template: '/Routes/AddChoiceDialog'
        });

        var expectedController = jasmine.objectContaining({
            controller: 'AddVoterChoiceDialogController'
        });

        it('Calls ngDialog to open the dialog', function () {

            scope.addChoice();

            expect(mockngDialog.open).toHaveBeenCalled();
        });

        it('Calls ngDialog with the correct template', function () {
            scope.addChoice();

            expect(mockngDialog.open).toHaveBeenCalledWith(expectedTemplate);
        });

        it('Calls ngDialog with the correct controller', function () {
            scope.addChoice();

            expect(mockngDialog.open).toHaveBeenCalledWith(expectedController);
        });

        it('Calls passes the pollId to ngDialog', function () {
            scope.pollId = 67;

            var expectedData = jasmine.objectContaining({
                data: { pollId: 67 }
            });

            scope.addChoice();

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
            expect(votes[0].ChoiceId).toBe(23);
        });

        it('VoteValue is always 1', function () {
            var option = [{
                Id: 35,
                voteValue: 56
            }];

            var votes = voteCallback(option);

            expect(votes[0].VoteValue).toBe(1);
        });
    });
});