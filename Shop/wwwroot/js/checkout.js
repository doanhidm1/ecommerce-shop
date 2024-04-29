const apiUrl = "https://vietnam-administrative-division-json-server-swart.vercel.app";
const apiEndpointDistrict = apiUrl + '/district/?idProvince=';
const apiEndpointCommune = apiUrl + '/commune/?idDistrict=';

async function getDistrict(idProvince) {
    const { data: districtList } = await axios.get(apiEndpointDistrict + idProvince);
    return districtList
}

async function getCommune(idDistrict) {
    const { data: communeList } = await axios.get(apiEndpointCommune + idDistrict);
    return communeList
}

$.validator.addMethod("phoneUS", function (phone_number, element) {
    phone_number = phone_number.replace(/\s+/g, "");
    return this.optional(element) || phone_number.match(/^0\d{9}$/);
}, "Please enter a valid 10-digit phone number.");

$.validator.addMethod("mail", function (email, element) {
    return this.optional(element) || email.match(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/);
}, "Please enter a valid email address.");

$('#frm-customer-info').validate({
    rules: {
        CustomerName: {
            required: true,
        },
        CityProvince: {
            required: true,
        },
        DistrictTown: {
            required: true,
        },
        WardCommune: {
            required: true,
        },
        ExactAddress: {
            required: true,
        },
        PhoneNumber: {
            required: true,
            phoneUS: true,
        },
        Email: {
            required: true,
            mail: true,
        },
        terms: {
            required: true,
        },
    },
    messages: {
        CustomerName: {
            required: "Please enter your name",
        },
        CityProvince: {
            required: "Please select your city/province",
        },
        DistrictTown: {
            required: "Please select your district/town",
        },
        WardCommune: {
            required: "Please select your ward/commune",
        },
        ExactAddress: {
            required: "Please enter your exact address",
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

function updateAdress() {
    var cityProvince = $('#city-province option:selected').text();
    var districtTown = $('#district-town option:selected').text();
    var wardCommune = $('#ward-commune option:selected').text();
    $('#cityprovince').val(cityProvince);
    $('#districttown').val(districtTown);
    $('#wardcommune').val(wardCommune);
}

$('#city-province').on("change", async function () {
    $('.district-town-select > span').css('display', 'block');
    const idProvince = $('#city-province').val();
    let outputCommune = "";
    $('#ward-commune').html(outputCommune);
    const districtList = await getDistrict(idProvince) || [];
    let outputDistrict = "";
    for (let i = 0; i < districtList.length; i++) {
        if (districtList[i].idProvince === idProvince) {
            outputDistrict += `<option value='${districtList[i].idDistrict}'>${districtList[i].name}</option>`;
        }
    }
    $('#district-town').html(outputDistrict);
    $('.district-town-select > span').css('display', 'none');
    $('#district-town').trigger('change');
});

$('#district-town').on("change", async function () {
    $('.wardcommune-select > span').css('display', 'block');
    const idDistrict = $('#district-town').val();
    const communeList = await getCommune(idDistrict) || [];
    let outputCommune = "";
    for (let i = 0; i < communeList.length; i++) {
        if (communeList[i].idDistrict === idDistrict) {
            outputCommune += `<option value='${communeList[i].idCommune}'>${communeList[i].name}</option>`;
        }
    }
    $('#ward-commune').html(outputCommune);
    $('.ward-commune-select > span').css('display', 'none');
    $('#ward-commune').trigger('change');
});

$('#ward-commune').on("change", function () {
    updateAdress();
});

$('body').on('click', '#btn-place-order', (e) => placeOrder(e));
$('#city-province').trigger('change');