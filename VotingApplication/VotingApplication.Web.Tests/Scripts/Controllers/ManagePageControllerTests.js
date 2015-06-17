'use strict';

describe('Manage Page Controller', function () {

    beforeEach(module('GVA.Manage'));

    var scope;

    var mockManageService;

    var manageUpdateQuestionPromise;
    var manageGetPollPromise;

    beforeEach(inject(function ($rootScope, $q, $controller, $routeParams) {

        scope = $rootScope.$new();
        scope.poll = {};

        mockManageService = {
            getPoll: function () { },
            getVisited: function () { },
            setVisited: function () { },
            updateQuestion: function () { }
        };
        manageGetPollPromise = $q.defer();
        manageUpdateQuestionPromise = $q.defer();
        spyOn(mockManageService, 'getPoll').and.callFake(function () { return manageGetPollPromise.promise; });
        spyOn(mockManageService, 'updateQuestion').and.callFake(function () { return manageUpdateQuestionPromise.promise; });

        $routeParams.manageId = 'de60a93b-d701-432c-9443-7f025ab22ce1';

        $controller('ManagePageController', { $scope: scope, ManageService: mockManageService });
    }));

    it('Loads Poll from service', function () {



        expect(mockManageService.getPoll).toHaveBeenCalled();
    });

    it('Sets poll data', function () {

        var pollData = {
            Name: 'Shall we play a game?'
        };

        manageGetPollPromise.resolve(pollData);

        scope.$apply();
        expect(scope.poll).toEqual(pollData);
    });

    it('Sets Question from poll name', function () {
        var question = 'What shall we do today, Brian?';
        var pollData = {
            Name: question
        };

        manageGetPollPromise.resolve(pollData);

        scope.$apply();
        expect(scope.Question).toEqual(question);
    });

    describe('Update Question', function () {

        it('Sets poll name when service call succeeds', function () {
            var newQuestion = 'What do you want to eat?';
            scope.Question = newQuestion;

            manageUpdateQuestionPromise.resolve();
            scope.updateQuestion();


            scope.$apply();
            expect(scope.poll.Name).toEqual(newQuestion);
        });

        it('Resets question when service call fails', function () {
            var originalQuestion = 'Where shall we go for lunch?';
            scope.poll.Name = originalQuestion;


            var newQuestion = 'What do you want to eat?';
            scope.Question = newQuestion;


            manageUpdateQuestionPromise.reject();
            scope.updateQuestion();


            scope.$apply();
            expect(scope.Question).toEqual(originalQuestion);
        });

    });

    describe('Discard Name Changes', function () {

        it('Reloads the poll from the service', function () {
            scope.poll.Name = 'What shall we do then?';

            var pollData = {
                Name: 'Shall we play a game?'
            };

            manageGetPollPromise.resolve(pollData);

            scope.discardNameChanges();


            scope.$apply();
            expect(scope.poll).toEqual(pollData);
        });

    });
});