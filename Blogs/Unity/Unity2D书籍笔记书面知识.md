Unity2D游戏开发实例教程
一、Sprite对应的Inspector面板
1.第25页 
Packing Tag选项：用来将Sprite自动打包成一个Sprite Sheet（序列图），也就是常说的Texture Atlasing（纹理集） --- 貌似Unity Pro版本才提供此功能
Texture Atlasing：将多个单独的Sprite、纹理打包在一起，形成一个完整的Sprite或Texture，这样当每帧渲染多个对象时，可以大大提升性能
2.第25页
Pixels To Units选项：当导入的图像素材比场景大很多或者小很多，可以通过此选项（像素单位转换）设置等比缩放
在 Unity中，〝单位(Units)〞并不一定对应到屏幕上的像素，通常物体的大小都是相对于彼此的，可以假设单位为任何计量单位，如1 unit = 1米。对于 Sprite，Unity以像素为单位来确定大小。
在精灵渲染器(Sprite Renderer)的〝像素到单位(Pixels to Units)”默认值是100
如果将Pixels To Units设置为100，则代表 1 Unit 有 100 pixels。
3.第25页
Pivot选项：变换参照点
4.第26页
Filter Mode选项：决定了在为Sprite添加滤镜时，纹理图像的使用方式
a.Point点模式：效果最好，性能消耗最低；适用于距离较远，尺寸较小的图片；因为图片放大会呈现一个个色块
b.Bilinear双线性：柔化显示，让图片放大后不呈现色块；比Point性能消耗成本稍高
c.Trilinear三线性：更耗性能；使用Mipmaps多重贴图技术进行渲染优化；Mipmaps是在多纹理情况下的进行性能提升的一种技术，当某个纹理原理观察者，使用低分辨率渲染；当距离观察者越来越近时，自动切换成高分辨率渲染模式
5.第27页
Platform Setting选项：针对不同的目标平台做一些调整，勾选Override，便可以对默认配置进行调整（Max Size/Format）。前者代表分辨率，后者表示呈现图像的格式：Compressed(压缩)，16Bits，Truecolor(真彩色)

二、Camera对应的Inspector面板
1.Clear Flags：设置舞台中需要清除的部分
2.Cull Mask：选择Camera中渲染的图层
3.Size：正交2D模式下Camera的大小
假如要在 960 x 540的手机上要以 1:1像素的模式显示贴图，就需要去调整 Size值。
以 Sprite中，Pixels Per  Unit默认值为 100的状况下，高度 540 pixels刚好为 5.4 units，Size纪录的是 Camera中心点到顶端的距离，所以 5.4 units / 2 = 2.7 units。因此把 Size设成 2.7，就能看到一张 960 x 540 的图片铺满整个版面。
4.Clipping planes：设置Camera渲染的起始和终止距离
5.Viewport Rect：由Camera的X和Y坐标，以及Width和Height组成的矩形框
6.Depth：定义Camera渲染的顺序，属性较大的Camera要比较小的优先被渲染(一个Scene中可能多个Camera)
7.Render Depth：定义Camera的高级渲染模式
8.Target Texture：设置在Camera显示区域的内容以制定的渲染纹理输出（Pro版本功能）
9.Occlusion culling：对Camera可视范围之外的对象停止渲染
10.HDR：设置高级动态范围渲染

三、图层顺序
参考此链接：https://www.cnblogs.com/LeonLazuli/p/3961283.html
同一图层：设置Sprite的Sprite Renderer组件中的Order in layer属性
不同图层：设置各个图层的渲染顺序(depth)
注意只有摄像机的depth，sorting layer以及sorting order能决定图层的顺序
而gameobject所指定的layer，用来做两件事：
1.分类，好辨识，可以跟tag配合
2.可以配合摄像机做culling mask（剔除遮罩）
3.可以选择性投射光线
注意千万不要认为layer就能决定图层顺序了，实际上并不能，决定顺序的是上面的三个(depth,sorting layer,sorting order)，所以这意味着在同一个sorting layer中的对象可以碰撞，纵然这两个对象的layer不一样
https://docs.unity3d.com/Manual/Layers.html

