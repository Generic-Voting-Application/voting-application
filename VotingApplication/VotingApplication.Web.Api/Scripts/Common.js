define(['jquery'], function () {

    // Checks if elements are collapsed
    (function () {

        $.fn.isCollapsed = function () {
            var $this = $(this).find('.accordion-body').filter(':visible');
            return $this ? !$this.parents().toArray().some(function (element) {
                return !$(element).hasClass('in');
            }) : false;
        };

        $.fn.collapseSection = function (collapse) {
            if (($(this).isCollapsed() && collapse == 'hide') || (!$(this).isCollapsed() && collapse == 'show')) {
                return;
            }

            $(this).find('.accordion-body').collapse(collapse);
        };
    })();

    function Common() {

    }

    Common.currentUserId = function (pollId) {
        return Common.sessionItem("id", pollId);
    };

    Common.currentUserName = function (pollId) {
        return Common.sessionItem("userName", pollId);
    };

    Common.sessionItem = function (sessionKey, pollId) {
        var localUserJSON = localStorage["user_" + pollId];

        if (localUserJSON) {
            var localUser = $.parseJSON(localUserJSON);
            if (localUser.expires < Date.now()) {
                localStorage.removeItem("user_" + pollId);
            }
            else {
                return localUser[sessionKey];
            }
        }

        return undefined;
    }

    Common.loginUser = function (userData, userName, pollId) {
        //Expire in 6 hours
        var expiryTime = Date.now() + (6 * 60 * 60 * 1000);
        localStorage["user_" + pollId] = JSON.stringify({ id: userData.UserId, userName: userName, expires: expiryTime, token: userData.TokenGuid });
    };

    Common.logoutUser = function (pollId) {
        localStorage.removeItem("user_" + pollId);
    }

    Common.handleError = function (error) {

        var friendlyText;

        switch (error.status) {
            case 404:
                friendlyText = 'Double check the poll ID and ensure that you have logged in correctly'
                break;
        }

        var newError = '<div class="alert alert-danger">' +
        '<a href="#" class="close" data-dismiss="alert">&times;</a>' +
        '<strong>' + error.statusText + ' </strong>' +
        JSON.parse(error.responseText).Message +
        (friendlyText ? '<br/>' + friendlyText : '') +
        '</div>';

        $('#errorArea').append(newError);
    }

    return Common;
});