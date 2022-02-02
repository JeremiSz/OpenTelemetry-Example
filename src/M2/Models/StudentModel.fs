namespace StudentModel

open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes

type Student =
    {
        [<BsonId>]
        [<BsonRepresentation(BsonType.ObjectId)>]
        id : string;

        [<BsonElement("Name")>]
        name : string;
    }

