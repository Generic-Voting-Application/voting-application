'use strict';

describe('AccountLoginController', function () {

    beforeEach(module('GVA.Common'));

    var rootScope;
    var scope;
    var mockAccountService;

    var loginPromise;
    var forgotPasswordPromise;

    beforeEach(inject(function ($rootScope, $controller, $route, $q) {

        rootScope = $rootScope;
        rootScope.error = {};

        scope = $rootScope.$new();
        scope.closeThisDialog = function () { };

        loginPromise = $q.defer();
        forgotPasswordPromise = $q.defer();

        mockAccountService = {
            login: function () { },
            forgotPassword: function () { }
        };

        spyOn(mockAccountService, 'login').and.callFake(function () { return loginPromise.promise; });
        spyOn(mockAccountService, 'forgotPassword').and.callFake(function () { return forgotPasswordPromise.promise; });
        spyOn(scope, 'closeThisDialog').and.callThrough();

        $controller('AccountLoginController', { $scope: scope, AccountService: mockAccountService });
    }));


    describe('Login Account', function () {

        it('Calls the account service to login', function () {
            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };


            loginPromise.resolve();

            scope.loginAccount(form.email, form.password);


            scope.$apply();
            expect(mockAccountService.login).toHaveBeenCalled();
        });

        it('Given a successful login call, closes the dialog', function () {

            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };


            loginPromise.resolve();

            scope.loginAccount(form.email, form.password);


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


            loginPromise.reject();

            scope.loginAccount(form.email, form.password);


            scope.$apply();
            expect(scope.displayError).toBe(errorMessage);
        });

        it('Given an unsuccessful register call, removes the error message from the root scope', function () {

            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };

            var errorMessage = 'Some Error message';
            rootScope.error.readableMessage = errorMessage;


            loginPromise.reject();

            scope.loginAccount(form.email, form.password);


            scope.$apply();
            expect(rootScope.error).toBeNull();
        });

        it('Clears any errors before attempting to login', function () {

            scope.displayError = 'Some Error Message';

            var form = {
                email: 'user@example.com',
                password: 'p@ssword'
            };


            scope.loginAccount(form.email, form.password);


            scope.$apply();
            expect(scope.displayError).toBe(null);
        });
    });

    describe('Forgotten Password', function () {

        it('Calls the account service for a forgotten password', function () {
            var form = {

                email: 'user@example.com'
            };


            forgotPasswordPromise.resolve();

            scope.forgottenPassword(form);


            scope.$apply();
            expect(mockAccountService.forgotPassword).toHaveBeenCalled();
        });

        it('Displays an error message when no email is supplied', function () {

            var form = {};

            scope.forgottenPassword(form);


            scope.$apply();
            expect(scope.displayError).toBe('Please supply email address.');
        });

        it('Given a successful email reset call, closes the dialog', function () {

            var form = {
                email: 'user@example.com'
            };

            forgotPasswordPromise.resolve();


            scope.forgottenPassword(form);


            scope.$apply();
            expect(scope.closeThisDialog).toHaveBeenCalled();
        });

        it('Given an unsuccessful email reset call, displays the error message', function () {

            var form = {
                email: 'user@example.com'
            };

            var rejection = {
                Message: 'Some error message'
            };

            forgotPasswordPromise.reject(rejection);

            scope.forgottenPassword(form);


            scope.$apply();
            expect(scope.displayError).toBe('Some error message');
        });
    });

});