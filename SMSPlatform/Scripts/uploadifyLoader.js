var options = {
    'buttonCursor': "hand", //指针样式
    'swf': "/Scripts/uploadify/uploadify.swf", //Flash地址
    'auto': true, //选择文件后是否自动上传
    'buttonText': '上传文件', //按钮文字
    'queueID': 'queue', //文件列表Div
    'uploader': '/api/contactor/uploadcontactors', //一般处理程序
    //            'width': '73', //控件宽
    //            'height': '25', //控件高
    'queueSizeLimit': 5000, //同时上传数量
    'removeTimeout': 1, //文件上传完成后消失时间(秒)
    'multi': false, //是否允许多选
    //                'uploadLimit': 1, //上传文件数量
    'removeCompleted': false, //完成后是否移除序列
    'fileTypeExts': '*.xlsx', //限制文件类型 //flv/mp4/mov/f4v/3gp
    'onUploadStart': function (file) {




        if (hasFlash) {
            //                    console.log(file);
            //                    uploadifyInstance.uploadify('cancel', file.id);
        } else {
            return true;
        }
    },
    'onUploadError': function (file, errCode, errorMsg, errorString) {
        if (file) {
            console.log({ file: file, code: errCode, msg: errorMsg, str: errorString });

        }
    },
    onUploadSuccess: function (file, data, response) {
        if (hasFlash) {
            if (data) {
                $('#' + file.id).css('background', 'pink');
                $('#' + file.id + ' .data').text(data);
                window.getData();
            }
        }
    }
};

var uploadifyInstance;
var hasFlash = false;
try {
    hasFlash = Boolean(new ActiveXObject('ShockwaveFlash.ShockwaveFlash'));
} catch (exception) {
    hasFlash = ('undefined' != typeof navigator.mimeTypes['application/x-shockwave-flash']);
}
var selement = document.createElement('script');
var lelement = document.createElement('link');
lelement.rel = 'stylesheet';
if (hasFlash) {
    selement.src = '/Scripts/uploadify/jquery.uploadify.min.js';
    lelement.href = '/Scripts/uploadify/css/uploadify.css';

    selement.onload = function (e) {
        //                uploadifyInstance = $("#file_upload1");
        uploadifyInstance = $("#file_upload1").uploadify(options);
    };

} else {
    selement.src = '/Scripts/uploadifive/jquery.Huploadify.js';
    lelement.href = '/Scripts/uploadifive/Huploadify.css';


    selement.onload = function () {
        //                 $("#file_upload2");
        uploadifyInstance = $("#file_upload2").Huploadify(options);
        $('#file_upload1').hide();
    };
}



document.head.appendChild(selement);
document.head.appendChild(lelement);



function setOptions(key, value) {
    if (uploadifyInstance) {
        if (hasFlash) {
            uploadifyInstance.uploadify('settings', key, value);
        } else {
            uploadifyInstance.options[key] = value;
        }
    }
}

function getOptions(key) {
    if (uploadifyInstance) {
        if (hasFlash) {
            return uploadifyInstance.uploadify('settings', key);
        } else {
            return uploadifyInstance.options[key];
        }
    }
}



