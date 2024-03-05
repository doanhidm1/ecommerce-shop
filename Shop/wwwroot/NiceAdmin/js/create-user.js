$('#create-user-form').validate({
    rules: {
        UserName: {
            required: true
        },
        Email: {
            required: true,
            email: true
        },
        Password: {
            required: true
        },
        ConfirmPassword: {
            required: true,
            equalTo: "#Password"
        },
        Avatar: {
            required: true
        }
    },
    messages: {
        UserName: {
            required: "User name is required"
        },
        Email: {
            required: "Email is required",
            email: "Email is invalid"
        },
        Password: {
            required: "Password is required"
        },
        ConfirmPassword: {
            required: "Confirm password is required",
            equalTo: "Confirm password must be the same as password"
        },
        Avatar: {
            required: "Avatar is required"
        }
    }
});

function save(e) {
    e.preventDefault()
    if ($('#create-user-form').valid()) {
        var a = $('#create-user-form');
        a.submit();
    }
};

$('body').on('click', '#btn-create-user', (e) => save(e));