using Microsoft.SemanticKernel.Memory;

namespace Connectors.Memory.Milvus.Tests
{
    [TestClass]
    public class MilvusMemoryStore_Tests
    {
        string collectionName = "unit_test_collection";
        //string collectionName = "test_collection";

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
            var data = await _milvusMemoryStore!.GetBatchAsync(collectionName, new string[] { "0", "1" }, false).ToListAsync();

            Assert.IsTrue(data.Any());
        }
        [TestMethod]
        public async Task ItCanGetBatchWithVectorAsync()
        {
            var data = await _milvusMemoryStore!.GetBatchAsync(collectionName, new string[] { "0", "1" }, true).ToListAsync();

            Assert.IsTrue(data.Any());
            Assert.IsTrue(data[0].Embedding.Length > 0);
        }

        [TestMethod]
        public async Task ItCanInsertDataAsync()
        {
            var records = new List<MemoryRecord>();

            for (int i = 0; i < 100; i++)
            {
                records.Add(MemoryRecord.LocalRecord($"{i}", $"text{i}", $"description{i}", new float[1536], $"", $"{i}"));
            }

            await foreach (var id in _milvusMemoryStore!.UpsertBatchAsync(collectionName, records))
            {
                Console.WriteLine(id);
            }
        }
    }
}