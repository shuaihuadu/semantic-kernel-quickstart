namespace Connectors.Memory.Milvus.Tests
{
    [TestClass]
    public class MilvusMemoryStore_Tests
    {
        //string collectionName = "unit_test_collection";
        string collectionName = "test_collection";

        private readonly IMilvusDbClient _milvusDbClient = new MilvusDbClient("192.168.186.129");

        private MilvusMemoryStore? _milvusMemoryStore = null;

        [TestInitialize]
        public void Init()
        {
            this._milvusMemoryStore = new MilvusMemoryStore(this._milvusDbClient);
        }

        [TestMethod]
        public async Task ItCanCreateAndGetCollectionAsync()
        {
            await _milvusMemoryStore!.CreateCollectionAsync(collectionName);

            var collections = _milvusMemoryStore.GetCollectionsAsync().ToEnumerable();

            Assert.IsTrue(collections.Any());

            foreach (var collection in collections)
            {
                Console.WriteLine(collection);
            }
        }

        [TestMethod]
        public async Task ItCanDeleteCollectionAsync()
        {
            await _milvusMemoryStore!.DeleteCollectionAsync(collectionName);

            bool collectionExists = await _milvusMemoryStore.DoesCollectionExistAsync(collectionName);

            Assert.IsFalse(collectionExists);
        }

        [TestMethod]
        public async Task ItCanGetBatchWithoutVectorAsync()
        {
            var data = await _milvusMemoryStore!.GetBatchAsync("test_collection", new string[] { "test_id1", "test_id2" }, false).ToListAsync();

            Assert.IsTrue(data.Any());
        }
        [TestMethod]
        public async Task ItCanGetBatchWithVectorAsync()
        {
            var data = await _milvusMemoryStore!.GetBatchAsync("test_collection", new string[] { "test_id1", "test_id2" }, true).ToListAsync();

            Assert.IsTrue(data.Any());
            Assert.IsTrue(data[0].Embedding.Length > 0);
        }
    }
}