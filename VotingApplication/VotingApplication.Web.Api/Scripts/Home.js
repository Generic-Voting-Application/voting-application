function HomeViewModel() {
    var self = this;

    self.options = ko.observableArray();

    $(document).ready(function () {
        $.ajax({
            type: 'GET',
            url: "/api/option",

            success: function (data) {
                data.forEach(function (option) {
                    self.options.push(option);
                });
            }
        });
    });
}

ko.applyBindings(new HomeViewModel());