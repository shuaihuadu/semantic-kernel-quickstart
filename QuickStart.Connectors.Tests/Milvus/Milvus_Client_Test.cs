using Milvus.Client;

namespace QuickStart.Connectors.Tests.Milvus;

[TestClass]
public class Milvus_Client_Test
{
    [TestMethod]
    public async Task Get_Collection_Test()
    {
        MilvusClient milvusClient = new MilvusClient("192.168.186.129", port: 19530, null, null, null);

        MilvusHealthState state = await milvusClient.HealthAsync();

        MilvusCollection collection = milvusClient.GetCollection("test_collection");

        await collection.LoadAsync();

        await collection.WaitForCollectionLoadAsync();


        string expr = "id in [\"test_id1\"]";

        QueryParameters queryParameters = new();

        queryParameters.OutputFields.Add("*");

        var queryResult = await collection.QueryAsync(expr, queryParameters);

        MilvusCollectionDescription collectionInfo = await collection.DescribeAsync();
    }
}
