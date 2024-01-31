var homeconfig = {
    pageSize: 8, // số lượng bản ghi mỗi page
    pageIndex: 1 // trang đầu tiên
}

var categoryId = '';
var fromPrice = null;
var toPrice = null;
var rating = null;

var dataTable =
{
    loadData: function (changePageSize) {

        var total = 0;
        var model = new Object();
        model.PageSize = homeconfig.pageSize;
        model.PageIndex = homeconfig.pageIndex;
        model.IsFeatured = false;

        //optional cho phép vừa phân trang vừa lọc điều kiện
        // model.CategoryId = $(".input-radio input[type='radio']:checked").val()
        model.FromPrice = fromPrice;
        model.ToPrice = toPrice;
        model.CategoryId = categoryId;
        model.Rating = rating;
        // model.KeyWord = $('#keyword').val()
        model.SortBy = Number($('#product-order-by').val());
        model.SelectedBrandIds = [];
        $('.brand-filter:checked').each(function () {
            model.SelectedBrandIds.push(this.id)
        });
        
        //end

        $.ajax({
            type: 'post',
            url: '/product/ProductListPartial', //url của action return ra partial view
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                $("#table-content").html(data);

                total = $('.total-count:first').data('count') === undefined ? 0 : $('.total-count:first').data('count');
                $('#count').text(total + " products found")
                dataTable.paging(total, function () {

                }, changePageSize);
            }
        });
    },
    paging: function (totalRow, callback, changePageSize) {

        var totalPage = 0;
        if (totalRow < homeconfig.pageSize) {
            totalPage = 1
        }
        else {
            totalPage = Math.ceil(totalRow / homeconfig.pageSize);
        }
        if ($('#pagination a').length === 0 || changePageSize === true) {
            $('#pagination').empty(); //pagination là id của <div id ="pagination "> </div> có thể thay đổi tùy ý
            $('#pagination').removeData("twbs-pagination");
            $('#pagination').unbind("page");
        }
        $('#pagination').twbsPagination({
            totalPages: totalPage,
            first: "<<", //thay đổi ký tự theo ý muốn
            next: ">", //thay đổi ký tự theo ý muốn
            last: ">>", //thay đổi ký tự theo ý muốn
            prev: "<", //thay đổi ký tự theo ý muốn
            visiblePages: 10, //số page muốn hiển thị
            onPageClick: function (event, page) {

                homeconfig.pageIndex = page;
                dataTable.loadData();

            }
        });
    },
}


$.ajax({
    type: 'post',
    url: '/product/FeaturedProductsPartial', //url của action return ra partial view
    success: function (data) {
        $("#featured-product").html(data);
    }
});

dataTable.loadData(true);

function filterproduct() {
    dataTable.loadData(true);
}

function changePage() {
    homeconfig.pageSize = Number($('#product-page-size').val());
    dataTable.loadData(true);
}

// biding event

$('body').on('click', 'input.price-filter', function () {
    // get price from input (this)
    fromPrice = Number($(this).attr('price-min'));
    toPrice = Number($(this).attr('price-max'));
    filterproduct();
});

$('body').on('change', 'input.rating-filter', function () {
    // Lấy giá trị rating từ thuộc tính rating của checkbox
    rating = $(this).prop('checked') ? Number($(this).attr('rating')) : null;

    // Nếu checkbox được chọn, bỏ chọn tất cả các checkbox khác
    if ($(this).prop('checked')) {
        $('input.rating-filter').not(this).prop('checked', false);
    }

    // Gọi hàm filterproduct() và truyền giá trị rating vào
    filterproduct();
});

// $('body').on('change', '#price-min', filterproduct);
// $('body').on('change', '#price-max', filterproduct);
$('body').on('change', '#product-order-by', filterproduct);
$('body').on('change', '#product-page-size', changePage);
$('body').on('change', '.brand-filter', filterproduct);
$('body').on('click', 'a.category-id', function () {
    if ($(this).hasClass('selected-category')) {
        categoryId = '';
        $(this).removeClass('selected-category');
    } else {
        categoryId = this.id;
        $('.selected-category').removeClass('selected-category');
        $(this).addClass('selected-category');
    }
    filterproduct();
});