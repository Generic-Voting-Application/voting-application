define(["Create", "Common", "mockjax"], function (CreateViewModel, Common, mockjax) {
    describe("CreateViewModel", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target
            target = new CreateViewModel();

            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();

            // Spy on the navigation method
            spyOn(target, 'navigateToManage');
            // Spy on Common.handleError
            spyOn(Common, 'handleError');
        });
        //#endregion

        it("createPoll with Invalid expect Do nothing", function (done) {
            // arrange
            spyOn(target, 'validateForm').and.returnValue(false);
            var posted = false;
            mockjax({
                type: "POST", url: "/api/poll",
                responseText: { ManageID: 303 },
                response: function () { posted = true; }
            });

            // act
            target.createPoll();

            // assert
            setTimeout(function () {
                expect(target.validateForm).toHaveBeenCalled();
                expect(posted).toBe(false);
                expect(target.navigateToManage.calls.count()).toEqual(0);
                done();
            }, 10);
        });

        it("createPoll with Valid expect Create new poll and navigates to Manage page", function (done) {
            // arrange
            spyOn(target, 'validateForm').and.returnValue(true);
            mockjax({
                type: "POST", url: "/api/poll",
                responseText: { ManageID: 303 }
            });

            // act
            target.createPoll();

            // assert
            setTimeout(function () {
                expect(target.validateForm).toHaveBeenCalled();
                expect(target.navigateToManage).toHaveBeenCalledWith(303);
                done();
            }, 10);
        });

        it("createPoll with Data expect Create new poll with options", function (done) {
            // arrange
            spyOn(target, 'validateForm').and.returnValue(true);
            var posted;
            mockjax({
                type: "POST", url: "/api/poll",
                responseText: { ManageID: 303 },
                response: function (request) { posted = JSON.parse(request.data); }
            });

            var expiryDate = "2015-01-15T17:00:00.000Z";

            // Setup the data to post
            target.pollName("test-poll");
            target.creatorName("test-creator");
            target.creatorEmail("test-email");
            target.templateId(32);
            target.strategy(48);
            target.maxPoints(10);
            target.maxPerVote(5);
            target.inviteOnly(true);
            target.anonymousVoting(true);
            target.requireAuth(true);
            target.expiry(true);
            target.expiryDate(expiryDate);
            target.optionAdding(true);

            // act
            target.createPoll();

            // assert
            setTimeout(function () {
                // Check the posted options
                expect(posted.Name).toEqual("test-poll");
                expect(posted.Creator).toEqual("test-creator");
                expect(posted.Email).toEqual("test-email");
                expect(posted.TemplateId).toEqual(32);
                expect(posted.VotingStrategy).toEqual(48);
                expect(posted.MaxPoints).toEqual(10);
                expect(posted.MaxPerVote).toEqual(5);
                expect(posted.InviteOnly).toEqual(true);
                expect(posted.AnonymousVoting).toEqual(true);
                expect(posted.RequireAuth).toEqual(true);
                expect(posted.Expires).toEqual(true);
                expect(posted.ExpiryDate).toEqual(expiryDate);
                expect(posted.OptionAdding).toEqual(true);
                done();
            }, 10);
        });

        it("createPoll with Exception expect Handle server error", function (done) {
            // arrange
            spyOn(target, 'validateForm').and.returnValue(true);
            mockjax({
                type: "POST", url: "/api/poll",
                status: 500, responseText: {}
            });

            // act
            target.createPoll();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.navigateToManage.calls.count()).toEqual(0);
                done();
            }, 10);
        });


        it("populateTemplates expect Get templates list", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/template",
                responseText: [ "one", "two" ]
            });

            // act
            target.populateTemplates();

            // assert
            setTimeout(function () {
                expect(target.templates().join()).toEqual("one,two");
                done();
            }, 10);
        });

    });
});