四、贴图纹理集 - Texture Atlasing
第51页
将多个单独的图像打包在一起，形成要给单独的图像。
为了创建Texture Atlas，Unity增加了Sprite Packers打包工具，Pro版本支持。
工具栏选择Window -> Sprite Packer，开启Sprite Packers窗口，点击左上角的Pack按钮，会使用默认设置将所有的纹理打包成一个纹理集。

五、Sprite Renderer组件的Inspector面板
Sprite：Sprite资源
Color：一种滤镜，会将所选颜色叠加到当前的Sprite对象上，默认白色，保持原始颜色
Material：为图像设置纹理材质，影响素材的渲染效果
Sorting Layer：可以将当前选中的一个或者多个Sprite作为一组，放置到一个图层中
Order in Layer：设置当前所选对象的在同一图层中相对于这一图层的其他对象的显示顺序

Toolbar上右上角Layers下拉菜单 -> Edit Layers
Sprite对应的Inspector面板最上面的Layer选项可以下来选择上一步Edit的Layers
为了让多个摄像机只能看到自己的图层，在Camera的Inspector面板上设置Culling Mask(剔除遮罩)属性，选择对应的图层

六、为Sprite添加动画 - Animation面板
第57页
1.Scene或者Hierarchy面板中选择Sprite对象，点击Window -> Animation，开启Animation Editor
2.点击Create New Clip下拉菜单选择Create New Clip
3.设置动画名称并Save
4.点击Add Curve
5.选择目标组件（对哪个组件进行动画，就选哪个）
6.认清Sample属性控制着每秒播放的帧数，即帧频FPS
之后的操作如同Visual Studio Blend的动画操作，关键帧动画

七，Sprite状态与动画的维系 - Animator面板
第60页
当某个游戏条件发生时，Animator面板可以实现多个动画之间的切换。比如当人物 进入跳跃状态时，Unity要自动播放Jump动画。
游戏人物的状态与动画之间的对应关系在Animator面板中定义
1.Window -> Animator 开启Animator面板
2.在Project面板中选中某个动画Animation，拖入到Animator面板中
3.右击Anystate状态选择Make Transition选项，然后点击2中的动画，这样就完成了从Anystate状态过度到动画状态
4.在动画状态上右击，选择Set as Default，运行就会自动运行动画效果

七、游戏中的输入
1.Unity输入管理器
通过Unity输入管理器中的标准输入方式来设置(映射)游戏控制配置
打开Unity输入管理器：工具栏Edit -> Project Settings -> Input
好处：
输入管理器可以让开发者简单的在代码中使用默认的控制键
输入管理器可以让玩家按照自己的喜好设置控制方式

八、导入纹理（精灵）
1.纹理资源本身尺寸最好是2的次方
2.导入后在Inspector面板设置最大尺寸为4096，方便以后游戏场景对纹理资源的缩放

九、区块创建背景（Tilesets）
四大元素和规则：无缝拼接，边角元素，网格，预制
网格：Edit -> Snap Settings -> 根据需求设定网格大小(根据每块Tile的大小) -> 按住Ctrl，拖动区块图像便可以自动对齐网格了

十、UGUI自适应屏幕
https://blog.csdn.net/jasper_ding/article/details/52938999
https://blog.csdn.net/u014314850/article/details/64439671
http://www.cnblogs.com/flyFreeZn/p/4073655.html
https://blog.csdn.net/su9257/article/details/53945748

十一、Platform Effector 2D - 物理引擎Effector
适合于做跳跃
Add -> Component -> Effects 包括很多特定的Effect组件
相关链接：http://tieba.baidu.com/p/4310232254
https://blog.csdn.net/supersunstar/article/details/55049437
https://blog.csdn.net/yupu56/article/details/78658544

