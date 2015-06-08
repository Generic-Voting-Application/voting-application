'use strict';

describe('ManageService', function () {

    beforeEach(module('GVA.Manage'));

    var scope;
    var accountServiceMock;

    beforeEach(inject(function ($rootScope) {

        scope = $rootScope.$new();

        accountServiceMock = {
            account: {},
            registerAccountObserver: function () { }
        };

    }));

    describe('isLoggedIn', function () {

        it('false for undefined AccountService account', inject(function ($controller) {

            accountServiceMock.account = undefined;

            $controller('HomepageController', { $scope: scope, AccountService: accountServiceMock });

            expect(scope.isLoggedIn).toBe(false);
        }));

        it('false for null AccountService account', inject(function ($controller) {

            accountServiceMock.account = null;

            $controller('HomepageController', { $scope: scope, AccountService: accountServiceMock });

            expect(scope.isLoggedIn).toBe(false);
        }));

        it('true for existing AccountService account', inject(function ($controller) {

            accountServiceMock.account = {};

            $controller('HomepageController', { $scope: scope, AccountService: accountServiceMock });

            expect(scope.isLoggedIn).toBe(true);
        }));

    });
});