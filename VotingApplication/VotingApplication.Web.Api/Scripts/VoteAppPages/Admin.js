function AdminViewModel() {
    var self = this;
    var sessionId = 0;

    self.votes = ko.observableArray();
    self.sessions = ko.observableArray();
    self.options = ko.observableArray();
    self.selectedDeleteOptionId = null;

    self.resetVotes = function () {
        $.ajax({
            type: 'DELETE',
            url: "/api/session/" + sessionId + "/vote",

            success: function () {
                $("#reset-votes").attr('disabled', 'disabled');
                $("#reset-votes").text("Votes were reset");
                self.populateVotes();
            }
        });
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

    self.deleteVote = function (data, event) {
        $.ajax({
            type: 'DELETE',
            url: '/api/session/' + sessionId + '/vote?id=' + data.Id,
            contentType: 'application/json',

            success: function () {
                self.populateVotes();
            }
        });
    }

    self.populateVotes = function () {
        $.ajax({
            type: 'GET',
            url: "/api/session/" + sessionId + "/vote",

            success: function (data) {
                //Replace contents of self.votes with 'data'
                self.votes(data);
            }
        });
    }

    self.submitSession = function () {
        sessionId = $("#session-select").val();
        window.location = "?session=" + sessionId;
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

    self.populateOptions = function () {
        $.ajax({
            type: 'GET',
            url: "/api/option",

            success: function (data) {
                self.options(data);
            }
        });
    }

    self.deleteOption = function (data, event) {
        $.ajax({
            type: 'DELETE',
            url: '/api/option?id=' + data.Id,
            contentType: 'application/json',

            success: function () {
                self.populateOptions();
            }
        });
    }

    $(document).ready(function () {
        var windowArgs = getJsonFromUrl();

        if (!windowArgs['session']) {
            self.allSessions();
            $("#admin-panel").hide();
            $("#sessions").show();
            return;
        }

        sessionId = windowArgs['session'];
        $("#HomeLink").attr('href', '/?session=' + sessionId);
        $("#ResultLink").attr('href', '/Result?session=' + sessionId);
        $("#sessions").hide();
        $("#admin-panel").show();

        self.populateVotes();
        self.populateOptions();


        $("#reset-votes").click(self.resetVotes);
    });
}

ko.applyBindings(new AdminViewModel());
