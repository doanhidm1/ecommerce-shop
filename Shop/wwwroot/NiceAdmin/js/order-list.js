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
    updateAdress();
    filterproduct();
    let outputDistrict = "<option value=''></option>";
    for (let i = 0; i < districtList.length; i++) {
        if (districtList[i].idProvince === idProvince) {
            outputDistrict += `<option value='${districtList[i].idDistrict}'>${districtList[i].name}</option>`;
        }
    }
    $('#district-town').html(outputDistrict);
    $('.district-town-select > span').css('display', 'none');
    // $('#district-town').trigger('change');
});

$('#district-town').on("change", async function () {
    $('.wardcommune-select > span').css('display', 'block');
    const idDistrict = $('#district-town').val();
    const communeList = await getCommune(idDistrict) || [];
    updateAdress();
    filterproduct();
    let outputCommune = "<option value=''></option>";
    for (let i = 0; i < communeList.length; i++) {
        if (communeList[i].idDistrict === idDistrict) {
            outputCommune += `<option value='${communeList[i].idCommune}'>${communeList[i].name}</option>`;
        }
    }
    $('#ward-commune').html(outputCommune);
    $('.ward-commune-select > span').css('display', 'none');
    // $('#ward-commune').trigger('change');
});

$('#ward-commune').on("change", function () {
    updateAdress();
    filterproduct();
});

var homeconfig = {
    pageSize: 10, // số lượng bản ghi mỗi page
    pageIndex: 1 // trang đầu tiên
}

var dataTable =
{
    loadData: function (changePageSize) {

        var total = 0;
        var model = new Object();
        model.PageSize = homeconfig.pageSize;
        model.PageIndex = homeconfig.pageIndex;
        model.CustomerName = $('#customer-name').val() ?? "";
        model.Phone = $('#Phone').val() ?? "";
        model.Email = $('#Email').val() ?? "";
        model.CityProvince = $('#cityprovince').val() ?? "";
        model.DistrictTown = $('#districttown').val() ?? "";
        model.WardCommune = $('#wardcommune').val() ?? "";
        model.Status = $('#order-status').val() === '' ? null : Number($('#order-status').val());
        model.OrderBy = Number($('#order-by').val());

        $.ajax({
            type: 'post',
            url: '/order/OrderListPartial', //url của action return ra partial view
            data: JSON.stringify(model),
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                $("#table-content").html(data);

                total = $('.total-count:first').data('count') === undefined ? 0 : $('.total-count:first').data('count');
                $('#count').text(`All orders (${total})`)
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

dataTable.loadData(true);

function filterproduct() {
    dataTable.loadData(true);
}

$('body').on('change', '#customer-name', filterproduct);
$('body').on('change', '#Phone', filterproduct);
$('body').on('change', '#Email', filterproduct);
$('body').on('change', '#order-status', filterproduct);
$('body').on('change', '#order-by', filterproduct);