Box2D引擎，包括Physics2D
Add -> Component -> Physics 2D 这里面包括Rigidbodies(刚体)，Colliders(碰撞器)，Joints(关节)
关节组件举例：Distance Joint 2D
关节的作用：可以将多个对象关联起来，例如一扇门和一堵墙，或者地板和楼梯。又或者机器人和它的手臂。还可以设置关节连接对象直接是否发生碰撞等等

Rigidbody 2D组件各个选项说明：
https://blog.csdn.net/SerenaHaven/article/details/78851089
a.mass：对象刚体的质量
b.linear drag：对象刚体位置移动过程中受到的阻力
c.Angular Drag：对象刚体旋转过程中，在角速度方向受到的阻力
d.Gravity scale：对象刚体本身受到的重力作用比例
e.fixed angle：开启此项，刚体收到外力作用时，角度不会发生旋转
f.is kinematic：确定刚体是否接受动力学模拟
功能区别：
a. useGravity属性是确定刚体是否接受重力加速度的感应。
b. isKinematic属性是确定刚体是否接受动力学模拟（默认即不勾选的时候是false），此影响不仅包括重力感应，还包括速度、阻力、质量等的物理模拟。
举例说明：如图10-19所示，A和B为两个刚体物体，A在B的正上方，开始时A和B的重力感应都被关闭，都处于静止状态，且接受动力学模拟即isKinematic为false。现在开启A的重力感应，则A从1处开始加速下落，当下落到2处时，关闭A的重力感应，但isKinematic依然为false（即接受动力学模拟），则A将以当前速度匀速下落。但是此时若关闭物理感应，即isKinematic=true，则A将立即停止移动。当A与B发生碰撞时，若B的重力感应依然关闭，但接受动力学模拟，即isKinematic=false，则根据动量守恒B将产生一个向下的速度。但是若关闭B物体的动力学模拟，即isKinematic=true，则B保持静止，不会因受到A的碰撞而下落。
在Unity中在刚体不与其他物体接触的情况下velocity的值只与Gravity、drag及Kinematic有关，与质量mass及物体的Scale值无关。
g.interpolate：当物理引擎更新时，所用的插值运算方法（第177页）
h.sleeping mode：开启后，当刚体睡眠(处于静止状态时)，物理引擎将不对它进行模拟计算，以节省处理器开销（第178页）
i.collision detection：设置刚体之间或与其他对象的碰撞检测方式
discrete：只在物理模拟计算更新时，检测某个对象的碰撞器与另一个碰撞器的碰撞
continuous：在每次物理模拟计算更新时，进行多次碰撞检测，避免某些特殊情况下出现纰漏，比如高速运行的刚体。

岩浆或者洪水向上涌动的过程：第208页

重点：
没有碰撞体的刚体彼此之间相互穿过

十二、场景缩放
https://blog.csdn.net/h5q8n2e7/article/details/50633654

十三、碰撞检测
https://blog.csdn.net/oncruise/article/details/80070926

十四、Unity图片资源类型的设定和材质
https://www.jianshu.com/p/be420cf04564


以下内容十分重要：https://docs.unity3d.com/Manual/class-LineRenderer.html
十五、Renderer
a.Line Renderer
material：材质
material.renderQueue：材料的渲染队列
setVertexCount/numPositions：设置线段数，但是例子看来应该是节点数
setWidth/startWidth,endWidth：设置线条的开始和结束的宽度

十六、Mesh
自定义的时候：
a.Mesh的设定以对应Render的一个单位来说
b.Mesh的最小单位是一个三角形，所以定义Mesh就是告诉它如何用三角形绘制出需要的网格即可
vertices：Mesh各个顶点的向量（三维）
uv：Mesh各个顶点在二维坐标系中的相对坐标
triangles：通过vertices和uv的设置和对应关系(Vertices数组和UV数组对应的相对位置顶点一致，则对应的各自顶点的index必须保持一致)，通过这些顶点绘制三角形（通常是一个数组，三个一组进行定义三角形）

