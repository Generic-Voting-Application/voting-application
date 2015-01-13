define(['jquery', 'knockout'], function ($, ko) {
    return function SigninViewModel() {
        var self = this;

        self.username = ko.observable("");
        self.password = ko.observable("");

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
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),

                success: function (data) {
                    // Registration doesn't auto-signin, we need to manually do this too
                    doSignin(self.newUsername(), self.newPassword());
                },

                error: function (data) {
                    $("#duplicate-username").show();
                }
            });
        }

        var doSignin = function (username, password) {

            var data = {
                grant_type: 'password',
                username: username,
                password: password
            };

            $.ajax({
                type: 'POST',
                url: '/Token',
                contentType: 'application/x-www-form-url-encoded; charset=utf-8',
                data: data,

                success: function (data) {
                    $("#bad-credentials").hide();
                    sessionStorage.setItem('creator_token', data.access_token);
                    window.location.href = "/Create/Index";
                },

                error: function (data) {
                    $("#bad-credentials").show();
                }
            });
        }

        self.signin = function () {
            doSignin(self.username(), self.password());
        }

        ko.applyBindings(this);
    }
});
