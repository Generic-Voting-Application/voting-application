require(['jquery', 'knockout', 'bootstrap', 'Common'], function ($, ko, bs, Common) {
    function VoteViewModel() {
        var self = this;

        self.userId = ko.observable(0);

        self.toggleVoteResults = function () {

        }

        self.submitLogin = function () {

            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: $("#loginUsername").val()
                }),

                success: function (data) {
                    //Expire in 6 hours
                    var expiryTime = Date.now() + (6 * 60 * 60 * 1000)
                    self.userId(data);
                    localStorage["userId"] = JSON.stringify({ id: self.userId, expires: expiryTime });

                    $('#loginSection').collapse('hide');
                    $('#voteSection').collapse('show');
                },

                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 400) {
                        $('#loginSection').addClass("has-error");
                        $('#usernameWarnMessage').show();
                    }
                }
            });
        }
    }

    ko.applyBindings(new VoteViewModel());
});

