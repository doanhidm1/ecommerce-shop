var homeconfig = {
    pageSize: 6, // số lượng bản ghi mỗi page
    pageIndex: 1 // trang đầu tiên
}

var dataTable =
{
    loadData: function (changePageSize) {

        var total = 0;
        var model = new Object();
        model.PageSize = homeconfig.pageSize
        model.PageIndex = homeconfig.pageIndex;

        //optional cho phép vừa phân trang vừa lọc điều kiện
        // model.CategoryId = $(".input-radio input[type='radio']:checked").val()
        // model.KeyWord = $('#keyword').val()
        // model.SortBy = Number($('#product-order-by').val());
        // model.FromPrice = Number($('#price-min').val());
        // model.ToPrice = Number($('#price-max').val());
        //end

        $.ajax({
            type: 'post',
            url: '/product/ProductListPartial', //url của action return ra partial view
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                $("#table-content").html(data);

                total = $('.total-count:first').data('count')
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
            visiblePages: 3, //số page muốn hiển thị
            onPageClick: function (event, page) {

                homeconfig.pageIndex = page;
                dataTable.loadData();

            }
        });
    },
}

dataTable.loadData(true);

function filterproduct() {
    dataTable.loadData(true);
}

function changePage() {
    homeconfig.pageSize = Number($('#product-page-size').val());
    dataTable.loadData(true);
}

// biding event

// $('body').on('change', ".checkbox-filter input[type='radio']", filterproduct);
// $('body').on('change', '#price-min', filterproduct);
// $('body').on('change', '#price-max', filterproduct);
$('body').on('change', '#product-order-by', filterproduct);
$('body').on('change', '#product-page-size', changePage);

