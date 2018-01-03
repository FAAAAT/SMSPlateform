if ($) {
    if (!window.JSHelper) {
        window.JSHelper = {
            //从当前对象向顶层对象寻找符合筛选条件的对象并返回所有匹配的对象
            //element 要进行搜索的element
            //selector 要匹配的selector字符串 参考jquery中的selector定义
            //result 在代码中调用时请不要填写这个参数
            GetParents: function (element, selector, result) {
                var returnRe = [];
                var re = result || [];
                var parent = $(element).parent(selector);
                if (parent.length === 0) {
                    try {
                        if (!element) {
                            return [];
                        }
                        parent = JSHelper.GetParents(element.parentElement, selector, re);
                    }
                    catch (e) {
                        console.error(e);
                    }
                    if (parent.length !== 0) {
                        if (parent.each)
                            parent.each(function (index, ele) {
                                returnRe.push(ele);
                            });
                        else if (parent.forEach) {
                            parent.forEach(function (ele) {
                                returnRe.push(ele);
                            });
                        }
                    }
                } else {
                    if (parent.each)
                        parent.each(function (index, ele) {
                            returnRe.push(ele);
                        });
                    else if (parent.forEach) {
                        parent.forEach(function (ele) {
                            returnRe.push(ele);
                        });
                    }
                }
                return returnRe;
            }
            //返回一个function对象,该对象中的this关键字只想object所指定的对象
            //object this指向的对象
            //func 要进行this指向替换的函数
            //
            //注意,在调用bind时 可以在object和func参数后附加更多的参数，如果在bind时指定了更多的参数 在调用返回的func时，
            //无论是否指定参数都会自动调用bind时指定的参数。bind时指定的参数会在调用返回的func时优先入栈 也就是说，
            //func的参数列表中bind指定的参数永远是在调用返回的func时指定的参数之前的。
            //
            , Bind: function (object, func) {
                var args = Array.prototype.slice.call(arguments).slice(2);
                return function () {
                    var innerArgs = arguments;
                    args.push.apply(args, innerArgs);
                    return func.apply(object, args);
                }
            }
        };
    }
} else {
    console.error('JSHelper:We need Jquery!');
}