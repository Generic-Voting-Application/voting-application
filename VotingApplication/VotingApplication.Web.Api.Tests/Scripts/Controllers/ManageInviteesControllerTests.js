'use strict';

describe('ManageInviteesController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;
    var manageServiceMock;
    
    var manageGetInvitationsPromise;
    var manageSendInvitationsPromise;

    var routingServiceMock;


    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        
        manageServiceMock = {
            getInvitations: function () { },
            sendInvitations: function() { }
        };

        manageGetInvitationsPromise = $q.defer();
        manageSendInvitationsPromise = $q.defer();
        spyOn(manageServiceMock, 'getInvitations').and.callFake(function () { return manageGetInvitationsPromise.promise; });
        spyOn(manageServiceMock, 'sendInvitations').and.callFake(function () { return manageSendInvitationsPromise.promise; });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');


        $controller('ManageInviteesController', { $scope: scope, ManageService: manageServiceMock, RoutingService: routingServiceMock });
    }));

    it('sends emails once when inviting', function () {
        // Arrange
        scope.pendingUsers = [{ Email: 'test@test.com', EmailSent: false }];
        scope.invitedUsers = [];
        scope.manageId = 'abc';
        scope.discard = function () { };

        // Act
        scope.sendInvitations();

        // Assert
        expect(manageServiceMock.sendInvitations.calls.count()).toBe(1);
        expect(manageServiceMock.sendInvitations.calls.first().args[1]).toEqual([{ Email: 'test@test.com', EmailSent: false, SendInvitation: true }]);
    });

    it('does not send emails when saving', function () {
        // Arrange
        scope.pendingUsers = [{ Email: 'test@test.com', EmailSent: false }];
        scope.invitedUsers = [];
        scope.manageId = 'abc';
        scope.discard = function () { };

        // Act
        scope.saveChanges();

        // Assert
        expect(manageServiceMock.sendInvitations.calls.count()).toBe(1);
        expect(manageServiceMock.sendInvitations.calls.first().args[1]).toEqual([{ Email: 'test@test.com', EmailSent: false, SendInvitation: false }]);
    });

    it('does not send emails when discarding changes', function () {
        // Arrange
        scope.pendingUsers = [{ Email: 'test@test.com', EmailSent: false }];
        scope.invitedUsers = [];
        scope.manageId = 'abc';
        scope.discard = function () { };

        // Act
        scope.discardChanges();

        // Assert
        expect(manageServiceMock.sendInvitations.calls.count()).toBe(0);
    });
});