十七、Shader


十八、Lens Flare光晕效果


十九、Particle System 粒子系统
https://blog.csdn.net/ios_song/article/details/52836503

二十、解决UGUI中ScrollView下嵌套Button时Button难以响应的问题
这个灵敏度通过EventSystem组件的Drag Threshold参数来指定
https://blog.csdn.net/serenahaven/article/details/80845994

二十一、敌人AI
1.敌人能自动跟随主角
2.敌人模型一共要有四个动作：Idle（空闲）,Run，Attack，Death
3.要求敌人在合适的时机能够做出合适的动作

二十二、SpriteMask组件
SpriteMask影响的是Sprite Renderer组件
Mask默认的范围是整个Sprite，但如果启用Custom Range，就可以设置SpriteMask的范围了。
Mask Interaction：Visible Outside Mask：镂空效果；Visible Inside Mask：凸显效果
 Visible Under Mask等等


概念辨析：https://blog.csdn.net/Lilinyuanvr/article/details/54377271
两个物体A和B，如下情况：
1.A是刚体，没有碰撞器；B是刚体，没有碰撞器；A和B相撞，是否会彼此穿过
2.A是刚体，有碰撞器；B是刚体，没有碰撞器；A和B相撞，是否会彼此穿过
3.A是刚体，有碰撞器；B是刚体，有碰撞器；A和B相撞，是否会彼此穿过
4.A和B都不是刚体，但A和B都有碰撞体
结论：1和2会，3和4不会
没有碰撞器，物体就相当于空气；而刚体只是让物体遵循物理原则，比如受到重力影响下降
只有作用的两者同时具有碰撞器(是不是刚体无所谓)，才不能彼此穿过，要不然都能穿过
刚体是表示物体可以受到力的作用；而碰撞器是让物体具有碰撞的能力 

碰撞产生的条件（即可调用OnCollisionEnter/Stay/Exit）
1.必须双方都要有碰撞器，发生碰撞的是碰撞器。 
2.运动的一方一定要有刚体，另一方有无刚体无所谓。 
注：如果运动的一方无刚体，它去碰撞静止的刚体，相当于没有装上。（无法调用OnCollisionEnter/Stay/Exit）

碰撞器与触发器的区别
1.碰撞器是触发器的载体，而触发器只是碰撞器身上的一个属性。 
2.当Is Trigger=false时，碰撞器根据物理引擎引发碰撞，产生碰撞的效果，可以调用OnCollisionEnter/Stay/Exit函数； 
3.当Is Trigger=true时，碰撞器被物理引擎所忽略，没有碰撞效果，可以调用OnTriggerEnter/Stay/Exit函数。

何时使用触发器
如果既要检测到物体的接触又不想让碰撞检测影响物体移动或要检测一个物件是否经过空间中的某个区域这时就可以用到触发器。

相关实验结果：
碰撞事件 - 即调用OnCollisionEnter/Stay/Exit函数
1.控制A（刚体加碰撞体）撞击 静止的B（只有碰撞体），双方能收到碰撞事件。 
2.控制B（只有碰撞体）撞击 静止的A（刚体加碰撞体），双方收不到碰撞事件。

触发事件（均为控制A撞B）- 即OnTriggerEnter/Stay/Exit函数
1.A（碰撞体），B（没有碰撞体，无论有没有刚体），没有触发事件。 
2.A（碰撞体），B（碰撞体），没有触发事件。 
3.A（碰撞体和刚体，开启IsTrigger），B（碰撞体，关闭IsTrigger），双方都能收到触发事件。 
4.A（碰撞体和刚体，关闭IsTrigger），B（碰撞体，开启IsTrigger），双方都能收到触发事件。 
5.A （碰撞体，关闭IsTrigger），B（碰撞体和刚体，开启IsTrigger），没有触发事件。 
6.A （碰撞体，开启IsTrigger），B（碰撞体和刚体，关闭IsTrigger），没有触发事件。