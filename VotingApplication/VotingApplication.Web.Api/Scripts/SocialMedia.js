define('SocialMedia', ['platform'], function () {

    return new function SocialMedia() {

        this.facebookLogin = function (callbackFn) {
            FB.login(function (response) {
                if (response.status === 'connected') {
                    FB.api('/me', function (content) {
                        var username = content.first_name + " " + content.last_name;
                        callbackFn(username);
                    })
                }
            });
        }

        this.googleLogin = function (callbackFn) {
            gapi.auth.signIn({
                'callback': function (authResult) {
                    //Login failed
                    if (authResult['status']['signed_in']) {
                        //Load the username and login to GVA
                        gapi.client.load('plus', 'v1', function () {
                            var request = gapi.client.plus.people.get({
                                'userId': 'me'
                            });
                            request.execute(function (resp) {
                                callbackFn(resp.displayName);
                            });
                        });
                    }
                }
            });
        };

    };
});

//#region Begin Facebook boilerplate

window.fbAsyncInit = function () {
    FB.init({
        appId: '333351380206896',
        xfbml: true,
        version: 'v2.2'
    });
};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

///#endregion End Facebook boilerplate
