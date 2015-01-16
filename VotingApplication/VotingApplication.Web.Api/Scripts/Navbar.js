define('Navbar', ['jquery'], function ($) {
    
    var Navbar = function () {
    }
    Navbar.signedIn = function () {
            return sessionStorage['creator_token'] != undefined
        };

    Navbar.signOut = function () {
            sessionStorage.removeItem('creator_token');

            // Hard reload to current page, preserving query parameters
            window.location.href = window.location.href;
    }

    $(document).ready(function () {
        if (Navbar.signedIn()) {
            $("#navbar-signin").hide();
            $("#navbar-signout").show();
        }
        else {
            $("#navbar-signin").show();
            $("#navbar-signout").hide();
        }
        
        $("#navbar-signout").click(Navbar.signOut);
    });
    
    return Navbar;
});
