$('#create-product-form').validate({
    rules: {
        ProductName: {
            required: true
        },
        Price: {
            required: true
        },
        Quantity: {
            required: true
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
            required: "Price is required"
        },
        Quantity: {
            required: "Quantity is required"
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
        }
    }
});

function save(e) {
    e.preventDefault()
    if ($('#create-product-form').valid()) {
        var a = $('#create-product-form');
        a.submit();
    }
};

$('body').on('click', '#btn-create-product', (e) => save(e));