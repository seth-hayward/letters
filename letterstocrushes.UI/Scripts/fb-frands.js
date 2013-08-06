// Additional JS functions here
var oldCB = window.fbAsyncInit;
window.fbAsyncInit = function () {
    if (typeof oldCB === 'function') {
        oldCB();
    }
    //Do Something else here

    // Check if the current user is logged in and has authorized the app
    FB.getLoginStatus(checkLoginStatusAndPopulate);

    // Login in the current user via Facebook and ask for email permission
    function authUser() {
        FB.login(checkLoginStatus, { scope: 'email' });
    }

    function load_dropdown(response) {

        $("#frands").empty();

        for (var i = 0; i < response.data.length; i++) {
            $("#frands").append('<option value="' + response.data[i].uid + '" data-image="' + response.data[i].pic_square + '">' + response.data[i].name + '</option>');
        }

        $("#frands").msDropdown({
            visibleRows: 5,
            rowHeight: 60,
            on: {
                change: function (data, ui) {
                    $("input[name='toFacebookUID']").val(data.value);
                    $("#fbSend").show();
                }
            }
        });

        $("input[name='toFacebookUID']").val($('#frands').val());

    }

    $("#songs").on('click', function () {
        $("#facebook").show();
    });

    $("#fbSend").on("click", function () {
        var toFacebookUID =$("input[name='toFacebookUID']").val();
        var fromFacebookUID = $("input[name='fromFacebookUID']").val();

        $.post("@Url.Content("~/Home/facebookLetter")", {
            "id": letter_id,
            "toFacebookUID": toFacebookUID,
            "fromFacebookUID": fromFacebookUID
        },
        function (data) {
            alert("to: " + toFacebookUID + ", from: " + fromFacebookUID + " - " + data.Message);
        }, "json").fail(function () {
            alert("Unable to save the facebook ID's. Sorry, please report this to seth at letters.to.crushes@gmail.com.");
        });

    });

    // Check the result of the user status and display login button if necessary
    function checkLoginStatusAndPopulate(response) {
        if (response && response.status == 'connected') {

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

                            // clear list
                            document.getElementById("frands").options.length = 0;

                            var oHandler = $('#frands').msDropDown().data("dd");
                            if (oHandler) {
                                oHandler.destroy();
                            }

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