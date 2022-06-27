Etcd

目标：构建一个高可用 分布式 键值(kv)数据库
并可以解决各种服务的配置信息的管理分享、服务发现

一、特征概述
1. 简单可靠，API丰富
2. 支持HTTPS方式访问
3. 支持并发10k/s的写操作
4. 支持分布式结构，基于Raft的一致性算法
5. 通过http轮询，监听网络变化

二、重要应用
etcd是一个开源的分布式键值对存储工具。在每个coreos节点上面运行的etcd，共同组建了coreos集群的共享数据总线。etcd可以保证coreos集群的稳定，可靠。当集群网络出现动荡，或者当前master节点出现异常时，etcd可以优雅的进行master节点的选举工作，同时恢复集群中损失的数据。
分布在各个coreos节点中的app，都可以自由的访问到etcd中的数据。最常见的场景就是etcd存储cluster的metadata，cache setting, cluster logging, feature flags等。
一般情况下，用户使用 etcd 可以在多个节点上启动多个实例，并添加它们为一个集群。同一个集群中的 etcd 实例将会保持彼此信息的一致性。

三、安装
1. 二进制文件下载（curl + tar + cd）
    https://yeasy.gitbooks.io/docker_practice/etcd/install.html
2. 解压将主要的两个文件放到系统可执行目录（/usr/local/bin）：
    etcd：服务主文件
    etcdctl：提供给用户的命令客户端
    sudo cp etcd* /usr/local/bin
3. 因为etcd一方面要处理用户请求，一方面要处理集群成员之间的通信，所以它具备两个端口，一般来说：2379端口 - 处理客户端请求；2380端口 - 处理集群各成员之间的通信
4. 启动etcd：etcd
5. etcdctl检查etcd服务是否启动成功
    ETCDCTL_API=3 etcdctl member list
    ETCDCTL_API=3 etcdctl put testkey "hello world"
    etcdctl get testkey
6. 镜像方式安装并启动运行
    https://yeasy.gitbooks.io/docker_practice/etcd/install.html

四、使用etcdctl
1. etcd是一个命令行客户端，提供命令，供用户直接跟etcd打交道，而不用http
2. 这种方式很方便，适合于测试和初学，且原理与Http API一致，相对的
3. 数据库操作：CRUD
    etcd在键的组织上也采用了层次化的空间结构，比如可以是
    	testkey：此时实际上放在根目录/下
 cluster1/node2/testkey：创建相应的目录结构
4. 非数据库操作：
    watch：监测
    member：etcdctl member [具体命令]
      具体命令：list，add，update，remove可以列出、添加、更新、删除etcd实例到etcd集群中





































