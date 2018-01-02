//使表格可以选择行
//需要在绑定OnSelect事件的元素上配置data-id属性。其中全选的data-id为all
//options{dataHidden:<dataHidden>,Data:{pages:{all:<bool>,1:[<id>,<id>],2:[]....}}}
//参数说明 dataHidden是用于回发数据的HIddenField的Selector
//checkbox注意绑定 onselectchange事件。事件参数需要传递当前页码
//整行选择时调用BindTrEvent
//在table上加 data-onlyOnePage 可以控制单个table是否开启跨页选择
function InitTableSelectable(table, options) {
    var allDatas = options.AllData;
    var pages = options.Data || {};
    var currentPageIndex;
    var eventList = [];
    var temp = $(table).attr('data-onlyOnePage');
    var onlyOnePage;
    if (temp) {
        onlyOnePage = temp.toLowerCase() === 'true';
    } else {
        if (globalOnlyPageSwitch) {
            onlyOnePage = globalOnlyPageSwitch;
        } else {
            onlyOnePage = false;
        }
    }


    function clearData() {
        for (var k in pages) {
            pages[k] = undefined;
        }
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
    }

    //selector:触发事件的控件
    //pageOptions{pageIndex:<pageIndex>}
    function onSelectChange(selector, pageOptions) {
        //        var eventTarget = event.currentTarget || event.target || event.srcElement || window.event;
        var eventTarget;
        if (typeof (selector) == typeof ('')) {
            eventTarget = $(selector)[0];
        } else if (selector) {
            eventTarget = selector.currentTarget || event.target || event.srcElement;
            selector.stopPropagation();
        }
        else {
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
            clearData();
            if (checked == true) {
                pages['all'] = true;
            } else {
                pages['all'] = false;
            }
            if (!pages[currentPageIndex + '']) {
                pages[currentPageIndex + ''] = [];
            }

        } else {

            if (!pages[currentPageIndex + '']) {
                pages[currentPageIndex + ''] = [];
            }

            if ((checked ^ (pages['all']))) {
                var index = pages[pageOptions.pageIndex + ''].findIndex(function (element, index, array) {
                    return (element === id);
                });
                if (index == -1) {
                    pages[pageOptions.pageIndex + ''].push(id);
                }

            } else {


                var indexRemove = pages[pageOptions.pageIndex + ''].findIndex(function (element, index, array) {
                    return (element === id);
                });
                if (indexRemove >= 0) {
                    pages[pageOptions.pageIndex + ''].splice(indexRemove, 1);
                }

            }
        }



        render();
        $(options.dataHidden).val(JSON.stringify(getIDs()));
    }

    function onPageChange(event, pageOptions) {

        if (!pageOptions) {
            pageOptions = { pageIndex: 1 };
        }



        currentPageIndex = pageOptions.pageIndex;

        for (var name in pages) {
            if (name + '' !== currentPageIndex && name.toLowerCase() != 'all') {
                pages[name] = [];
            }
        }



        if (pages.all) {
            $(':checkbox[data-id=all]')[0].checked = true;
        }
        render();
    }

    function getCurrentPageIndexData() {
        if (!pages[currentPageIndex + '']) {
            pages[currentPageIndex + ''] = [];
        }
        return pages[currentPageIndex + ''];
    }

    function render() {
        var currentPageData = getCurrentPageIndexData();
        if (table && currentPageData) {
            $(table).find(':checkbox[data-id!=all]').each(function (index, element) {
                var searchResult = currentPageData.indexOf(element.getAttribute('data-id'));
                var parents = JSHelper.GetParents(element, 'tr');
                var tr = parents[0];
                tr.style.backgroundColor = '';

                if (pages['all']) {
                    if (searchResult < 0) {
                        element.checked = true;
                    } else {
                        element.checked = false;
                    }
                } else {
                    if (searchResult >= 0) {
                        element.checked = true;
                    } else {
                        element.checked = false;
                    }
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

        var result = [];

        var isAll = false;
        for (var k in pages) {
            if (k === 'all' && pages[k]) {
                isAll = true;
            }
            if (k !== 'all' && pages[k]) {
                result = result.concat(pages[k]);
            }

        }


        if (onlyOnePage) {
            result = pages[currentPageIndex];
            if (isAll) {
                result = [];
                $(table).find(':checkbox:checked[data-id!=all]').each(function (index, element) {
                    var id = element.getAttribute('data-id');
                    result.push(id);
                });
            }
            return result;
        }



        if (isAll) {
            for (var i = 0; i < result.length; i++) {
                var index = allDatas.findIndex(function (element, index, array) {
                    if (element === result[i]) {
                        return true;
                    } else {
                        return false;
                    }
                });
                if (index >= 0) {
                    allDatas.splice(index, 1);
                }
            }
            result = allDatas;
        }
        return result;
    }

    function clearSelection() {
        clearData();
        render();
    }

    return {
        //获取数据源所有数据
        AllDatas: allDatas,
        //绑定郑航选择
        BindTrEvent: bindTrEvent,
        //全不选
        ClearSelection: clearSelection,
        //存储每页已经被选择的数据 但是请不要直接使用 需要获取所有已经被选择的数据 调用GetIDs方法
        Data: pages,
        //获取当前页被选择的数据
        GetCurrentPageData: getCurrentPageIndexData,
        //获取所有被选择的数据的ID
        GetIDs: getIDs,
        //checkbox绑定的事件
        OnSelectChange: onSelectChange,
        //翻页时绑定的事件
        OnPageChange: onPageChange,
        //行选中事件
        TrSelector: trSelector
    };


}
