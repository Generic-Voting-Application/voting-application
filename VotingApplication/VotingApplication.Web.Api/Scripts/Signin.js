define(['jquery', 'knockout'], function ($, ko) {
    return function SigninViewModel() {
        var self = this;

        self.newUsername = ko.observable("");
        self.newPassword = ko.observable("");
        self.confirmPassword = ko.observable("");

        var checkPasswordFormat = function (password) {
            // Checks for password with uppercase, lowercase, digits and punctuation.
            // AccountController fails the password if any of these are missing
            var passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$&*]).*$/;
            return passwordRegex.test(password);
        }

        var checkEmailFormat = function (email) {
            var emailRegex = /[\w._%+-]+@\w+(\.\w+)+/;
            return emailRegex.test(email);
        }

        self.register = function () {
            $("#invalid-username").toggle(!checkEmailFormat(self.newUsername()));
            $("#password-length").toggle(self.newPassword().length < 6);
            $("#password-format").toggle(!checkPasswordFormat(self.newPassword()));
            $("#different-password").toggle(self.newPassword() != self.confirmPassword());
            $("#duplicate-username").hide();

            // Don't continue if any errors are still displayed
            var errors = $(".error-message");
            for (var i = 0; i < errors.length; i++) {
                if ($(errors[i]).css('display') != 'none') {
                    return;
                }
            }

            var data = {
                Email: self.newUsername(),
                Password: self.newPassword(),
                ConfirmPassword: self.confirmPassword()
            };

            $.ajax({
                type: 'POST',
                url: '/api/Account/Register',
                contentType: 'application/json; charser=utf-8',
                data: JSON.stringify(data),

                success: function (data) {
                },

                error: function (data) {
                    $("#duplicate-username").show();
                }
            });
        }

        ko.applyBindings(this);
    }
});
