var homeconfig = {
    pageSize: 10, // số lượng bản ghi mỗi page
    pageIndex: 1 // trang đầu tiên
}

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
        model.FromPrice = fromPrice;
        model.ToPrice = toPrice;
        model.CategoryId = $('#product-category-filter').val()
        model.Rating = rating;
        model.KeyWord = $('#product-search').val() ?? null;
        model.SortBy = Number($('#product-order-by').val());
        model.SelectedBrandIds = [];
        var brandIds = $('#product-brand-filter').val();
        if (brandIds) {
            model.SelectedBrandIds.push(brandIds)
        }

        $.ajax({
            type: 'post',
            url: '/adminproduct/ProductListPartial', //url của action return ra partial view
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                $("#table-content").html(data);

                total = $('.total-count:first').data('count') === undefined ? 0 : $('.total-count:first').data('count');
                $('#count').text(total + " products found")
                dataTable.paging(total, function () {

                }, changePageSize);
            },
            error: function() {
                $.notify("Failed to load data", "error");
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
            $('#pagination').off("page");
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

dataTable.loadData(true);

function filterproduct() {
    dataTable.loadData(true);
}

function ExportWord() {
    var model = new Object();
    model.PageSize = homeconfig.pageSize;
    model.PageIndex = homeconfig.pageIndex;
    model.IsFeatured = false;
    model.FromPrice = fromPrice;
    model.ToPrice = toPrice;
    model.CategoryId = $('#product-category-filter').val()
    model.Rating = rating;
    model.KeyWord = $('#product-search').val() ?? null;
    model.SortBy = Number($('#product-order-by').val());
    model.SelectedBrandIds = [];
    var brandIds = $('#product-brand-filter').val();
    if (brandIds) {
        model.SelectedBrandIds.push(brandIds)
    }

    $.ajax({
        type: 'post',
        url: '/adminproduct/exportword', //url của action return ra partial view
        data: JSON.stringify(model),
        xhrFields: { responseType: 'blob' },
        contentType: "application/json; charset=utf-8",
        success: function (blob) {
            var windowUrl = window.URL || window.webkitURL;
            var url = windowUrl.createObjectURL(blob);
            var anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = `ProductReport${new Date().toJSON()}.docx`;
            anchor.click();
            windowUrl.revokeObjectURL(url);
            $.notify("Export to word file successfully", "success");
        },
        error: function() {
            $.notify("Failed to export to word file", "error");
        }
    });
}

function ExportExcel() {
    var model = new Object();
    model.PageSize = homeconfig.pageSize;
    model.PageIndex = homeconfig.pageIndex;
    model.IsFeatured = false;
    model.FromPrice = fromPrice;
    model.ToPrice = toPrice;
    model.CategoryId = $('#product-category-filter').val()
    model.Rating = rating;
    model.KeyWord = $('#product-search').val() ?? null;
    model.SortBy = Number($('#product-order-by').val());
    model.SelectedBrandIds = [];
    var brandIds = $('#product-brand-filter').val();
    if (brandIds) {
        model.SelectedBrandIds.push(brandIds)
    }

    $.ajax({
        type: 'post',
        url: '/adminproduct/exportexcel', //url của action return ra partial view
        data: JSON.stringify(model),
        xhrFields: { responseType: 'blob' },
        contentType: "application/json; charset=utf-8",
        success: function (blob) {
            var windowUrl = window.URL || window.webkitURL;
            var url = windowUrl.createObjectURL(blob);
            var anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = `ProductReport${new Date().toJSON()}.xlsx`;
            anchor.click();
            windowUrl.revokeObjectURL(url);
            $.notify("Export to excel file successfully", "success");
        },
        error: function() {
            $.notify("Failed to export to excel file", "error");
        }
    });
}

function ExportPdf() {
    var model = new Object();
    model.PageSize = homeconfig.pageSize;
    model.PageIndex = homeconfig.pageIndex;
    model.IsFeatured = false;
    model.FromPrice = fromPrice;
    model.ToPrice = toPrice;
    model.CategoryId = $('#product-category-filter').val()
    model.Rating = rating;
    model.KeyWord = $('#product-search').val() ?? null;
    model.SortBy = Number($('#product-order-by').val());
    model.SelectedBrandIds = [];
    var brandIds = $('#product-brand-filter').val();
    if (brandIds) {
        model.SelectedBrandIds.push(brandIds)
    }

    $.ajax({
        type: 'post',
        url: '/adminproduct/exportpdf', //url của action return ra partial view
        data: JSON.stringify(model),
        xhrFields: { responseType: 'blob' },
        contentType: "application/json; charset=utf-8",
        success: function (blob) {
            var windowUrl = window.URL || window.webkitURL;
            var url = windowUrl.createObjectURL(blob);
            var anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = `ProductReport${new Date().toJSON()}.pdf`;
            anchor.click();
            windowUrl.revokeObjectURL(url);
            $.notify("Export to pdf file successfully", "success");
        },
        error: function () {
            $.notify("Failed to export to pdf file", "error");
        }
    });
}

$('body').on('change', '#product-order-by', filterproduct);
$('body').on('change', '#product-category-filter', filterproduct);
$('body').on('change', '#product-brand-filter', filterproduct);
$('body').on('change', '#product-search', filterproduct);
$('body').on('click', '#btn-export-word', ExportWord);
$('body').on('click', '#btn-export-excel', ExportExcel);
$('body').on('click', '#btn-export-pdf', ExportPdf);