$('#frm-customer-info').validate({
    rules: {
        FirstName:
        {
            required: true,
        },
        LastName:
        {
            required: true,
        },
        Street:
        {
            required: true,
        },
        City:
        {
            required: true,
        },
        Country:
        {
            required: true,
        },
        ZipCode:
        {
            required: true,
            number : true,
        },
        PhoneNumber:
        {
            required: true,
        },
        Email:
        {
            required: true,
            email : true,
        },
    },

    messages: {
        FirstName:
        {
            required: "Please enter your first name",
        },
        LastName:
        {
            required: "Please enter your last name",
        },
        Street:
        {
            required: "Please enter your street",
        },
        City:
        {
            required: "Please enter your city",
        },
        Country:
        {
            required: "Please enter your country",
        },
        ZipCode:
        {
            required: "Please enter your zip code",
        },
        PhoneNumber:
        {
            required: "Please enter your phone number",
        },
        Email:
        {
            required: "Please enter your email",
        },
    }
});

function placeOrder(e) {
    e.preventDefault()
    if ($('#frm-customer-info').valid()) {
        var selectedPaymentMethod = $('input[name="PaymentMethod"]:checked').val();
        if (selectedPaymentMethod == undefined) {
            $.notify("Please select a payment method!", "warn")
            return
        }
        if (selectedPaymentMethod < 1 || selectedPaymentMethod > 3) {
            $.notify("Invalid selected payment method!", "warn")
            return
        }
        var checkTerm = $('#terms').prop("checked");
        if (!checkTerm) {
            $.notify("Please accept the terms and conditions!", "warn")
            return
        }
        var form = $('#frm-customer-info');
        form.submit();
    }
}

function init() {

    var message = $('#check-out-message').text();
    var status = Number($('#check-out-status').text());
    if (message != '') {
        var messageType = status == 200 ? "success" : "error"
        $.notify(message, messageType);
    }
}
init();
$('body').on('click', '#btn-place-order', (e) => placeOrder(e));