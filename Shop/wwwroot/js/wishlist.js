function renderWishlist() {
    if ($('#wishlist-partial').length == 0) {
        return
    }
    $.ajax({
        type: 'post',
        url: '/Wishlist/WishlistPartial',
        success: function (data) {
            $('#wishlist-partial').html(data)
            var count = $('#wishlist-count').text()
            $('#wl-count').text(count)
        }
    });
}

function addWishlist() {
    var productId = $(this).attr('product-id')
    $.ajax({
        type: 'post',
        url: `/Wishlist/AddToWishlist/?productId=${productId}`,
        success: function (data) {
            handleResponseWishlist(data)
        }
    });
}

function confirmRemoveWishlist() {
    var productId = $(this).attr('product-id')
    var productName = $(this).attr('product-name')
    bootbox.confirm({
        message: `Delete ${productName} from your wishlist?`,
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
                removeWishlistItem(productId)
            }
        }
    });
}

function removeWishlistItem(productId) {
    $.ajax({
        type: 'post',
        url: `/Wishlist/RemoveFromWishlist/?productId=${productId}`,
        success: function (data) {
            handleResponseWishlist(data)
        }
    });
}

function handleResponseWishlist(data) {
    if (data.statusCode == 200) {
        $.notify(data.message, "success")
        renderWishlist()
    }
    else {
        $.notify(data.message, "error")
    }
}

renderWishlist()
$('body').on('click', '.btn-wishlist', addWishlist);
$('body').on('click', '.btn-remove-wl', confirmRemoveWishlist);