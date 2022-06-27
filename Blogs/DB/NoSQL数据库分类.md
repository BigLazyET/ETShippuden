NoSQL数据库分类
---------

* [参考文档](https://www.mongodb.org.cn/tutorial/2.html)


类型 | 部分代表 | 特点
---|---|---
列存储 | Hbase、Cassandra、Hypertable | 按列存储数据的。最大的特点是方便存储结构化和半结构化数据，方便做数据压缩，对针对某一列或者某几列的查询有非常大的IO优势
文档存储 | MongoDB、CouchDB | 文档存储一般用类似json的格式存储，存储的内容是文档型的。这样也就有有机会对某些字段建立索引，实现关系数据库的某些功能
key-value存储 | Tokyo Cabinet/Tyrant、Berkeley DB、MemcacheDB、Redis | 可以通过key快速查询到其value。一般来说，存储不管value的格式，照单全收。（Redis包含了其他功能）
图存储 | Neo4J、FlockDB | 图形关系的最佳存储。使用传统关系数据库来解决的话性能低下，而且设计使用不方便
对象存储 | db4o、Versant | 通过类似面向对象语言的语法操作数据库，通过对象的方式存取数据
xml数据库 | Berkeley DB XML、BaseX | 高效的存储XML数据，并支持XML的内部查询语法，比如XQuery,Xpath

#### 其他

* 关系型数据库的ACID原则（原子性，一致性，独立性，持久性）
* 分布式系统的CAP定理（一致性，可用性，分割容忍） - 根据需求最好满足其一或其二或协调
* Base理论是NoSQL数据库通常对可用性和一致性的弱要求原则：基本可用、最终一致性，软状态/柔性事务(可以理解为无连接)