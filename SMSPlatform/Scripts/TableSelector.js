globalOnlyPageSwitch = false;

//使表格可以选择行
//AllData
//需要在绑定OnSelect事件的元素上配置data-id属性。其中全选的data-id为all
//options{dataHidden:<dataHidden>,Data:{pages:{all:<bool>,1:[<id>,<id>],2:[]....}}}
//参数说明 dataHidden是用于回发数据的HIddenField的Selector
//checkbox注意绑定 onselectchange事件。事件参数需要传递当前页码
//整行选择时调用BindTrEvent
//在table上加 data-onlyOnePage 可以控制单个table是否开启跨页选择
function InitTableSelectable(table, options) {
    var allDatas = options.AllData;
    var isAll = false;
    var currentPageIndex;
    var eventList = [];
    var innerTable = table;
    var temp = $(table).attr('data-onlyOnePage');
    var onlyOnePage;
    var onchange = options.onchange || function () { };
    if (temp) {
        onlyOnePage = temp.toLowerCase() === 'true';
    } else {
        if (globalOnlyPageSwitch) {
            onlyOnePage = globalOnlyPageSwitch;
        } else {
            onlyOnePage = false;
        }
    }
    var innerOptions = options;
    var selectedIDs = options.selectedIDs || [];
    if (options.lazyCallback) {
        options.lazyCallback(function () {
            console.log('lazy callback');
            $(selectedIDs).each((i, e) => {
                var selector = $(table).find(':checkbox[data-id=' + e + ']')[0];
                if (selector) {
                    selector.checked = true;
                }
            });
        });
    } else {
        $(selectedIDs).each((i, e) => {
            var selector = $(table).find(':checkbox[data-id=' + e + ']')[0];
            if (selector) {
                selector.checked = true;
            }
        });
    }





    function clearData() {
        selectedIDs = [];
    }

    function bindTrEvent(selector, func) {

        if (eventList.findIndex(function (element, index, array) {
            if (element.name === func.name) {
                return true;
            } else {
                return false;
            }
        }) < 0) {
            eventList.push(func);
            $(selector).bind('click', func);
            $(selector).find(':checkbox').bind('click', function (event) { event.stopPropagation(); });
        }
    }

    function trSelector(event) {
        var eventTarget = event.currentTarget || event.target || event.srcElement;
        $(eventTarget).find(':checkbox')[0].click();
        onchange();
    }

    //selector:触发事件的控件
    //pageOptions{pageIndex:<pageIndex>}
    function onSelectChange(selector, pageOptions) {
        //        var eventTarget = event.currentTarget || event.target || event.srcElement || window.event;
        var eventTarget;
        if (selector.constructor.name == 'HTMLTableElement') {
            eventTarget = selector;
        } else if (typeof (selector) == typeof ('')) {
            eventTarget = $(selector)[0];
        } else if (selector) {
            eventTarget = selector.currentTarget || event.target || event.srcElement;
            selector.stopPropagation();
        } else {
            console.error('selector is undefined');
            return;
        }
        if (!pageOptions) {
            pageOptions = { pageIndex: 1 };
        }
        currentPageIndex = pageOptions.pageIndex;
        var checked = eventTarget.checked;
        var id = eventTarget.getAttribute('data-id');



        if (id === 'all') {
            if (checked) {
                selectedIDs = allDatas.map((e, i) => parseInt(e));
                isAll = true;
            } else {
                selectedIDs = [];
                isAll = false;
            }


        } else {

            if (!checked) {
                selectedIDs = selectedIDs.filter((e, i) => e !== parseInt(id));
            } else {
                selectedIDs.push(parseInt(id));
            }
        }



        render();
        $(options.dataHidden).val(JSON.stringify(getIDs()));
        onchange();
    }

    function onPageChange(event, pageOptions) {


        if (innerOptions.lazyCallback) {
            innerOptions.lazyCallback(innerPageChange(event, pageOptions));
        } else {
            innerPageChange(event, pageOptions);
        }
    }

    function innerPageChange(event, pageOptions) {
        if (!pageOptions) {
            pageOptions = { pageIndex: 1 };
        }

        currentPageIndex = pageOptions.pageIndex;

        var currentPageIDs = $(table).find(':checkbox[data-id!=all]').map((i, e) => parseInt(e.getAttribute('data-id')));
        var currIDs = currentPageIDs.filter((i, e) => selectedIDs.indexOf(e) !== -1);


        if (isAll) {
            $(':checkbox[data-id=all]')[0].checked = true;
        }

        render();
    }



    function render() {

       
        if (table && selectedIDs) {
            $(table).find(':checkbox[data-id!=all]').each(function (index, element) {
                var searchResult = selectedIDs.indexOf(parseInt(element.getAttribute('data-id')));
                var parents = JSHelper.GetParents(element, 'tr');
                var tr = parents[0];
                tr.style.backgroundColor = '';
                if (searchResult >= 0) {
                    element.checked = true;
                } else {
                    element.checked = false;
                }
            });

            $(table).find(':checkbox:checked[data-id!=all]').each(function (index, element) {
                var parents = JSHelper.GetParents(element, 'tr');
                var tr = parents[0];
                tr.style.backgroundColor = 'lightblue';
            });
        }
    }

    function getIDs() {
        return selectedIDs;
    }

    function clearSelection() {
        clearData();
        render();
    }

    function setSelectedIDs(ids) {
        selectedIDs = ids;
        onSelectChange(innerTable);
    }

    function setAllData(data) {
        allDatas = data;
        if (isAll)
            selectedIDs = selectedIDs.filter((e, i) => {
                return data.find((ee, ii) => ee === e) !== undefined;
            });

    }

    return {
        //获取数据源所有数据
        AllDatas: allDatas,
        //绑定郑航选择
        BindTrEvent: bindTrEvent,
        //全不选
        ClearSelection: clearSelection,
        //存储每页已经被选择的数据 但是请不要直接使用 需要获取所有已经被选择的数据 调用GetIDs方法
        //        Data: pages,
        //获取当前页被选择的数据
        //        GetCurrentPageData: selectedIDs,
        //获取所有被选择的数据的ID
        GetIDs: getIDs,
        //checkbox绑定的事件
        OnSelectChange: onSelectChange,
        //翻页时绑定的事件
        OnPageChange: onPageChange,
        //行选中事件
        TrSelector: trSelector,
        SetSelectedIDs: setSelectedIDs,
        SetAllData: setAllData,
    };


}
