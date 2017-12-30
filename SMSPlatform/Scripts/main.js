~function (window, jQuery, undefined) {
    window.notifyContent = {
//        icon: 'glyphicon glyphicon-warning-sign',
//        title: 'Bootstrap notify',
//        message: 'Turning standard Bootstrap alerts into "notify" like notifications',
//        url: 'https://github.com/mouse0270/bootstrap-notify',
//        target: '_blank'
    };
    window.notifyOptions = {
        delay:2000
    };
    $.extend({
        notifySuccess: function(title, msg) {
            $.notify(Object.assign({}, window.notifyContent, { 'type': 'success', 'message': msg, 'title': title }), Object.assign({}, window.notifyOptions, { type: 'success' }));
        },
        notifyWarn:function(title, msg) {
            $.notify(Object.assign({}, window.notifyContent, { 'type': 'warn', 'message': msg, 'title': title }), Object.assign({}, window.notifyOptions, { type: 'warning' }));

        },
        notifyInfo: function(title, msg) {
            $.notify(Object.assign({}, window.notifyContent, { 'type': 'info', 'message': msg, 'title': title }),
                Object.assign({},window.notifyOptions, {type:'info'}));

            
        },
        notifyError: function(title, msg) {
            $.notify(Object.assign({}, window.notifyContent, { 'type': 'error', 'message': msg, 'title': title }), Object.assign({}, window.notifyOptions, { type: 'danger' }));

        }


    });

}(window, jQuery);