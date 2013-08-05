// Additional JS functions here
window.fbAsyncInit = function () {
    FB.init({
        appId: '575199149189657', // App ID
        channelUrl: '//WWW.crushsongs.COM/Content/channel.html', // Channel File
        status: true, // check login status
        cookie: true, // enable cookies to allow the server to access the session
        xfbml: true  // parse XFBML
    });

    // Check if the current user is logged in and has authorized the app
    FB.getLoginStatus(checkLoginStatus);

    // Login in the current user via Facebook and ask for email permission
    function authUser() {
        FB.login(checkLoginStatus, { scope: 'email' });
    }

    // Check the result of the user status and display login button if necessary
    function checkLoginStatus(response) {
        if (response && response.status == 'connected') {

            // hide login button, they don't need to see it if they're
            // logged in
            $("#fb_login_button").hide();
            // Now Personalize the User Experience
            console.log('Access Token: ' + response.authResponse.accessToken);
        } else {

            // do what now?
        }
    }

};

// Load the SDK asynchronously
(function (d) {
    var js, id = 'facebook-jssdk', ref = d.getElementsByTagName('script')[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement('script'); js.id = id; js.async = true;
    js.src = "//connect.facebook.net/en_US/all.js";
    ref.parentNode.insertBefore(js, ref);
}(document));
