(function () {
    // ES6——声明和赋值（ let 和 const、变量的解构赋值）:
    // https://blog.csdn.net/joyce_lcy/article/details/84258590
    // 关于js funtion函数类型的解释:
    // http://c.biancheng.net/view/8099.html

    // native调用的js方法缓存池
    // 与之相对的window.bridge.nativeFuncs是js调用的native方法缓存池
    let events = {};
    // 方法id，用于js的callback方法存放events字典中对应的key的组成部分
    let functionId = 0;
    // 调试模式开关，可通过window.bridge.setDebug(bool)开关
    let debug = false;

    // 用于定义js的callback方法存放于events字典中对应key的组成部分的名称前缀
    // 真实的callback方法存放于events字典中对应key的前缀 + {fnName || callId}
    const CALLBACK_PREFIX = "bridge_callback_id_";
    const PROMISE_CALLBACK_PREFIX = "bridge_promise_callback_id_";

    /**
     * 是否为function的判断
     * @param {Object} func 要检测的对象
     * @returns {boolean}
     */
    function isFunction(func) {
        return typeof func === "function";
    }

    /**
     * 是否为对象的判断
     * @param {Object} obj 
     * @returns {Boolean}
     */
    function isObject(obj) {
        return ({}).toString.call(obj) === "[object Object]";
    }

    /**
     * 是否为空对象的判断
     * @param {Object} obj 
     * @returns {boolean}
     */
    function isObjectEmplty(obj) {
        let result = true;

        if (!isObject(obj)) {
            return result;
        }

        result = Object.keys(obj).length > 0;

        return result;
    }

    /**
     * 设置debug模式
     * @param {Boolean} isDebug 
     */
    function setDebug(isDebug) {
        debug = !!isDebug;
    }

    /**
     * 注册回调函数 Function 
     * 回调方法是js的方法，用于c# native去调用
     * 回调方法最终收归到events集合中，events集合就是native调用的js方法缓存池
     * @param {Object} params 
     * params.fnName 指定回调方法名称，如果指定了则作为回调方法在events字典中的key;否则用另外一套自动生成key
     * params.callback 回调方法整个定义语句
     * params.cache 回调是否可用于调用多次，即是否缓存
     * params.onPromise {Object} 注意理解！这个不是Promise，而是个包含 fetch方法返回的Promise的resolve和reject的一个对象而已！！
     * @returns {String} methodName 返回回调方法存储在events字典中对应的key
     */
    function registCallback(params) {
        // 赋值语句解构
        let { fnName = "", callback, cache, onPromise } = params;

        let callId, methodName;

        if (!fnName) {
            callId = ++functionId;
        }
        else {
            methodName = fnName;
        }

        if (isFunction(callback)) {
            if (!fnName) {
                methodName = CALLBACK_PREFIX + callId;
            }
            events[methodName] = (args) => {
                callback(args)
                if (!cache) {
                    removeCallback(methodName);
                }
            }
        }
        else {
            if (!fnName) {
                methodName = PROMISE_CALLBACK_PREFIX + callId;
            }
            // 这里的onPromise只是个普通的Object，而非Promise对象，请注意理解！
            // 这个resolve是让fetch方法返回的Promise做then调用的
            // 毕竟Promise的then要用到resolve传递的args
            // reject同理
            let onResolve = onPromise.resolve || function () { };
            let onReject = onPromise.reject || function () { };

            // 这边便于好理解，不再用onPromise这个变量了，因为本身Promise就不好理解
            let hope = { success, fail };
            hope.success = (args) => {
                // 这个resolve是让fetch方法返回的Promise做then调用的
                // 毕竟Promise的then要用到resolve传递的args
                onResolve(args);
                if (!cache) {
                    removeCallback(methodName);
                }
            }

            hope.fail = (args) => {
                onReject(args);
                if (!cache) {
                    removeCallback(methodName);
                }
            }

            events[methodName] = hope;
        }

        return methodName;
    }

    /**
     * 
     * @param {String} methodName 需要被移除的回调方法在events字典中对应的key
     * methodName一般由指定的fnName构成或者没指定情况下的prefix + callid构成
     */
    function removeCallback(methodName) {
        //delete events[methodName]
        events[methodName] && delete events[methodName];
    }

    /**
     * js调用native方法 - android和ios公用
     * 关键是通过 window.webkit.messageHandlers.dora.postMessage 
     * 这一步来让webview这个媒介得到message从而让native获取到，执行对应的viewmodel中的方法
     * @param {Object} params 
     * params.actionName String native的bridge方法名
     * params.data Object native的bridge方法参数
     * params.methodName Function js的回调方法名 - 即在events字典中存储此方法对应的key，用于native调用
     * params.hasCallback Boolean 是否有js的回调方法
     * params.cache Boolean 回调是否缓存
     */
    function callNative(params) {
        try {
            let { actionName, data = {}, methodName, hasCallback = false, cache = false, timer } = params;
            let result = { a: actionName, m: methodName, h: hasCallback, cache, timer, d: data };

            if (debug) {
                alert("call native width params: " + JSON.stringify(result));
            }

            // 这句话是精髓，{dora}这一节可以自定义！
            //window.webkit.messageHandlers.dora.postMessage(JSON.stringify(result));
            invokeCSharpAction(JSON.stringify(result));
        }
        catch (e) {
            window.bridge.report(e);
        }
    }

    // invokeCSharpAction方法是在HybridWebViewRenderer中定义且注入到WebView中的，此方法用于针对Android和ios统一收归
    // Android: function invokeCSharpAction(data){jsBridge.invokeAction(data);}
    // iOS: function invokeCSharpAction(data){window.webkit.messageHandlers.dora.postMessage(data);}
    if(!invokeCSharpAction || !isFunction(invokeCSharpAction)) {
        invokeCSharpAction = function () {};
    }

    // 定义bridge对象
    window.bridge = window.bridge || {};

    // nativeFuncs集合存放native方法(viewmodel中定义的)
    if (!window.bridge.nativeFuncs || !window.bridge.nativeFuncs.size) {
        window.bridge.nativeFuncs = new Set();
    }

    window.bridge = Object.assign(window.bridge, {
        VERSION: "1.0.0",
        /**
         * native 调用的 js收口方法，收口即统一
         * @param {methodName,data} params Object
         * params.methodName String 要调用的前端方法名，该方法需要前端先通过on|one注册 Function
         * params.data Object 调用前端方法时传给前端的参数
         */
        callJs(params) {
            try {
                let { methodName, data = {} } = JSON.parse(params);

                let fn = events[methodName] || {};
                if (isFunction(fn)) {
                    methodName && fn && isFunction(fn) && fn(data);
                }
                else {
                    let success = fn.success || "",
                        fail = fn.fail || "";
                    if (success) {
                        success(data);
                    }
                    // else if (fail) {
                    //     fail(data);
                    // }
                }
            }
            catch (e) {
                window.bridge.report(e);
            }
        },

        /**
         * native 回调的 js收口方法，收口即统一
         * @param {*} params 
         */
        // nativeCB: function(){} 跟 nativeCB(){} 有啥不一样呢？
        nativeCB: function (params) {
            try {
                params = JSON.parse(params);
                let { methodName = "", data = {}, timer } = params;
                timer && clearTimeout(timer);
                let fn = events[methodName] || "";
                if (isFunction(fn)) {
                    fn(data);
                }
                else {
                    let success = fn.success || "",
                        fail = fn.fail || "";
                    if (success) {
                        success(data);
                    }
                    // else if (fail) {
                    //     fail(data);
                    // }
                }
            }
            catch (e) {
                window.bridge.report(e);
            }
        },

        /**
         * 前端注册的方法，存放于events字典中
         * 这些方法用于native主动调用或回调js使用
         * @param {*} params 
         * @param params.fnName String 注册的方法名
         * @param params.cache Boolean 是否缓存该方法以便下次继续调用
         * @param params.callback Function 注册的方法
         */
        bind(params) {
            let { fnName, cache = false, callback } = params || {};
            let me = this;
            if (callback && isFunction(callback)) {
                events[fnName] = (data) => {
                    //!cache && removeCallback(fnName);
                    //？为什么这边不直接用外部函数removeCallback
                    !cache && me.off(fnName);
                    callback(data);
                }
            }
        },

        /**
         * bind重载之一；cache=true:缓存回调
         * @param {*} params 
         * @returns {on}
         */
        on(params) {
            params = params || {}
            params.cache = true;
            this.bind(params);
            return this;
        },

        /**
         * bind重载之一：cache=false:不缓存回调
         * @param {*} params 
         */
        once(params) {
            params = params || {}
            params.cache = false;
            this.bind(params);
            return this;
        },

        /**
         * 移除已注册的方法
         * @param {String} fnName methodName
         * @returns {off}
         */
        off(methodName) {
            events[methodName] && delete events[methodName];
            return this;
        },

        /**
         * 设置debug，沿用外部的方法
         */
        setDebug,

        /**
         * 获取debug
         * @returns 
         */
        getDebug() {
            return debug;
        },

        /**
         * 调用的actionName是否已定义
         * js调用native的方法池中是否存在actionName定义
         * @param {String} actionName 要检查的方法名
         * @param {Object}} callback 回调，非必传，不传则走promise
         * @returns {Promise<any>}
         */
        define: function (actionName, callback) {
            if (callback && isFunction(callback)) {
                let result = window.bridge.nativeFuncs.has(actionName);
                callback(result);
            }
            else {
                return new Promise((resolve) => {
                    let result = window.bridge.nativeFuncs.has(actionName);
                    resolve(result);
                })
            }
        },

        /**
         * 执行define方法
         * @returns 
         */
        has: function () {
            return this.define.apply(this, arguments);
        },

        /**
         * 
         * @param {Object} params 传递给c# native方法的参数
         * params.callback String 非必传，回调函数，没有可以不传，支持Promise
         * params.sync Boolean 非必传，是否为同步方法
         * params.cache Boolean 缓存当前回调函数以便下次继续使用，默认值：false - 执行完回调函数后立即删除该回调函数
         * params.timeout Number 超时时间，超过超时时间，直接callback，不再等待native回调js，默认值：30s
         */
        fetch: function (params) {
            let { actionName = "", data = {}, sync = false, cache = false, callback = "", timeout = 900000 } = params || {};

            let callNativeParams = { actionName, data, cache, methodName, hasCallback, timer };

            if (!actionName) {
                alert("params error, param actionName required!");
                return;
            }

            // 如果没有指定native方法名，且有回调方法的时
            if (!actionName && callback) {
                return callback({
                    code: 0,
                    message: "params error, param actionName required!"
                })
            }

            if ((callback && isFunction(callback))) {
                // 如果js调用native时带了callback回调，那js在调用真正的native方法之前就先把回调缓存到events字典中，
                // 以便后续native回调时，js可以从events字典中找到方法
                let methodName = registCallback({ callback, cache });
                callNativeParams.hasCallback = !!methodName;
                // js弱类型 orz
                // 上面params.callback还是方法体；下面params.callback就赋值为callback所在的events字典中的对应key|methodName
                // native执行完后，根据hasCallback判断
                // true就调用js的nativeCB方法，并将此处的callback(其实就是methodName|key)作为参数
                // 然后js就可以根据methodName|key从events字典中找出callback执行了
                // 原写法：params.hasCallback && (params.callback = methodName);
                callNativeParams.hasCallback && (callNativeParams.methodName = methodName); // 受够了弱语法导致代码阅读障碍，所以重新定义了传给callNative方法的参数对象

                callNativeParams.timer = setTimeout(function () {
                    // 销毁回调，防止native超时之后再回调
                    events[methodName] && delete events[methodName];
                    // 超时直接调用回调方法返回
                    callback({
                        code: 0,
                        message: "timeout!"
                    });
                }, timeout)

                callNative(callNativeParams);
            }
            else if (sync) {
                // 同步方法，没有回调
                callNativeParams.hasCallback = false;
                callNative(callNativeParams);
            }
            else {
                return new Promise(function (resolve, reject) {
                    let methodName = registCallback({
                        callback,
                        cache,
                        onPromise: { resolve, reject }
                    });

                    callNativeParams.hasCallback = !!methodName;
                    callNativeParams.hasCallback && (callNativeParams.callback = methodName);

                    callNativeParams.timer = setTimeout(function () {
                        // 销毁回调，防止native超时之后再回调
                        events[methodName] && delete events[methodName];
                        reject({
                            code: 0,
                            message: "timeout!"
                        });
                    }, timeout)

                    callNative(callNativeParams);
                })
            }
        },

        /**
         * 前端异常上报
         * @param {Object} e 错误对象
         * @returns {Promise<*>}
         */
        async report(e = {}) {
            let message = e.message || "",
                stack = e.stack || "",
                exist = await this.has("report"),
                params = { "actionName": "report", data: { message, stack } };

            if (exist) {
                if (debug) {
                    alert("异常错误信息提示：" + JSON.stringify(e));
                }
                return this.fetch(params);
            }
        }
    })
})()

export default {};