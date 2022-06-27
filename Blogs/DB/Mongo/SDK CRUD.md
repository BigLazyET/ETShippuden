MongoDB .NET SDK
----

==数据库操作==

### 一、获取数据库实例
* the GetDatabase method on client
* 如果数据库不存在，会在第一次使用时自动创建
```
// 这个database变量就代表着对于"foo"数据库的引用
var database = client.GetDatabase("foo");
```

### 二、获取集合实例
* the GetCollection<TDocument> method on database
* 如果集合不存在，会在第一次使用时自动创建
```
// 这个collection变量就代表着对于foo数据库中bar集合的引用
var collection = database.GetCollection<BsonDocument>("bar");
```

### 三、插入文档

#### 1. 插入单个文档到集合

##### 考虑以下的Json文档：
```
{
    "name": "MongoDB",
    "type": "database",
    "count": 1,
    "info": {
        x: 203,
        y: 102
    }
}
```
##### 转换Json文档结构为BsonDocument类
```
var document = new BsonDocument
{
    { "name", "MongoDB" },
    { "type", "Database" },
    { "count", 1 },
    { "info", new BsonDocument
        {
            { "x", 203 },
            { "y", 102 }
        }
    }
};
```

##### 插入文档到集合
==InsertOne/InsertOneAsync==
```
collection.InsertOne(document);
await collection.InsertOneAsync(document);
```

#### 2. 插入多个文档
==InsertMany/InsertManyAsync==
```
var documents = Enumerable.Range(0,99).select(i=>new BsonDocument("counter",i));
collection.InsertMany(documents);
await collection.InsertManyAsync(documents);
```

### 四、统计文档个数
==CountDocuments/CountDocumentsAsync==
```
var count = collection.CountDocuments(new BsonDocument());
var count = await collection.CountDocumentsAsync(new BsonDocument());
```

##### 本质
* 本质上以上代码中==传入给CountDocuments方法的参数是一个filter==
* new BsonDocument()是一个空的filter，所以结果是所有文档的个数

### 五、查询
==Find(filter): IFindFluent<TDocument,TProjection>==

#### 1. 查找第一个document
```
var document = collection.Find(new BsonDocument()).FirstOrDefault();
var document = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
```

#### 2. 查找所有Document
```
var documents = collection.Find(new BsonDocument()).ToList();
var documents = await collection.Find(new BsonDocument()).ToListAsync();
```

##### 迭代获取所有document
```
// 异步迭代，提高性能，每一个document返回就会触发回调，然后输出
await collection.Find(new BsonDocument()).ForEachAsync(d=>Console.WriteLine(d));

// 同步迭代，返回所有的document，才能迭代输出
var cursor = collection.Find(new BsonDocument()).ToCursor();
foreach(var document in cursor.ToEnumerable())
{
    Console.WriteLine(document);
}
```

### 六、Filter

==应用Filter需要预先采用Builders<TDocument>.Filter创建filter==

**Filter可用于以下各个操作！！，相当于关系型数据库中的where**

#### 1. 查找所有文档中，i字段为71的文档
```
var filter = Builders<BsonDocument>.Filter.Eq("i", 71);
var document = collection.Find(filter).First();
var document = await collection.Find(filter).FirstAsync();
```

#### 1. 查找所有文档中，50 < i <= 100 的文档
```
var filterBuilder = Builders<BsonDocument>.Filter;
var filter = filterBuilder.Gt("i", 50) & filterBuilder.Lte("i",100);
```

### 七、Sort
==应用Sort需要预先采用Builders<TDocument>.Sort创建sort==
```
var filter = Builders<BsonDocument>.Filter.Exists("i");
var sort = Builders<BsonDocument>.Sort.Descending("i");
var document = collection.Find(filter).Sort(sort).First();
```

### 八、Projection Fields
==应用Projection需要预先采用Builders<TDocument>.Projection创建projection==
```
// 在结果document中不包含_id字段
var projection = Builders<BsonDocument>.Projection.Exclude("_id");
var documents = collection.Find(new BsonDocument()).Project(projection).First();
```

### 九、更新
==Update具体操作需要采用Builders<TDocument>.Update创建update操作==
```
var update = Builders<BsonDocument>.Update.Set("i",110);
```

