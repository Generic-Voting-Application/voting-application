define('Navbar', ['jquery'], function ($) {
    var self = this;

    self.signedIn = function () {
        return sessionStorage['creator_token'] != undefined
    };

    self.signOut = function () {
        sessionStorage.removeItem('creator_token');

        // Hard reload to current page, preserving query parameters
        window.location.href = window.location.href;
    }

    $(document).ready(function () {
        if (self.signedIn()) {
            $("#navbar-signin").hide();
            $("#navbar-signout").show();
        }
        else {
            $("#navbar-signin").show();
            $("#navbar-signout").hide();
        }
        
        $("#navbar-signout").click(self.signOut);
    });
});