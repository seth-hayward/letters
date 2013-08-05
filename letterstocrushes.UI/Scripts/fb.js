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

    function load_dropdown(response) {

        document.getElementById("frands").options.length = 0;

        var oHandler = $('#frands').msDropDown().data("dd");
        if (oHandler) {
            oHandler.destroy();
            console.log(oHandler);
        }

        for (var i = 0; i < response.data.length; i++) {
            $("#frands").append('<option value="' + response.data[i].uid + '" data-image="' + response.data[i].pic_square + '">' + response.data[i].name + '</option>');
        }

        $("#frands").msDropdown({
            visibleRows: 5,
            on: {
                change: function (data, ui) {
                    $("input[name='toFacebookUID']").val(data.value);
                    $("#fbSend").show();
                }
            }
        });

        // visible row hack...
        $("#frands").msDropdown().data("dd").open();
        $("#frands").msDropdown().data("dd").close();


        $("input[name='toFacebookUID']").val($('#frands').val());

        $("#facebook").fadeIn('fast');

    }

    $("#fbSend").on("click", function () {
        alert("to: " + $("input[name='toFacebookUID']").val() + ", from: " + $("input[name='fromFacebookUID']").val());
    });

    // Check the result of the user status and display login button if necessary
    function checkLoginStatus(response) {
        if (response && response.status == 'connected') {

            // hide login button, they don't need to see it if they're
            // logged in
            $("#fb_login_button").hide();

            // Now Personalize the User Experience
            console.log('Access Token: ' + response.authResponse.accessToken);
            var looking_for = "";
            var fql = "SELECT uid, name, pic_square, sex FROM user WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) ";

            FB.api('/me', function (fql_response) {

                console.log(fql_response);
                $("input[name='fromFacebookUID']").val(fql_response.id);

                if (fql_response.gender === 'male') {
                    $("#show_women").prop('checked', true);
                    looking_for = " AND sex = 'female'";
                    fql = fql + looking_for;
                }

                if (fql_response.gender === 'female') {
                    $("#show_men").prop('checked', true);
                    looking_for = " AND sex = 'male'";
                    fql = fql + looking_for;
                }

                $(":radio[name='show_gender']").change(function () {
                    var show_fql = "SELECT uid, name, pic_square, sex FROM user WHERE uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) ";
                    if ($('input[name=show_gender]:checked').val() == "men") {
                        show_fql = show_fql + " AND sex = 'male'";
                    } else {
                        show_fql = show_fql + " AND sex = 'female'";
                    }

                    FB.api('/fql', 'GET', { q: show_fql }, function (response) {
                        if (response && response.data) {
                            load_dropdown(response);
                        }
                    });
                });

                FB.api('/fql', 'GET', { q: fql }, function (response) {
                    if (response && response.data) {
                        load_dropdown(response);
                    }
                });


            });

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
