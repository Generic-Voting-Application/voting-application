'use strict';

describe('BasicVoteController', function () {

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

        $controller('BasicVoteController', { $scope: scope, ngDialog: mockngDialog });
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

        it('Returns single vote', function () {
            var option = {
                optionId: 35,
                voteValue: 1
            };

            var votes = voteCallback(option);

            expect(votes.length).toBe(1);
        });

        it('VoteValue is always 1', function () {
            var option = {
                optionId: 35,
                voteValue: 56
            };

            var votes = voteCallback(option);

            expect(votes[0].VoteValue).toBe(1);
        });
    });
});