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

    it('only sends emails once when inviting then saving', function () {
        // Arrange
        scope.pendingUsers = [{ Email: 'test@test.com', EmailSent: false }];
        scope.invitedUsers = [];

        // Act
        scope.sendInvitations();
        scope.saveChanges();

        // Assert
        expect(manageServiceMock.sendInvitations).toHaveBeenCalled();
        expect(manageServiceMock.sendInvitations.calls.count()).toBe(1);
    });
});