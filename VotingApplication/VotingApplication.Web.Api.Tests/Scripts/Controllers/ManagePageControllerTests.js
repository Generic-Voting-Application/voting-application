'use strict';

describe('Manage Page Controller', function () {

    beforeEach(module('GVA.Creation'));

    var scope;

    var mockManageService;

    var manageUpdateQuestionPromise;

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.poll = {};

        mockManageService = {
            getPoll: function () { },
            getVisited: function () { },
            setVisited: function () { },
            updateQuestion: function () { }
        };
        manageUpdateQuestionPromise = $q.defer();
        spyOn(mockManageService, 'updateQuestion').and.callFake(function () { return manageUpdateQuestionPromise.promise; });


        $controller('ManagePageController', { $scope: scope, ManageService: mockManageService });
    }));

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
});