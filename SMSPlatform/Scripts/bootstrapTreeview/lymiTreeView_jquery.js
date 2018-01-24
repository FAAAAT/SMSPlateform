// ReSharper disable once WrongExpressionStatement
~function (window, jQuery, undefined) {
    $.extend({
        lymiTreeSelector: function (options) {
            var defaultOptions = {
                isAsync: true,
                treeviewID: 'treeViewSelector',
                userDefineNodes: {},
                onTreeInited: undefined,
                onClientSelected: undefined,
                requestUrl: '',
                sendData: {},
                selectedMIDs: [],
                treeviewOptions: {},
                done:null,
            };
            var finalOptions = $.extend({}, defaultOptions, options);

            return new LymiTreeSelector(finalOptions);
        }
    });
}(window, jQuery);


function LymiTreeSelector(options) {
    //设置搜索框的placeholder
    $('#searchBox').attr('placeholder', '筛选部门名称');
    
    this.options = options;
    this.instance = {};
    this.init = () => {
        this.oldData ? treeSelectorInit(this.oldData, this) : treeSelectorInit(null, this);
    }
    this.init();
    if (options.done) {
        options.done();
    }

}





//指定initOptions中的属性将会覆盖控件中设置的options属性
function treeSelectorInit(initOptions, departmentSelector) {
    var inited = false;
    var DepartmentSelector = departmentSelector;
    var doptions = departmentSelector.options;
    var treeViewID = doptions.treeviewID;
    ///window.DepartmentSelector<%=this.ParentID%>.<%=this.ClientID%>
    var IsAsync = doptions.isAsync;
    var UserInsertData = doptions.userDefineNodes;

    //    $("#<%=hid_chooseDepIDs.ClientID%>").val('');
    if (IsAsync) {
        $(".loadimg-div").show();
        //判断是否有历史数据  如果有 赋值
        $.ajax({
            type: "get",
            //        url: "Manager.aspx/GetTreeData",
            url: doptions.requestUrl,
            data: doptions.sendData,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                console.log(1);
                //清空旧的选中值
                //            console.log(data.d);
                //返回的数据用data.d获取内容
                //var json = JSON.parse(data.d);
                var options = doptions.treeviewOptions;

                ///用initOptions中的选项覆盖options中的选项
                if (initOptions) {
                    for (var attr in initOptions) {
                        options[attr] = initOptions[attr];
                    }
                }
                //获取程序设置的默认选中的元素的值 selectedMIDs is  an array  of mids
                var defaultSelectedItems = doptions.selectedMIDs;
                var items = data;

                //方便递归调用 调用时请不要填写result
                var FindNodeRecursion = function (nodes, recursionFieldName, callback, result) {
                    $(nodes).each(function (index, item) {
                        if (callback(index, item)) {
                            if (result) {
                                result.push(item);
                            } else {
                                result = [];
                            }
                        }

                        if (item[recursionFieldName] && recursionFieldName !== 'parents') {
                            $(item[recursionFieldName]).each(function (index, itemChild) {
                                if (item instanceof Array) {
                                    itemChild.parents = item;
                                } else
                                    itemChild.parents = [item];
                            });
                        }
                        FindNodeRecursion(item[recursionFieldName], recursionFieldName, callback, result);
                    });
                    return result;
                }


                FindNodeRecursion(items, 'nodes', function (index, item) {
                    var returnResult = false;
                    ///使用oldNodes同步节点的各个状态
                    if (options && options.oldNodes) {
                        $(options.oldNodes).each(function (optIndex, optItem) {
                            if (optItem.MID === item.MID) {
                                item.state = optItem.state;
                            }
                        });
                    }


                    $(defaultSelectedItems).each(function (innerIndex, innerItem) {
                        if (item.MID === innerItem.MID) {
                            var p = {};
                            for (var property in item) {
                                p[property] = item[property];
                            }
                            p.parents = null;
                            var originValue = DepartmentSelector.selectedMIDs;
                            if (originValue) {
                                var selectedCloneItem = JSON.parse(JSON.stringify(originValue));
                                selectedCloneItem.push(p);
                                DepartmentSelector.selectedMIDs = selectedCloneItem;
                            } else {
                                DepartmentSelector.selectedMIDs = [p];
                            }

                            if (item.state) {
                                item.state.selected = true;

                            } else {
                                item.state = { selected: true };
                            }
                            returnResult = true;
                        }

                    });



                    if (returnResult) {
                        FindNodeRecursion(item, 'parents', function (innerIndex, innerItem) {
                            $(innerItem).each(function (iinnerIndex, iinnerItem) {
                                if (iinnerItem.state)
                                    iinnerItem.state.expanded = true;
                                else {
                                    iinnerItem.state = { expanded: true };
                                }
                            });
                        });
                    }
                    return returnResult;

                });
                ///循环引用会导致不能JSON序列化
                ///移除parents 暂时解决这个问题
                FindNodeRecursion(items, 'nodes', function (index, item) {
                    if (item.parents) {
                        item.parents = [];
                    }
                });


                options.data = items;

                var tree = DepartmentSelector.instance = $('#' + treeViewID).treeview(options);
                $('#' + treeViewID).on('nodeSelected', DepartmentSelector.treeItemOnSelect);
                $('#' + treeViewID).on('nodeUnselected ', DepartmentSelector.treeItemOnSelect);
                DepartmentSelector.treeItemOnSelect();
                var event = {};
                event.tree = DepartmentSelector.instance;
                //        <%=this.OnTreeInited.IsNullOrWhiteSpaceE() ? "" : "if(" + this.OnTreeInited + "){(function (e) { " + this.OnTreeInited + "(e); })(event);}" %>
                if (doptions.onTreeInited) {
                    try {
                        doptions.onTreeInited(event);

                    } catch (e) {
                        console.error(e);
                    }
                }
                if (defaultSelectedItems.length > 0) {
                    //触发选中选项的后台事件
                    //                    $('#btn_chooseDep').click();
                    var selected = DepartmentSelector.instacne.treeview('getSelected');
                    $(selected).each(function (index, ele) {
                        var p = DepartmentSelector.instance.treeview('getParent', ele.nodeId);

                        if (p.state) {
                            p.state.expanded = true;
                        } else {
                            p.state = { expanded: true };
                        }


                    });
                }
                inited = true;
                $(".loadimg-div").hide();
            },
            error: function (err) {
                console.log(err);
            }
        });
    } else {
        //        var data = JSON.parse('<%=this.UserData%>');
        var options = doptions;
        //        options.data = data;
        DepartmentSelector.instance = $('#' + treeViewID).treeview(options);

    }




    DepartmentSelector.treeOnSearch = function (e) {
        if (e.target && DepartmentSelector.instance) {
            DepartmentSelector.instance.treeview('collapseAll', { silent: true });
            DepartmentSelector.instance.treeview('search', [e.target.value, {
                ignoreCase: true,     // case insensitive
                exactMatch: false,    // like or equals
                revealResults: true,  // reveal matching nodes
            }]);
        }
    }

    DepartmentSelector.searchOnKeypress = function (e) {
        e = window.event || e;
        var keycode = e.keyCode || e.which;
        if (keycode === 13) {
            e.target.onchange(e);
            e.cancelBubble = true;
            e.preventDefault();
        }

    }


    DepartmentSelector.treeItemOnSelect = function (event, data) {
        if (!inited) {
            return;
        }
        //            console.log(123);
        //            console.log(event);
        $(DepartmentSelector.instance.treeview('getSelected')).each(function (index, node) {

            if (node.CanNotBeSelected) {
                $(DepartmentSelector.instance.treeview('unselectNode', node, { slient: true }));
            }
        });
        var selectedNodes = DepartmentSelector.instance.treeview('getSelected');
        data = selectedNodes;
        //    <%=this.OnClientSelected.IsNullOrWhiteSpaceE() ? "" : "if(" + this.OnClientSelected + "){(function (data) { " + this.OnClientSelected + "(data); })(selectedNode);}" %>
        if (doptions.onClientSelected) {
            doptions.onClientSelected(selectedNodes);
        }


        DepartmentSelector.selectedMIDs = selectedNodes;
        //
        //        if ('<%=this.SelectPostBack%>'.toLowerCase() === 'true')
        //            $("#<%=btn_chooseDep.ClientID%>").click();

        DepartmentSelector.oldData = { oldNodes: DepartmentSelector.instance.treeview('getSelected') }
    }

    DepartmentSelector.clearSelection = function () {
        if (DepartmentSelector.instance.treeview) {
            $(DepartmentSelector.instance.treeview('getSelected')).each(function (i, e) {
                DepartmentSelector.instance.treeview('unselectNode', e);
            });
        }
       
    }
    DepartmentSelector.selectNodes = function (mids) {
        if (!DepartmentSelector.instance.treeview) {
            return;
        }
        if (mids) {
            var nodes = DepartmentSelector.instance.treeview('getNodes');

            $(mids).each(function (index, element) {
                var node = nodes.find((e, i) => e.MID == element);
                console.log(node);
                DepartmentSelector.instance.treeview('selectNode', node);
                var parent = DepartmentSelector.instance.treeview('getParent', node);
                DepartmentSelector.instance.treeview('expandNode', parent);
//                DepartmentSelector.instance.treeview('expandNode', node, { levels: node.level});


            });
        }
    }
}

//if (window["DepartmentSelector<%=this.ParentID%>"]) {
//    treeSelectorInit(window["DepartmentSelector<%=this.ParentID%>"].oldData);
//} else {
//    treeSelectorInit();
//}


