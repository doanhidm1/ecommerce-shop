function init() {
    var message = $('#message').text();
    var status = Number($('#status').text());
    if (message != '') {
        var messageType = status == 200 ? "success" : "error"
        $.notify(message, messageType);
    }
}
init();