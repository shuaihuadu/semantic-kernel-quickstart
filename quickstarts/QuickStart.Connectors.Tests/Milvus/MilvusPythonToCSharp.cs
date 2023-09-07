namespace QuickStart.Connectors.Tests.Milvus
{
    internal class MilvusPythonToCSharp
    {
        public async Task<List<Tuple<MemoryRecord, float>>> GetNearestMatchesAsync(string collectionName, ndarray embedding, int limit, float? minRelevanceScore = null, bool withEmbeddings = false)
        {
            if (!_client.HasCollection(collectionName))
            {
                _logger.Debug($"Collection {collectionName} does not exist, cannot search.");
                throw new Exception($"Collection {collectionName} does not exist, cannot search.");
            }

            if (embedding.Shape.Length == 1)
            {
                embedding = np.ExpandDims(embedding, axis: 0);
            }

            var (results, searchType) = await Search(collectionName, embedding, limit, _metricCache.GetValueOrDefault(collectionName, null) ?? "IP");

            _metricCache[collectionName] = searchType;

            var cleanedResults = new List<Dictionary<string, dynamic>>();
            var ids = new List<long>();

            foreach (var x in results)
            {
                if (minRelevanceScore != null && x["distance"] < minRelevanceScore)
                {
                    continue;
                }

                cleanedResults.Add(x);
                if (withEmbeddings)
                {
                    ids.Add(x[ID_FIELD]);
                }
            }

            if (withEmbeddings)
            {
                try
                {
                    var vectors = await _client.GetAsync(collectionName, ids, new List<string> { EMBEDDING_FIELD });
                }
                catch (Exception e)
                {
                    _logger.Debug($"Get embeddings in search failed due to: {e}.");
                    throw e;
                }

                var vectorDict = vectors.ToDictionary(res => res[ID_FIELD], res => res[EMBEDDING_FIELD]);
                foreach (var res in cleanedResults)
                {
                    res[EMBEDDING_FIELD] = vectorDict[res[ID_FIELD]];
                }
            }

            var resultsTuple = cleanedResults.Select(result => (MilvusDictToMemoryRecord(result["entity"]), result["distance"])).ToList();

            return resultsTuple;
        }
        public async Task<(MemoryRecord, float)> GetNearestMatchAsync(string collectionName, ndarray embedding, float? minRelevanceScore = null, bool withEmbedding = false)
        {
            var m = await GetNearestMatchesAsync(
                collectionName,
                embedding,
                1,
                minRelevanceScore,
                withEmbedding
            );
            if (m.Count > 0)
            {
                return m[0];
            }
            else
            {
                return (null, 0.0f);
            }
        }

    }
}
