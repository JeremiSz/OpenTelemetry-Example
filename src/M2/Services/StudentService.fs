namespace StudentService
open StudentModel;
open Microsoft.Extensions.Options;
open MongoDB.Driver;

type StudentService =
    {
        _studentCollection : IMongoCollection<Student> 

        BooksService(IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings) =
            {
                var mongoClient = new MongoClient(
                    bookStoreDatabaseSettings.Value.ConnectionString);

                var mongoDatabase = mongoClient.GetDatabase(
                    bookStoreDatabaseSettings.Value.DatabaseName);

                _booksCollection = mongoDatabase.GetCollection<Book>(
                    bookStoreDatabaseSettings.Value.BooksCollectionName);
            }
    
    
    }

