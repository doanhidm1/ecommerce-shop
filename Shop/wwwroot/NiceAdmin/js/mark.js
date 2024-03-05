
$(document).ready(function () {
    // Lấy đường dẫn hiện tại
    var currentPath = window.location.pathname;

    // Lặp qua từng đối tượng có class "nav-link"
    $("aside .nav-link").each(function () {
        // Kiểm tra xem href có khớp với đường dẫn hiện tại hay không
        let href = $(this).attr("href");
        if (currentPath.includes(href)) {
            $(this).removeClass("collapsed");
            $(this).addClass("hover");
        }
    });
});