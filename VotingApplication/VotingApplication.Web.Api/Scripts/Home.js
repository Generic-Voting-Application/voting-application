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
            }
        });
    }

    $(document).ready(function () {
        // Get all options
        $.ajax({
            type: 'GET',
            url: "/api/option",

            success: function (data) {
                data.forEach(function (option) {
                    self.options.push(option);
                });
            }
        });

        // Do login
        $("#Name-submit").click(function () {
            $.ajax({
                type: 'PUT',
                url: '/api/user',
                contentType: 'application/json',
                data: JSON.stringify({
                    Name: $("#Name").val()
                }),

                success: function (data) {
                    userId = data;
                    $("#login-box").hide();
                    $("#vote-table").show();
                }
            });
        });
    });
}

ko.applyBindings(new HomeViewModel());