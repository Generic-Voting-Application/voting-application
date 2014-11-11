require(['jquery', 'knockout'], function ($, ko) {
    function HomeViewModel() {
        var self = this;

        self.sessions = ko.observableArray();

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
                    localStorage["userId"] = userId;
                    $('#loginForm').addClass("has-success");
                    window.location = "vote?session=" + self.sessionId;
                },

                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginForm').addClass("has-error");
                        $('#usernameWarnMessage').show();
                    }
                }
            });
        }

        self.submitSession = function () {
            self.sessionId = $("#session-select").val();
            window.location = "?session=" + self.sessionId;
        }

        self.createSession = function () {
            $.ajax({
                type: 'POST',
                url: '/api/session',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: $("#session-create").val()
                }),

                success: function (data) {
                    self.sessionId = data;
                    window.location = "?session=" + self.sessionId;
                }
            })
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

        $(document).ready(function () {
            self.sessionId = getSessionId();
            self.userId = localStorage["userId"];

            if (!self.sessionId) {
                self.allSessions();
                $("#login-box").hide();
                $("#sessions").show();
            }
            else if (!self.userId) {
                $("#sessions").hide();
                $("#login-box").show();
            } else {
                window.location = "vote/?session=" + self.sessionId;
            }
        });
    }

    ko.applyBindings(new HomeViewModel());

});
