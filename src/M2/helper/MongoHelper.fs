namespace Helpers

open MongoDB.Driver
open MongoDB.Driver.Core.Configuration
open MongoDB.Driver.Core.Extensions.DiagnosticSources
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open System

module MongoHelper = 
    type Student = 
        {
            [<BsonId>]
            [<BsonRepresentation(BsonType.ObjectId)>] 
            _id : string
            [<BsonElement("name")>] 
            name : string
        }

    let private connectionString = "mongodb://localhost:27017"
    let databaseName = "studentDB"
    let studentCollectionName = "students"
    let workflowCollectionName = "workflow"

    let private setting = MongoClientSettings.FromConnectionString(connectionString)
    let private  makeClusterBuilder(clusterbuilder:ClusterBuilder)= 
        new DiagnosticsActivityEventSubscriber()
        |> clusterbuilder.Subscribe
        |> ignore

    do setting.ClusterConfigurator <- Action<ClusterBuilder> makeClusterBuilder
    let private client = new MongoClient(setting)
    let private database = client.GetDatabase(databaseName)
   

    let getCollection collection  =
        database.GetCollection(collection);