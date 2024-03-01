function UpdateCart() {
    var updatedCart = [];

    // Lặp qua từng dòng trong bảng
    $('#cart-table tbody tr').each(function () {
        var productId = $(this).find('.cart-product-quantity').attr('product-id');
        var quantity = $(this).find('.cart-product-quantity input').val();

        // Tạo đối tượng CartItemViewModel cho mỗi sản phẩm
        var cartItem = {
            ProductId: productId,
            Quantity: quantity
        };

        // Thêm vào danh sách updatedCart
        updatedCart.push(cartItem);
    });

    $.ajax({
        type: 'post',
        url: '/Cart/UpdateCart',
        data: JSON.stringify(updatedCart),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            handleResponse(data);
        }
    });
}

function updateSubtotalAndTotal() {
    var subtotal = 0;

    // Lặp qua tất cả các dòng sản phẩm và tính tổng Subtotal
    $('.total-col').each(function () {
        subtotal += parseFloat($(this).text().replace('$', ''));
    });

    // Cập nhật giá trị Subtotal
    $('#subtotal').text('$' + subtotal.toFixed(2));

    // Cập nhật giá trị Total (nếu cần)
    var shipping = 0; // Thêm bất kỳ phí vận chuyển nếu có
    var total = subtotal + shipping;
    $('#total').text('$' + total.toFixed(2));
}

$('.cart-product-quantity input').on('input', function () {
    // Lấy giá trị số lượng, giá và ID của sản phẩm
    var quantity = $(this).val();
    var productId = $(this).attr('product-id');
    var price = parseFloat($('td[product-id="' + productId + '"].price-col').text().replace('$', ''));

    // Tính lại giá và cập nhật vào cột Total
    var total = quantity * price;
    $('td[product-id="' + productId + '"].total-col').text('$' + total.toFixed(2));
    updateSubtotalAndTotal();
});

$('.pm-input').on('input', function () {
    var pm = $(this).val();
    $('.btn-order').attr('href', `/checkout?PaymentMethod=${pm}`);
    
});


$('body').on('click', '.btn-remove', function (e) {
    var productId = $(this).attr("product-id");
    // Xóa dòng có ID tương ứng
    $('#' + productId).remove();
    updateSubtotalAndTotal();
});


$('body').on('click', '#btn-update-cart', function (e) {
    e.preventDefault();
    UpdateCart();
    setTimeout(function () {
        location.reload();
    }, 3000);
});
