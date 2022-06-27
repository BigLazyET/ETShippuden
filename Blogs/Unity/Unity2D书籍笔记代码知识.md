Unity2D游戏开发实例教程
一、输入
1.Input类 第99页
Input.GetAxis()：获取坐标信息，保证数据平滑连续；减少代码量和复杂程度
Input.GetButton()：获取按钮信息，通常在Update()函数中调用，按钮状态只有在帧更新后重置
Input.touches：追踪移动触屏设备多点触控输入
Input.acceleration：追踪加速信息
Input.gyro：追踪地理位置
input.touchCount：检测是否有输入

2.创建按钮
使用Unity中内置的GUILayout类和它的Button()函数创建按钮，按钮文本和大小以参数形式。

二、内置函数
1.OnGUI()函数
用来处理GUI事件，包括GUI的创建，外观变化和功能触发。是Unity内置的诸多回调函数的一部分。和Start()，Update()一样，由Unity自动调用
一帧当中可能会多次调用OnGUI函数，这取决于功能触发的频率，不过每一个GUI事件只会调用一次OnGUI函数
2.Start()函数
游戏第一帧运行时，Start函数会立刻调用，并且只被调用一次
通常游戏或程序会按照每秒播放固定的帧数来运行，在运行时，只有第一帧调用Start函数
初始化一些变量可以放在Start函数中，尽量不要放太过耗时的任务
3.Update()函数
游戏的每一帧都会调用Update函数，所以Update函数每秒会被调用多次。
当我们持续执行某个动作时，可以使用Update函数，例如敌人不停的来回移动
4.InvokeRepeating(...)函数
按照指定的频率重复调用某个函数
例子：InvokeRepeating("enemySpawn"，3，3) -- 在游戏开始3秒后，按照每次间隔3秒的频率，重复调用enemySpawn函数
以下的解释：https://blog.csdn.net/pixel_nplance/article/details/80759122
5.OnAwak()
6.OnEnable()
7.OnDisable()
8.OnDestroy()

三、协同程序

四、游戏控制方式
1.Raycasting（透射法） - 第101页
检测用户手指或者鼠标是否与屏幕的GameObject发生触碰，返回一个3D向量ray：
Camera.main.ScreenToWorldPoint(pos)
pos：用户手指在触屏上的坐标或者鼠标当前点击的位置坐标
2.拿到1中的ray结果，取出ray.x|ray.y获取用户手指或者鼠标当前位置的x,y的坐标，并创建2D向量touchPos，然后传给下面的方法，检测碰撞其是否覆盖这个点(touchPos)，此结果返回bool：
Physics2D.OverlapPoint(touchPos);
3.判断当前平台
Application.platform
(RuntimePlatform.Androd | RuntimePlatform.IPhonePlayer)

五、各个GameObject之间的依附
第79页

六、关卡
获取当前加载关卡的名称：Application.LoadLevelName()
加载关卡：Application.LoadLevel(...)
   Application.LoadScene(...)
https://blog.csdn.net/h522532768/article/details/53383603