using Dora.Xamarin.WebView.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Xamarin.WebView.ViewModel.Descriptors
{
    /// <summary>
    /// Native的Action方法的表达树
    /// 对表达式树的方便理解 => 它是一种结构和类型的表达，每个表达节点都非数据实体，你可以理解为占位符，最终需要真实的数据来填充
    /// 相关知识链接：
    /// https://www.cnblogs.com/cmliu/p/13246185.html
    /// https://blog.csdn.net/u012329294/article/details/77618528
    /// </summary>
    public class JSBridgeNativeHandlerBuilder
    {
        // 下面这一大堆retrunValueXXXMethod存在的原因：
        // 1. js 调用 natvie 定义的action方法之后，action方法返回可能是void,Task,Task<T>,ValueTask<T>
        // 2. 需要把以上native的action方法返回值返回给js呀，所以我们必须要将action的结果序列化成string，回传给js前端
        // 3. 回传的过程是native向前端js调用第1步传过来的回调方法(key)，前端js通过callback或者promise接收到数据进行下一步操作
        private static readonly MethodInfo getReturnValue4VoidMethod;
        private static readonly MethodInfo getReturnValue4TaskMethod;
        private static readonly MethodInfo getReturnValue4ValueTaskMethod;

        private static readonly MethodInfo getReturnValueMethod;
        private static readonly MethodInfo getReturnValue4TaskOfResultMethod;
        private static readonly MethodInfo getReturnValue4ValueTaskOfResultMethod;

        private static readonly MethodInfo deserializeMethod;
        private static readonly MethodInfo getServiceMethod;
        private static readonly PropertyInfo serviceProviderProperty;

        static JSBridgeNativeHandlerBuilder()
        {
            getReturnValue4VoidMethod = GetMethodInfo(() => GetReturnValue4Void());
            getReturnValue4TaskMethod = GetMethodInfo(() => GetReturnValue4Task(default));
            getReturnValue4ValueTaskMethod = GetMethodInfo(() => GetReturnValue4ValuTask(default));

            getReturnValueMethod = GetMethodInfo(() => GetReturnValue<string>(default, default)).GetGenericMethodDefinition();
            getReturnValue4TaskOfResultMethod = GetMethodInfo(() => GetReturnValue4Task<string>(default, default)).GetGenericMethodDefinition();
            getReturnValue4ValueTaskOfResultMethod = GetMethodInfo(() => GetReturnValue4ValueTask<string>(default, default)).GetGenericMethodDefinition();

            serviceProviderProperty = typeof(BaseViewModel).GetProperty("ServiceProvider", BindingFlags.Instance | BindingFlags.NonPublic);
            // json序列化默认使用newtonjson
            // deserializeMethod = typeof(JsonConvert).GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
            deserializeMethod = GetMethodInfo<INewtonJsonSerializer>(_ => _.Deserialize(default, default));
            // 下面两行等同；前者由反射直接取MethodInfo；后者先使用单行Lambda创建表达式，然后通过表达式反过来拿MethodInfo
            // getServiceMethod = typeof(IServiceProvider).GetMethod("GetService", new[] { typeof(Type) });
            getServiceMethod = GetMethodInfo<IServiceProvider>(_ => _.GetService(default));
        }

        /// <summary>
        /// JsBridge Action的表达式树构建 -> Compile() -> 委托实现
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public static Func<BaseViewModel, string, ValueTask<string>> BuildJSBridgeNativeHandler(JSBridgeActionDescriptor actionDescriptor)
        {
            // 创建参数类型为BaseViewModel的表达式树
            var baseViewModel = Expression.Parameter(typeof(BaseViewModel));
            Console.WriteLine($"BuildJSBridgeNativeHandler-baseViewModel: {baseViewModel}");
            // 创建类型转换表达式树 - 表达 BaseViewModel的类型转换成BaseViewModel的子类型
            var viewModel = Expression.Convert(baseViewModel, actionDescriptor.ViewModelType);
            Console.WriteLine($"BuildJSBridgeNativeHandler-viewModel: {viewModel}");
            // 创建参数类型为string的表达式树
            var input = Expression.Parameter(typeof(string));
            Console.WriteLine($"BuildJSBridgeNativeHandler-input: {input}");
            // 表达 获取viewModel的属性 - ServiceProvider
            var serviceProvider = Expression.Call(viewModel, serviceProviderProperty.GetMethod);
            Console.WriteLine($"BuildJSBridgeNativeHandler-serviceProvider: {serviceProvider}");
            // 表达 通过上一步获取的serviceProvider去DI根据ISerializer类型获取对应的实现服务
            var serializerInstance = Expression.Call(serviceProvider, getServiceMethod, Expression.Constant(typeof(INewtonJsonSerializer)));
            Console.WriteLine($"BuildJSBridgeNativeHandler-serializerInstance: {serializerInstance}");
            // 表达 类型转换
            var serializer = Expression.Convert(serializerInstance, typeof(INewtonJsonSerializer));
            Console.WriteLine($"BuildJSBridgeNativeHandler-serializer: {serializer}");

            MethodCallExpression methodCall;
            if (actionDescriptor.Parameter != null)
            {
                // 将js call native传过来的json字符串转换成jsbridge action方法中对应的参数类型
                var argument = Expression.Call(null, deserializeMethod, input, Expression.Constant(actionDescriptor.Parameter.ParameterType));
                Console.WriteLine($"BuildJSBridgeNativeHandler-argument: {argument}");
                // 表达 参数类型的类型转换
                var convertedArgument = Expression.Convert(argument, actionDescriptor.Parameter.ParameterType);
                Console.WriteLine($"BuildJSBridgeNativeHandler-convertedArgument: {convertedArgument}");
                // 表达 执行viewModel的action方法，并传入对应所需的参数
                methodCall = Expression.Call(viewModel, actionDescriptor.MethodInfo, convertedArgument);
                Console.WriteLine($"BuildJSBridgeNativeHandler-methodCall: {methodCall}");
            }
            else
            {
                // 表达 执行viewModel的action方法
                methodCall = Expression.Call(viewModel, actionDescriptor.MethodInfo);
                Console.WriteLine($"BuildJSBridgeNativeHandler-methodCall: {methodCall}");
            }

            var returnType = actionDescriptor.ReturnType;
            Expression getReturnValue;
            if (returnType == typeof(void))
            {
                getReturnValue = Expression.Block(new Expression[] { methodCall, Expression.Call(getReturnValue4VoidMethod) });
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }
            else if (returnType == typeof(Task))
            {
                // 表达 执行GetReturnValue4Task方法，并将methodCall执行结果(Task)传入，最终native的action其实是Task，但给js要是string(null)
                getReturnValue = Expression.Call(getReturnValue4TaskMethod, methodCall);
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }
            else if (returnType == typeof(ValueTask))
            {
                // 表达 执行GetReturnValue4ValueTask方法，并将methodCall执行结果(ValueTask)传入，最终native的action其实是ValueTask，但给js要是string(null)
                getReturnValue = Expression.Call(getReturnValue4ValueTaskMethod, methodCall);
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }
            else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // 表达 执行GetReturnValue4TaskOfResult方法，并将methodCall执行结果(Task<T>)传入
                // 在其中强制规定了GetReturnValue4TaskOfResult传入的类型T，最终native的action其实是Task<T>，但给js要是serializer.Serializer(T)
                getReturnValue = Expression.Call(getReturnValue4TaskOfResultMethod.MakeGenericMethod(returnType.GetGenericArguments()[0]), methodCall, serializer);
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }
            else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                // 表达 执行GetReturnValue4ValueTaskOfResult方法，并将methodCall执行结果(ValueTask<T>)传入
                // 在其中强制规定了GetReturnValue4TaskOfResult传入的类型T，最终native的action其实是Task<T>，但给js要是serializer.Serializer(T)
                getReturnValue = Expression.Call(getReturnValue4ValueTaskOfResultMethod.MakeGenericMethod(returnType.GetGenericArguments()[0]), methodCall, serializer);
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }
            else
            {
                getReturnValue = Expression.Call(getReturnValueMethod.MakeGenericMethod(returnType), methodCall, serializer);
                Console.WriteLine($"BuildJSBridgeNativeHandler-getReturnValue: {getReturnValue}");
            }

            // 尘埃落定
            // 下面表达式的意思是构建一个形如Func<BaseViewModel,string,ValueTask<string>>的Lambda表达式
            // 用上面我们定义的变量去创建，那么可以这么理解：(baseViewModel, input) => getReturnValue
            // 注意理解：后面参数的定义和顺序 不是和 Func里的参数和结果的定义顺序 对应的！！！
            // getReturnValue - 对应的是根据拿到传入的baseViewModel和input最终运行得出的结果，结果类型是ValueTask<string>，这个结果是给前端js(js调用native)的返回结果
            // baseViewModel - 对应BaseViewModel
            // input - 对应string
            return Expression.Lambda<Func<BaseViewModel, string, ValueTask<string>>>(getReturnValue, baseViewModel, input).Compile();
        }

        /// <summary>
        /// 针对下面
        /// 【将ViewModel中定义的 【注册在DI中的服务类】属性|字段，赋予DI中对应的实现类，并返回ViewModel】
        /// 的表达式树构建 -> Compile() -> 委托实现
        /// </summary>
        /// <param name="viewModelDescriptor"></param>
        /// <returns></returns>
        public static Action<BaseViewModel> BuildInjectedMembersInitializer(ViewModelDescriptor viewModelDescriptor)
        {
            var baseViewModel = Expression.Parameter(typeof(BaseViewModel));
            var serviceProvider = Expression.Call(baseViewModel, serviceProviderProperty.GetMethod);
            var viewModel = Expression.Convert(baseViewModel, viewModelDescriptor.ViewModelType);
            var blocks = new List<Expression>();

            foreach (var injectedMember in viewModelDescriptor.InjectedMembers)
            {
                if (injectedMember.Field != null)
                {
                    var service = Expression.Call(serviceProvider, getServiceMethod, Expression.Constant(injectedMember.Field.FieldType));
                    var convetedService = Expression.Convert(service, injectedMember.Field.FieldType);
                    var field = Expression.Field(viewModel, injectedMember.Field);
                    blocks.Add(Expression.Assign(field, convetedService));
                }

                if (injectedMember.Property != null)
                {
                    var service = Expression.Call(serviceProvider, getServiceMethod, Expression.Constant(injectedMember.Property.PropertyType));
                    var convertedService = Expression.Convert(service, injectedMember.Property.PropertyType);
                    var property = Expression.Property(viewModel, injectedMember.Property);
                    blocks.Add(Expression.Assign(property, convertedService));
                }
            }

            return Expression.Lambda<Action<BaseViewModel>>(Expression.Block(blocks), baseViewModel).Compile();
        }

        #region private methods
        private static MethodInfo GetMethodInfo<TTarge>(Expression<Action<TTarge>> expression)
        {
            // 参数用到的知识：Expression<Func<int, int>> expression = a => a + 100; - 使用单行Lambda创建表达式
            return ((MethodCallExpression)(expression.Body)).Method;
        }

        private static MethodInfo GetMethodInfo(Expression<Func<ValueTask<string>>> expression)
        {
            return ((MethodCallExpression)(expression.Body)).Method;
        }

        private static ValueTask<string> GetReturnValue4Void() => new ValueTask<string>((string)null);

        private static ValueTask<string> GetReturnValue4Task(Task task) => GetReturnValue4ValuTask(new ValueTask(task));

        private static ValueTask<string> GetReturnValue4ValuTask(ValueTask task)
        {
            if (task.IsCompletedSuccessfully)
            {
                return new ValueTask<string>((string)null);
            }
            return new ValueTask<string>(EvalTask(task.AsTask()));

            static async Task<string> EvalTask(Task task)
            {
                await task;
                return null;
            }
        }

        private static ValueTask<string> GetReturnValue<T>(T result, INewtonJsonSerializer serializer) => GetReturnValue4ValueTask(new ValueTask<T>(result), serializer);

        private static ValueTask<string> GetReturnValue4Task<T>(Task<T> task, INewtonJsonSerializer serializer) => GetReturnValue4ValueTask(new ValueTask<T>(task), serializer);

        private static ValueTask<string> GetReturnValue4ValueTask<T>(ValueTask<T> task, INewtonJsonSerializer serializer)
        {
            if (task.IsCompletedSuccessfully)
            {
                return new ValueTask<string>(EvalResult(task.Result, serializer));
            }
            return new ValueTask<string>(EvalTask(task.AsTask(), serializer));

            static string EvalResult<T2>(T2 result, INewtonJsonSerializer serializer)
            {
                if (result is string str)
                {
                    return str;
                }
                return serializer.Serialize(result);
            }

            static async Task<string> EvalTask<T1>(Task<T1> task, INewtonJsonSerializer serializer)
            {
                var result = await task;
                return EvalResult(result, serializer);
            }
        }
        #endregion
    }
}
