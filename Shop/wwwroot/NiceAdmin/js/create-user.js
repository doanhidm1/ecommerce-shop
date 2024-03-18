$.validator.addMethod("phoneUS", function (phone_number, element) {
    phone_number = phone_number.replace(/\s+/g, "");
    return this.optional(element) || phone_number.match(/^0\d{9}$/);
}, "Please enter a valid 10-digit phone number.");

// RegularExpression(@"^[a-zA-Z0-9_-]+$"
$.validator.addMethod("username", function (value, element) {
    return this.optional(element) || /^[a-zA-Z0-9_-]+$/.test(value);
}, "User name can only contain letters, numbers, underscores, and hyphens.");

$.validator.addMethod("password", function (value, element) {
    return this.optional(element) || /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$/.test(value);
}, "Password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and be at least 6 characters long.");

$('#create-user-form').validate({
    rules: {
        UserName: {
            required: true,
            username: true
        },
        Email: {
            required: true,
            email: true
        },
        Password: {
            required: true,
            password: true
        },
        ConfirmPassword: {
            required: true,
            equalTo: "#Password"
        },
        Avatar: {
            required: true
        },
        Roles: {
            required: true
        },
        PhoneNumber: {
            phoneUS: true,
        }
    },
    messages: {
        UserName: {
            required: "User name is required",
            username: "User name can only contain letters, numbers, underscores, and hyphens.",
        },
        Email: {
            required: "Email is required",
            email: "Email is invalid"
        },
        Password: {
            required: "Password is required",
            password: "Password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and be at least 6 characters long."
        },
        ConfirmPassword: {
            required: "Confirm password is required",
            equalTo: "Confirm password must be the same as password"
        },
        Avatar: {
            required: "Avatar is required"
        },
        Roles: {
            required: "Select at least one role"
        },
        PhoneNumber: {
            phoneUS: "Phone number is invalid"
        }
    }
});

function updateSubmitButtonState() {
    var isValid = $('#create-user-form').valid();
    $('#btn-create-user').prop('disabled', !isValid);
}

// Initial state check
// updateSubmitButtonState();

// Update button state whenever the form changes
$('#create-user-form').on('input change', function () {
    updateSubmitButtonState();
});

$('body').on('click', '#btn-create-user', function (e) {
    e.preventDefault();
    if ($('#create-user-form').valid()) {
        var form = $('#create-user-form');
        form.submit();
    }
});