==UpdateOne/UpdateOneAsync==
```
// 通过filter找出字段i=10的第一个文档，更新i的值为110
var filter = Builders<BsonDocument>.Filter.Eq("i",10);
collection.UpdateOne(filter,update);
```

==UpdateMany/UpdateManyAsync==
```
// 查找字段i<100的文档，并将字段i增加100
var filter = Builders<BsonDocument>.Filter.Lt("i",100);
var update = Builders<BsonDocument>.Update.Inc("i",100);
var result = collection.UpdateMany(filter,update);
```
**result代表更新文档的结果，包含了此次更新修改的文档的个数**

### 十、删除
==DeleteOne/DeleteOneAsync==
```
var filter = Builders<BsonDocumeent>.Filter.Eq("i",10);
collection.DeleteOne(filter);
```

==DeleteMany/DeleteManyAsync==
```
var filter = Builders<BsonDocument>.Filter.Gte("i",100);
var result =collection.DeleteMany(filter);
```
**result代表删除文档的结果，包含了此次被删除的文档的个数**

### 十一、批量写-Bulk Writes
**有两种批量写操作：**
* 有序批量操作：根据顺序有序地执行所有的操作，如果出错返回第一个错误
* 无序批量操作：无法保证执行操作的顺序性，执行所有操作返回所有的错误

**创建需要批量写操作:**
```
var models = new WriteModel<BsonDocument>[] 
{
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 4)),
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 5)),
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 6)),
    new UpdateOneModel<BsonDocument>(
        new BsonDocument("_id", 1), 
        new BsonDocument("$set", new BsonDocument("x", 2))),
    new DeleteOneModel<BsonDocument>(new BsonDocument("_id", 3)),
    new ReplaceOneModel<BsonDocument>(
        new BsonDocument("_id", 3), 
        new BsonDocument("_id", 3).Add("x", 4))
};
```

#### 1. 有序批量操作
```
// 1. Ordered bulk operation - order of operation is guaranteed
collection.BulkWrite(models);

// 2. Unordered bulk operation - no guarantee of order of operation
collection.BulkWrite(models, new BulkWriteOptions { IsOrdered = false });
```
```
// 1. Ordered bulk operation - order of operation is guaranteed
await collection.BulkWriteAsync(models);

// 2. Unordered bulk operation - no guarantee of order of operation
await collection.BulkWriteAsync(models, new BulkWriteOptions { IsOrdered = false });
```


SDK CRUD 汇总
--------

### 一、前提
```
表(Table) - 集合(Collection)
一行数据 - 文档(Document)
```

### 二、考虑以下的Bson文档结构和定义的类
```
// 文档结构
{
    "name": "MongoDB",
    "type": "database",
    "count": 1,
    "info": {
        x: 203,
        y: 102
    }
}
// 定义Document类
public class Foo
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Count { get; set; }
    public Bar Info { get; set; }
}
public class Bar
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

## FilterDefinition<Document>类 - 非常重要！-但是一般不单用，而是用Builders<TDocument>.Filter去创建！
* **以下标志着可以用单独的BsonDocument实例/Func委托表达式树/Bson结构的json来传入(会自动隐式转换为FilterDefinition<TDocument>)**
* **在不需要任何过滤条件Filter的时候，Empty属性是个很好的选择=空的BsonDocument对象实例**
* **除此之外还可以通过FilterDefinitionBuilder<TDocument>去创建出来，而前者可以由Builders<TDocument>的Filter属性获得！！**
* ** new BsonDocument("_id",1) 等同于 (BsonDoucment doc)=>doc._id == 1 **，**十分重要的概念！！！**
```
public static FilterDefinition<TDocument> Empty { get; }

