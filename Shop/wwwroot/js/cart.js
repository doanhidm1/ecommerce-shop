//function
function addCart() {
    var qty = null;
    if (isNaN($(this).attr('qty')))
    {
        qty = Number($('#qty').val())
    } else
    {
        qty = Number($(this).attr('qty'))
    }
    // check if qty is valid >=1, otherwise bootbox alert
    if (isNaN(qty) || qty < 1) {
            bootbox.alert({
                message: "Quantity invalid",
            });
            return;
        }
    var productId = $(this).attr('product-id')
    $.ajax({
        type: 'post',
        url: `/cart/AddToCart/?productId=${productId}&qty=${qty}`,
        success: function (data) {
            handleResponse(data)
        }
    });
}

function confirmRemove() {
    var productId = $(this).attr('product-id')
    var productName = $(this).attr('product-name')
    bootbox.confirm({
        message: `Delete ${productName} from your cart?`,
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-success'
            },
            cancel: {
                label: 'No',
                className: 'btn-danger'
            }
        },
        callback: function (result) {
            if (result) {

                removeItem(productId)
            }
        }
    });
}
function removeItem(productId) {

    $.ajax({
        type: 'post',
        url: '/cart/RemoveFromCart/?productId=' + productId,
        success: function (data) {
            handleResponse(data)
        }
    });
}
function handleResponse(data) {
    if (data.statusCode == 200) {
        $.notify(data.message, "success")
        renderCart()
    }
    else {
        $.notify(data.message, "error")
    }
}

function renderCart() {
    $.ajax({
        type: 'post',
        url: '/cart/CartPartial',
        success: function (data) {
            $('#cart-partial').html(data)

        }
    });
}
//binding event
renderCart()
$('body').on('click', '.btn-cart', addCart);
$('body').on('click', '.btn-remove-item', confirmRemove);

