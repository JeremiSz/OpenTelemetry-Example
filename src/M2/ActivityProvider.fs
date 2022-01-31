namespace TraceProvider
open System.Diagnostics
open MongoDB.Driver
open MongoDB
open MongoDB.Bson

type IActivityService =
    abstract member CreateActivity: unit -> Activity
type ActivityService() =
    let serviceName = "CCS.OpenTelemetry.M2";
    let activityFactory = new ActivitySource(serviceName);
    interface IActivityService with
        member _.CreateActivity() = 
            activityFactory.StartActivity()

//type Student = { 
 //   id: BsonObjectId 
//    name: string
//}

//type IDatabaseable =
  //  abstract member GetConnection : string * string -> IMongoCollection<Student>

//type Databasable() =
 //   let connectionString = "mongodb://localhost:27017"
  //  let client = new MongoClient(connectionString)
   // interface IDatabaseable with
    //    member _.GetConnection(database:string, collection:string) =
     //       let db = client.GetDatabase(database)
      //      db.GetCollection<Student>(collection)