public static implicit operator FilterDefinition<TDocument>(BsonDocument document);
public static implicit operator FilterDefinition<TDocument>(Expression<Func<TDocument, bool>> predicate);
public static implicit operator FilterDefinition<TDocument>(string json);
```

## Builders<TDocument>类 - 非常重要！- 很多XXXDefinitionBuilder<TDocument>的基础！
```
public static FilterDefinitionBuilder<TDocument> Filter { get; }    // 很重要，很常用！！！
public static IndexKeysDefinitionBuilder<TDocument> IndexKeys { get; }
public static ProjectionDefinitionBuilder<TDocument> Projection { get; }
public static SortDefinitionBuilder<TDocument> Sort { get; }    // 很重要很常用！！！
public static UpdateDefinitionBuilder<TDocument> Update { get; }    // 很重要很常用！！！
```

## FilterDefinitionBuilder<TDocument>类 - 非常重要！
**这里面就是传统的DB数据库的where条件语句的内容，包括但不限于以下：**
* All
* And
* Eq
* 等等其他的很多判断Filter条件

## UpdateDefinitionBuilder<TDocument>类 - 非常重要！
**对数据进行更新的具体操作，包括但不限于以下：**
* Set
* AddToSet
* Max
* Min
* Inc
* Pipeline - PipelineDefinition<TDocument> - 比较特殊，也很重要！！！
* 等等其他很多Update操作行为

### 三、插入操作汇总
* InsertOne/InsertOneAsync - TDocument
* InsertMany/InsertManyAsync - IEnumerable<TDocument>

### 四、计数操作汇总
* CountDocuments/CountDocumentsAsync - FilterDefinition<TDocument>
```
CountDocuments(new BsonDocument())
```

### 五、查询操作汇总 - 包括后置的行为(第一个|转换成列表|排序|投影字段|等等)
* Find/FindAsync - FilterDefinition<TDocument>
* FindOneAndDelete/FindOneAndDeleteAsync - FilterDefinition<TDocument>
* FindOneAndReplace/FindOneAndReplaceAsync - FilterDefinition<TDocument>
* FindOneAndUpdate/FindOneAndUpdateAsync - FilterDefinition<TDocument>
* Find/FindAsync -> FirstOrDefault/FirstOrDefaultAsync
* Find/FindAsync -> ToList/ToListAsync
* Find/FindAsync -> Sort - SortDefinition<TDocument>
* Find/FindAsync -> Project - ProjectionDefinition<TDocument>

### 六、更新操作汇总
* UpdateOne/UpdateOneAsync - FilterDefinition<TDocument>,UpdateDefinition<TDocument>
* UpdateMany/UpdateManyAsync - FilterDefinition<TDocument>,UpdateDefinition<TDocument>
```
var filter = Builders<BsonDocument>.Filter.Eq("i", 10);
var update = Builders<BsonDocument>.Update.Set("i", 110);
collection.UpdateOne(filter, update);

var filter = Builders<BsonDocument>.Filter.Lt("i", 100);
var update = Builders<BsonDocument>.Update.Inc("i", 100);
var result = collection.UpdateMany(filter, update);
if (result.IsModifiedCountAvailable)    // 这个判断要记住！！！
{
    Console.WriteLine(result.ModifiedCount);    // 此Count属性可以举一反三，如DelteCount等！！！
}
```

### 七、删除操作汇总
* DeleteOne/DeleteOneAsync - FilterDefinition<TDocument>
* DeleteMany/DeleteManyAsync - FilterDefinition<TDocument>

## Bulk Writes - 批量操作汇总 - 十分重要！！！
**分有序和无序批量执行，按下不表**
### 基本操作
* BulkWrite/BulkWriteAsync - IEnumerable<WriteModel<TDocument>>

### WriteModel<TDocument>类
* abstract - 抽象类，意味着肯定要被具体的类继承实现
* public abstract WriteModelType ModelType { get; } - 其实也就标记着哪些子类的可能
```
public enum WriteModelType
{
    InsertOne = 0,
    DeleteOne = 1,
    DeleteMany = 2,
    ReplaceOne = 3,
    UpdateOne = 4,
    UpdateMany = 5
}
```
#### 1. InsertOneModel - 构造(TDocument document)
#### 2. UpdateOneModel - 构造(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
#### 3. UpdateManyModel - 构造(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
#### 4. DeleteOneModel - 构造(FilterDefinition<TDocument> filter)
#### 5. DeleteManyModel - 构造(FilterDefinition<TDocument> filter)
#### 6. ReplaceOneModel - 构造(FilterDefinition<TDocument> filter, TDocument replacement)