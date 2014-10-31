function HomeViewModel() {
    var self = this;
    var userId = 0;
    var sessionId = 0;

    self.options = ko.observableArray();
    self.sessions = ko.observableArray();

    // Submit a vote
    self.doVote = function (data, event) {
        $.ajax({
            type: 'PUT',
            url: '/api/user/'+userId+'/vote',
            contentType: 'application/json',
            data: JSON.stringify({
                OptionId: data.Id,
                SessionId: sessionId
            }),

            success: function (returnData) {
                var currentRow = event.currentTarget.parentElement.parentElement;
                $(currentRow).siblings().removeClass("success");
                $(currentRow).addClass("success");

                setTimeout(function () { window.location = "/Result?session=" + sessionId; }, 500);
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

    self.submitSession = function () {
        sessionId = $("#session-select").val();
        window.location = "?session=" + sessionId;
    }

    self.highlightPreviousVote = function () {
        $.ajax({
            type: 'GET',
            url: '/api/user/' + userId + '/session/' + sessionId + '/vote',
            contentType: 'application/json',

            success: function (data) {
                for (var index = 0; index < data.length; index++) {
                    var previousOptionId = data[index].OptionId;
                    var previousOption = self.options().filter(function (d) { return d.Id == previousOptionId }).pop();
                    var previousOptionRowIndex = self.options().indexOf(previousOption);
                    var matchingRow = $("#inner-vote-table > tbody > tr").eq(previousOptionRowIndex);
                    matchingRow.addClass("success");
                }
            }
        });
    }

    self.allSessions = function () {
        $.ajax({
            type: 'GET',
            url: '/api/session',

            success: function (data) {
                self.sessions(data);
            }
        })
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
        $.ajax({
            type: 'POST',
            url: '/api/option',
            contentType: 'application/json',

            data: JSON.stringify({
                Name: $("#newName").val(),
                Description: $("#newDescription").val(),
                Info: $("#newInfo").val()
            }),

            success: function () {
                self.allOptions();
                $("#newName").val("")
                $("#newDescription").val("")
                $("#newInfo").val("")
            }
        })
    }

    function getJsonFromUrl() {
        var query = location.search.substr(1);
        var result = {};
        query.split("&").forEach(function (part) {
            var item = part.split("=");
            result[item[0]] = decodeURIComponent(item[1]);
        });
        return result;
    }

    $(document).ready(function () {
        var windowArgs = getJsonFromUrl();

        if (!windowArgs['session']) {
            self.allSessions();
            $("#login-box").hide();
            $("#sessions").show();
        }
        else {
            sessionId = windowArgs['session'];
            $("#HomeLink").attr('href', '/?session=' + sessionId);
            $("#ResultLink").attr('href', '/Result?session=' + sessionId);
            $("#sessions").hide();
            $("#login-box").show();
        }

        self.allOptions();

        $("#Name-submit").click(self.submitLogin);
        //Submit on pressing return key
        $("#Name").keypress(function (event) { self.keyIsEnter(event, self.submitLogin); });

        //Add option on pressing return key
        $("#newOptionRow").keypress(function(event) { self.keyIsEnter(event, self.addOption); });
    });
}

ko.applyBindings(new HomeViewModel());
