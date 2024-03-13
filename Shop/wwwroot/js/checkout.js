$.validator.addMethod("phoneUS", function (phone_number, element) {
    phone_number = phone_number.replace(/\s+/g, "");
    return this.optional(element) || phone_number.match(/^0\d{9}$/);
}, "Please enter a valid 10-digit phone number.");

$.validator.addMethod("zipcode", function (value, element) {
    return this.optional(element) || /^\d{5}$/.test(value);
}, "Please enter a valid zip code.");

$('#frm-customer-info').validate({
    rules: {
        FirstName: {
            required: true,
        },
        LastName: {
            required: true,
        },
        Street: {
            required: true,
        },
        City: {
            required: true,
        },
        Country: {
            required: true,
        },
        ZipCode: {
            required: true,
            number: true,
            zipcode: true,
        },
        PhoneNumber: {
            required: true,
            phoneUS: true,
        },
        Email: {
            required: true,
            email: true,
        },
        terms: {
            required: true,
        },
    },
    messages: {
        FirstName: {
            required: "Please enter your first name",
        },
        LastName: {
            required: "Please enter your last name",
        },
        Street: {
            required: "Please enter your street",
        },
        City: {
            required: "Please enter your city",
        },
        Country: {
            required: "Please enter your country",
        },
        ZipCode: {
            required: "Please enter your zip code",
            number: "Please enter a valid zip code",
        },
        PhoneNumber: {
            required: "Please enter your phone number",
            phoneUS: "Please enter a valid phone number",
        },
        Email: {
            required: "Please enter your email",
            email: "Please enter a valid email address",
        },
        terms: {
            required: "Please accept the terms and conditions",
        },
    }
});

function updatePlaceOrderButtonState() {
    var isValid = $('#frm-customer-info').valid();
    $('#btn-place-order').prop('disabled', !isValid);
}

function placeOrder(e) {
    e.preventDefault();

    if ($('#frm-customer-info').valid()) {
        // Additional checks or actions can be added here before submitting the form
        var form = $('#frm-customer-info');
        form.submit();
    }
}

// check if form exists
if ($('#frm-customer-info').length) {
    // Update button state whenever the form changes
    $('#frm-customer-info').on('input change', function () {
        updatePlaceOrderButtonState();
    });
}



function init() {

    var message = $('#check-out-message').text();
    var status = Number($('#check-out-status').text());
    if (message != '') {
        var messageType = status == 200 ? "success" : "error"
        $.notify(message, messageType);
    }
    var pm = $('#payment-method').text();
    if (pm != '') {
        // get all option elements
        var options = $('select[name="PaymentMethod"] option');
        // find the option with the value of the hidden field
        var selectedOption = options.filter(function () {
            return $(this).val() == pm;
        });
        // select the option
        selectedOption.prop('selected', true);
    }
}
init();
$('body').on('click', '#btn-place-order', (e) => placeOrder(e));