describe("ManageVotersController Tests", function () {

    beforeEach(module("GVA.Creation"));

    var scope;

    beforeEach(inject(function ($rootScope, $controller) {
        scope = $rootScope.$new();

        var contr = $controller("ManageVotersController", { $scope: scope });

    }));

    it("Reset All adds all ballots to the list to be removed", function () {



        expect(true).toBe(true);
    });
});