$('#create-product-form').validate({
    rules: {
        ProductName: {
            required: true
        },
        Price: {
            required: true,
            number: true,
            min: 1
        },
        DiscountPrice: {
            number: true,
            min: 1
        },
        Quantity: {
            required: true,
            number: true,
            min: 1
        },
        Detail: {
            required: true
        },
        Description: {
            required: true
        },
        CategoryId: {
            required: true
        },
        BrandId: {
            required: true
        },
        Images: {
            required: true
        }
    },
    messages: {
        ProductName: {
            required: "Product name is required"
        },
        Price: {
            required: "Price is required",
            number: "Price must be a number",
            min: "Price must be greater than 0"
        },
        Quantity: {
            required: "Quantity is required",
            number: "Quantity must be a number",
            min: "Quantity must be greater than 0"
        },
        Detail: {
            required: "Detail is required"
        },
        Description: {
            required: "Description is required"
        },
        CategoryId: {
            required: "Category is required"
        },
        BrandId: {
            required: "Brand is required"
        },
        Images: {
            required: "Images is required"
        },
        DiscountPrice: {
            number: "Discount price must be a number",
            min: "Discount price must be greater than 0"
        }
    }
});

function updateSubmitButtonState() {
    // if user give discount price, check if it smaller than price   
    var dPriceCheck = parseFloat($('#DiscountPrice').val()) < parseFloat($('#Price').val())
    var isValid = $('#create-product-form').valid();
    $('#btn-create-product').prop('disabled', !(isValid && dPriceCheck));
}

// Initial state check
// updateSubmitButtonState();

// Update button state whenever the form changes
$('#create-product-form').on('input change', function () {
    updateSubmitButtonState();
});

function save(e) {
    e.preventDefault()
    if ($('#create-product-form').valid()) {
        var a = $('#create-product-form');
        a.submit();
    }
};

$('body').on('click', '#btn-create-product', (e) => save(e));