'use strict';

describe('AccountServiceTests', function () {

    beforeEach(module('GVA.Common'));

    var accountService;
    var httpBackend;

    var formData = {
        email: 'user@example.com',
        password: 'letmein'
    };

    var tokenResponse = { access_token: 'A33CE1DB-67C1-47F2-832A-8CC3F8155814' };

    beforeEach(inject(function (AccountService, $httpBackend) {
        accountService = AccountService;
        httpBackend = $httpBackend;
    }));

    describe('Register Account And Login', function () {

        it('Calls api to register', function () {

            httpBackend
                .expect('POST', '/api/Account/Register')
                .respond(200);

            httpBackend
                .when('POST', '/Token')
                .respond(200, tokenResponse);

            accountService.register(formData.email, formData.password);

            httpBackend.flush();

            httpBackend.verifyNoOutstandingExpectation();
            httpBackend.verifyNoOutstandingRequest();

            // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
            expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
        });

        it('Given an unsuccessful register call, rejects promise', inject(function ($rootScope) {

            httpBackend
                .when('POST', '/api/Account/Register')
                .respond(400);

            var failed = false;

            accountService.register(formData.email, formData.password)
                .catch(function () { failed = true; });

            httpBackend.flush();

            $rootScope.$apply();
            expect(failed).toBe(true);
        }));

    });

    describe('Login', function () {

        it('Calls api to get a token', function () {
            httpBackend
                .expect('POST', '/Token')
                .respond(200, tokenResponse);

            accountService.login(formData.email, formData.password);

            httpBackend.flush();

            httpBackend.verifyNoOutstandingExpectation();
            httpBackend.verifyNoOutstandingRequest();

            // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
            expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
        });

        it('Given an unsuccessful token call, rejects promise', inject(function ($rootScope) {


            httpBackend
                .expect('POST', '/Token')
                .respond(400);

            var failed = false;

            accountService.login(formData.email, formData.password)
                .catch(function () { failed = true; });

            httpBackend.flush();

            $rootScope.$apply();
            expect(failed).toBe(true);
        }));

        it('Given a successful token call, saves the token in service account property', function () {
            httpBackend
                .when('POST', '/Token')
                .respond(200, tokenResponse);

            accountService.login(formData.email, formData.password);

            httpBackend.flush();

            expect(accountService.account).toBeDefined();
            expect(accountService.account.email).toBe('user@example.com');
            expect(accountService.account.token).toBe('A33CE1DB-67C1-47F2-832A-8CC3F8155814');
        });

        it('Given a successful token call, updates account and notifies observers', function () {
            httpBackend
                .when('POST', '/Token')
                .respond(200, tokenResponse);

            var observerNotified = false;
            accountService.registerAccountObserver(function () { observerNotified = true; });

            accountService.login(formData.email, formData.password);

            httpBackend.flush();

            expect(observerNotified).toBe(true);
        });
    });

    describe('Resend Email Confirmation', function () {

        it('Will call resend email confirmation with the correct email', function () {

            var email = 'test@test.com';
            var expectedUrl = '/api/Account/ResendConfirmation?email=' + email;

            httpBackend.expect('POST',
                                expectedUrl)
                       .respond(200);

            accountService.resendConfirmation(email);

            httpBackend.flush();

            // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
            expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
        });

    });

});