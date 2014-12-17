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

    Common.getJsonFromUrl = function () {
        var query = location.search.substr(1);
        var result = {};
        query.split("&").forEach(function (part) {
            var item = part.split("=");
            result[item[0]] = decodeURIComponent(item[1]);
        });
        return result;
    };

    Common.getPollId = function () {
        var windowArgs = Common.getJsonFromUrl();
        var pollId = windowArgs['poll'];
        return pollId;
    };

    Common.getManageId = function () {
        var windowArgs = Common.getJsonFromUrl();
        var manageId = windowArgs['manage'];
        return manageId;
    };

    Common.getToken = function () {
        var windowArgs = Common.getJsonFromUrl();
        var token = windowArgs['token'] || Common.sessionItem("token");
        return token;
    }

    Common.currentUserId = function () {
        return Common.sessionItem("id");
    };

    Common.currentUserName = function () {
        return Common.sessionItem("userName");
    };

    Common.sessionItem = function (sessionKey) {
        var localUserJSON = localStorage["user"];

        if (localUserJSON) {
            var localUser = $.parseJSON(localUserJSON);
            if (localUser.expires < Date.now()) {
                localStorage.removeItem("user");
            }
            else {
                return localUser[sessionKey];
            }
        }

        return undefined;
    }

    Common.loginUser = function (userData, userName) {
        //Expire in 6 hours
        var expiryTime = Date.now() + (6 * 60 * 60 * 1000);
        localStorage["user"] = JSON.stringify({ id: userData.UserId, userName: userName, expires: expiryTime, token: userData.TokenGuid });
    };

    Common.keyIsEnter = function (key, callback) {
        if (key && key.keyCode == 13) {
            callback();
        }
    };

    return Common;
});