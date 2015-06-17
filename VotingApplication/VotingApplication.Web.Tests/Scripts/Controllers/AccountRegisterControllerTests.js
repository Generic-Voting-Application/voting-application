'use strict';

describe('AccountRegisterController', function () {

    beforeEach(module('GVA.Common'));

    var rootScope;
    var scope;
    var mockAccountService;

    var registerPromise;

    beforeEach(inject(function ($rootScope, $controller, $q) {

        rootScope = $rootScope;
        rootScope.error = {};

        scope = $rootScope.$new();
        scope.closeThisDialog = function () { };
        scope.ngDialogData = {};

        registerPromise = $q.defer();

        mockAccountService = {
            register: function () { }
        };

        spyOn(mockAccountService, 'register').and.callFake(function () { return registerPromise.promise; });
        spyOn(scope, 'closeThisDialog').and.callThrough();

        $controller('AccountRegisterController', { $scope: scope, AccountService: mockAccountService });
    }));


    describe('Register Account', function () {

        it('Calls the account service to register', function () {
            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };


            registerPromise.resolve();

            scope.register(form.email, form.password);


            scope.$apply();
            expect(mockAccountService.register).toHaveBeenCalled();
        });

        it('Given a successful register call, closes the dialog', function () {

            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };


            registerPromise.resolve();

            scope.register(form.email, form.password);


            scope.$apply();
            expect(scope.closeThisDialog).toHaveBeenCalled();
        });

        it('Given an unsuccessful register call, displays the error message', function () {

            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };

            var errorMessage = 'Some Error message';
            rootScope.error.readableMessage = errorMessage;


            registerPromise.reject();

            scope.register(form.email, form.password);


            scope.$apply();
            expect(scope.displayError).toBe(errorMessage);
            expect(rootScope.error).toBeNull();
        });
    });

});