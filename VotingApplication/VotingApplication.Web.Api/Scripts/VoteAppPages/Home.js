function HomeViewModel() {
    var self = this;
    var userId = 0;

    self.options = ko.observableArray();

    // Submit a vote
    self.doVote = function (data, event) {
        $.ajax({
            type: 'PUT',
            url: '/api/user/'+userId+'/vote',
            contentType: 'application/json',
            data: JSON.stringify({
                OptionId: data.Id
            }),

            success: function (returnData) {
                var currentRow = event.currentTarget.parentElement.parentElement;
                $(currentRow).siblings().removeClass("success");
                $(currentRow).addClass("success");

                setTimeout(function () { window.location = "/Result"; }, 500);
            }
        });
    }

    self.keyIsEnter = function (key, callback) {
        if (key && key.keyCode == 13)
        {
            callback();
        }
    }

    // Do login
    self.submitLogin = function () {
        $.ajax({
            type: 'PUT',
            url: '/api/user',
            contentType: 'application/json',
            data: JSON.stringify({
                Name: $("#Name").val()
            }),

            success: function (data) {
                userId = data;
                self.highlightPreviousVote();
                $("#login-box").hide();
                $("#vote-table").show();
            }
        });
    }

    self.highlightPreviousVote = function () {
        $.ajax({
            type: 'GET',
            url: '/api/user/' + userId + '/vote',
            contentType: 'application/json',

            success: function (data) {
                if (data.length > 0) {
                    var previousOptionId = data[0].OptionId;
                    var previousOption = self.options().filter(function (d) { return d.Id == previousOptionId }).pop();
                    var previousOptionRowIndex = self.options().indexOf(previousOption);
                    var matchingRow = $("#inner-vote-table > tbody > tr").eq(previousOptionRowIndex);
                    matchingRow.addClass("success");
                }
            }
        });
    }

    self.allOptions = function () {
        // Get all options
        $.ajax({
            type: 'GET',
            url: "/api/option",

            success: function (data) {
                self.options(data);
            }
        });
    }

    self.addOption = function () {
        //Don't submit without an entry in the name field
        if ($("#newName").val() == "") {
            return;
        }

        var newName = $("#newName").val();
        var newDescription = $("#newDescription").val()
        var newInfo = $("#newInfo").val()

        //Reset before posting, to prevent double posts.
        $("#newName").val("")
        $("#newDescription").val("")
        $("#newInfo").val("")

        $.ajax({
            type: 'POST',
            url: '/api/option',
            contentType: 'application/json',

            data: JSON.stringify({
                Name: newName,
                Description: newDescription,
                Info: newInfo
            }),

            success: function () {
                self.allOptions();
            }
        })
    }

    $(document).ready(function () {

        self.allOptions();

        $("#Name-submit").click(self.submitLogin);
        //Submit on pressing return key
        $("#Name").keypress(function (event) { self.keyIsEnter(event, self.submitLogin); });

        //Add option on pressing return key
        $("#newOptionRow").keypress(function(event) { self.keyIsEnter(event, self.addOption); });
    });
}

ko.applyBindings(new HomeViewModel());
