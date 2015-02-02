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


        it("populateTemplates without Signed In expect Do Nothing", function (done) {
            // arrange
            var polls = [
                { UUID: "poll-1", Name: "Poll 1", CreatedDate: "2015-01-23T14:00:00.000Z" }
            ];

            mockjax({
                type: "GET", url: "/api/poll",
                responseText: polls
            });

            spyOn(target, 'isSignedIn').and.returnValue(false);

            // act
            target.populateTemplates();

            // assert
            setTimeout(function () {
                expect(target.templates()).toEqual([]);
                done();
            }, 10);
        });


        it("populateTemplates with Signed In expect Get Previous Polls Only", function (done) {
            // arrange
            var polls = [
                { UUID: "poll-1", Name: "Poll 1", CreatedDate: "2015-01-23T14:00:00.000Z" },
                { UUID: "poll-2", Name: "Poll 2", CreatedDate: "2014-12-25T15:00:00.000Z" },
                { UUID: "poll-3", Name: "Poll 3", CreatedDate: "2015-02-15T09:00:00.000Z" }
            ];

            mockjax({
                type: "GET", url: "/api/poll",
                responseText: polls
            });

            target.pollId("poll-3");
            spyOn(target, 'isSignedIn').and.returnValue(true);

            // act
            target.populateTemplates();

            // assert
            setTimeout(function () {
                expect(target.templates()).toEqual([
                    { UUID: "poll-1", Name: "Poll 1 (Fri Jan 23 2015)" },
                    { UUID: "poll-2", Name: "Poll 2 (Thu Dec 25 2014)" }
                ]);
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


        it("cloneOptions expect Clone Options From Other Poll", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/poll-423/option",
                responseText: ["option-423-1", "option-423-2"]
            });

            spyOn(target, "saveOptions");
            target.templateId("poll-423");

            // act
            target.cloneOptions();

            // assert
            setTimeout(function () {
                expect(target.saveOptions).toHaveBeenCalledWith(["option-423-1", "option-423-2"]);
                done();
            }, 10);
        });

        it("cloneOptions handles server error", function (done) {
            // arrange
            mockjax({
                type: "GET", url: "/api/poll/poll-423/option",
                status: 500, responseText: {}
            });

            spyOn(target, "saveOptions");
            target.templateId("poll-423");

            // act
            target.cloneOptions();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.saveOptions.calls.count()).toEqual(0);
                done();
            }, 10);
        });


        it("saveOptions expect Apply All Option Data", function (done) {
            // arrange
            var options = [
                { Name: "Option 1", Description: "Desc 1", Info: "Info 1" },
                { Name: "Option 2", Description: "Desc 2", Info: "Info 2" }
            ];

            mockjax({
                type: "PUT", url: "/api/manage/101/option",
                data: JSON.stringify(options),
                responseText: [{ id: "opt-1" }, { id: "opt-2" }]
            });

            target.options([{ id: "option-101-1" }]);

            // act
            target.saveOptions(options);

            // assert
            setTimeout(function () {
                expect(target.options()).toEqual([{ id: "opt-1" }, { id: "opt-2" }]);
                done();
            }, 10);
        });

        it("saveOptions handles server error", function (done) {
            // arrange
            mockjax({
                type: "PUT", url: "/api/manage/101/option",
                status: 500, responseText: {}
            });

            target.options([{ id: "option-101-1" }]);

            // act
            target.saveOptions([{ Name: "Option 1" }]);

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.options()).toEqual([{ id: "option-101-1" }]);

                done();
            }, 10);
        });


        it("addOption with New Name expect Post Option Reset and Update", function (done) {
            // arrange
            var posted;
            mockjax({
                type: "POST", url: "/api/manage/101/option",
                data: JSON.stringify({ Name: "Name 1", Description: "Desc 1", Info: "Info 1" }),
                response: function () { posted = true; }
            });

            target.newName("Name 1");
            target.newDescription("Desc 1");
            target.newInfo("Info 1");

            spyOn(target, 'getPollDetails');

            // act
            target.addOption();

            // assert
            expect(target.newName()).toEqual("");
            expect(target.newDescription()).toEqual("");
            expect(target.newInfo()).toEqual("");

            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.getPollDetails).toHaveBeenCalled();

                done();
            }, 10);
        });

        it("addOption handles server error", function (done) {
            // arrange
            var posted;
            mockjax({
                type: "POST", url: "/api/manage/101/option",
                status: 500, responseText: {}
            });

            target.newName("Name 1");
            spyOn(target, 'getPollDetails');

            // act
            target.addOption();

            // assert
            setTimeout(function () {
                expect(Common.handleError).toHaveBeenCalled();
                expect(target.getPollDetails.calls.count()).toEqual(0);

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


        it("sendInvites splits email addresses and requests send email", function (done) {
            // arrange
            target.invitationText("one\ntwo\nthree");

            var sentAddresses = null;
            mockjax({
                type: "POST", url: "/api/manage/101/invitation",
                response: function (request) {
                    sentAddresses = JSON.parse(request.data);
                    return "";
                }
            });

            // act
            target.sendInvites();

            // assert
            setTimeout(function () {
                expect(sentAddresses.join()).toBe("one,two,three");
                expect(target.invitationText()).toBe("");
                done();
            }, 10);
        });

    });
});

