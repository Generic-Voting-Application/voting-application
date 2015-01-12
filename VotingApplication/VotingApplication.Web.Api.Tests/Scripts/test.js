define(["Manage", "Common", "mockjax"], function (ManageViewModel, Common, mockjax) {
    describe("ManageViewModel", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target
            target = new ManageViewModel(101);

            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();

            // Spy on Common.handleError
            spyOn(Common, 'handleError');
        });
        //#endregion

        it("getPollDetails gets options for poll", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/manage/101",
                responseText: { Options: ["one", "two"] }
            });

            // act
            target.getPollDetails();

            // assert
            setTimeout(function () {
                expect(target.options().join()).toBe("one,two");
                done();
            }, 10);
        });

        it("getPollDetails handles server error", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/manage/101",
                status: 500, responseText: {}
            });

            // act
            target.getPollDetails();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                done();
            }, 10);
        });


        it("populateVotes gets votes for poll", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/manage/101/vote",
                responseText: ["one", "two"]
            });

            // act
            target.populateVotes();

            // assert
            setTimeout(function () {
                expect(target.votes().join()).toBe("one,two");
                done();
            }, 10);
        });


        it("resetVotes resets and repopulates votes", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/vote",
                responseText: {}
            });

            // Spy on the populate function
            spyOn(target, 'populateVotes');

            // act
            target.resetVotes();

            // assert
            setTimeout(function () {
                expect(target.populateVotes).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("resetVotes handles server error", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/vote",
                status: 500, responseText: {}
            });

            // Spy on the populate function
            spyOn(target, 'populateVotes');

            // act
            target.resetVotes();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.populateVotes.calls.count()).toEqual(0);
                done();
            }, 10);
        });


        it("deleteVote resets and repopulates votes", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/vote/36",
                responseText: {}
            });

            // Spy on the populate function
            spyOn(target, 'populateVotes');

            // act
            target.deleteVote({ Id: 36 });

            // assert
            setTimeout(function () {
                expect(target.populateVotes).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("deleteVote handles server error", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/vote/36",
                status: 500, responseText: {}
            });

            // Spy on the populate function
            spyOn(target, 'populateVotes');

            // act
            target.deleteVote({ Id: 36 });

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.populateVotes.calls.count()).toEqual(0);
                done();
            }, 10);
        });


        it("deleteOption resets and repopulates votes", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/option/52",
                responseText: {}
            });

            // Spy on the getPollDetails and populate function
            spyOn(target, 'getPollDetails');
            spyOn(target, 'populateVotes');

            // act
            target.deleteOption({ Id: 52 });

            // assert
            setTimeout(function () {
                expect(target.getPollDetails).toHaveBeenCalled();
                expect(target.populateVotes).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("deleteOption handles server error", function (done) {
            // arrange
            mockjax({
                type: "DELETE", url: "/api/manage/101/option/52",
                status: 500, responseText: {}
            });

            // Spy on the getPollDetails and populate function
            spyOn(target, 'getPollDetails');
            spyOn(target, 'populateVotes');

            // act
            target.deleteOption({ Id: 52 });

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.getPollDetails.calls.count()).toEqual(0);
                expect(target.populateVotes.calls.count()).toEqual(0);
                done();
            }, 10);
        });


    });
});

