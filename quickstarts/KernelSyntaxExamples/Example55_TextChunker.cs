namespace KernelSyntaxExamples;

public class Example55_TextChunker : BaseTest
{
    [Fact]
    public void RunExample()
    {
        this.WriteLine("=== Text chunking ===");

        List<string> lines = TextChunker.SplitPlainTextLines(Text, 40);
        List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120);

        WriteParagraphsToConsole(paragraphs);
    }

    [Theory]
    [InlineData(TokenCounterType.SharpToken)]
    [InlineData(TokenCounterType.MicrosoftML)]
    [InlineData(TokenCounterType.MicrosoftMLRoberta)]
    [InlineData(TokenCounterType.DeepDev)]
    public void RunExampleForTokenCounterType(TokenCounterType counterType)
    {
        this.WriteLine($"=== Text chunking with a custom({counterType}) token counter ===");

        Stopwatch sw = new Stopwatch();
        sw.Start();

        TokenCounter tokenCounter = tokenCounterFactory(counterType);

        List<string> lines = TextChunker.SplitPlainTextLines(Text, 40, tokenCounter);
        List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: tokenCounter);

        sw.Stop();

        this.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
        WriteParagraphsToConsole(paragraphs);
    }

    [Fact]
    public void RunExampleWithHeader()
    {
        this.WriteLine("=== Text chunking with chunk header ===");

        List<string> lines = TextChunker.SplitPlainTextLines(Text, 40);
        List<string> paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 150, chunkHeader: "DOCUMENT NAME:test.text\n\n");

        WriteParagraphsToConsole(paragraphs);
    }

    private void WriteParagraphsToConsole(List<string> paragraphs)
    {
        for (int i = 0; i < paragraphs.Count; i++)
        {
            this.WriteLine(paragraphs[i]);

            if (i < paragraphs.Count - 1)
            {
                this.WriteLine("------------------------");
            }
        }
    }

    public enum TokenCounterType
    {
        SharpToken,
        MicrosoftML,
        DeepDev,
        MicrosoftMLRoberta
    }

    private static TokenCounter SharpTokenTokenCounter => (string input) =>
    {
        GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");

        List<int> tokens = encoding.Encode(input);

        return tokens.Count;
    };

    public static TokenCounter MicrosoftMLTokenCounter => (string input) =>
    {
        Tokenizer tokenizer = new(new Bpe());

        IReadOnlyList<string> tokens = tokenizer.Encode(input).Tokens;

        return tokens.Count;
    };

    private static TokenCounter MicrosoftMLRobertaTokenCounter => (string input) =>
    {
        Stream? encoder = EmbeddedResource.ReadStream("EnglishRoberta.encoder.json");
        Stream? vocab = EmbeddedResource.ReadStream("EnglishRoberta.vocab.bpe");
        Stream? dict = EmbeddedResource.ReadStream("EnglishRoberta.dict.txt");

        if (encoder is null || vocab is null || dict is null)
        {
            throw new FileNotFoundException("Missing required resources");
        }

        EnglishRoberta model = new(encoder, vocab, dict);

        model.AddMaskSymbol();

        Tokenizer tokenizer = new(model, new RobertaPreTokenizer());

        IReadOnlyList<string> tokens = tokenizer.Encode(input).Tokens;

        return tokens.Count;
    };

    private static TokenCounter DeepDevTokenCounter => (string input) =>
    {
        ITokenizer tokenizer = TokenizerBuilder.CreateByEncoderNameAsync("cl100k_base").GetAwaiter().GetResult();

        List<int> tokens = tokenizer.Encode(input);

        return tokens.Count;
    };

    private static readonly Func<TokenCounterType, TokenCounter> tokenCounterFactory = (TokenCounterType counterType) =>
        counterType switch
        {
            TokenCounterType.SharpToken => (string input) => SharpTokenTokenCounter(input),
            TokenCounterType.MicrosoftML => (string input) => MicrosoftMLTokenCounter(input),
            TokenCounterType.DeepDev => (string input) => DeepDevTokenCounter(input),
            TokenCounterType.MicrosoftMLRoberta => (string input) => MicrosoftMLRobertaTokenCounter(input),
            _ => throw new ArgumentOutOfRangeException(nameof(counterType), counterType, null)
        };

    private const string Text = @"The city of Venice, located in the northeastern part of Italy,
is renowned for its unique geographical features. Built on more than 100 small islands in a lagoon in the
Adriatic Sea, it has no roads, just canals including the Grand Canal thoroughfare lined with Renaissance and
Gothic palaces. The central square, Piazza San Marco, contains St. Mark's Basilica, which is tiled with Byzantine
mosaics, and the Campanile bell tower offering views of the city's red roofs.

The Amazon Rainforest, also known as Amazonia, is a moist broadleaf tropical rainforest in the Amazon biome that
covers most of the Amazon basin of South America. This basin encompasses 7 million square kilometers, of which
5.5 million square kilometers are covered by the rainforest. This region includes territory belonging to nine nations
and 3.4 million square kilometers of uncontacted tribes. The Amazon represents over half of the planet's remaining
rainforests and comprises the largest and most biodiverse tract of tropical rainforest in the world.

The Great Barrier Reef is the world's largest coral reef system composed of over 2,900 individual reefs and 900 islands
stretching for over 2,300 kilometers over an area of approximately 344,400 square kilometers. The reef is located in the
Coral Sea, off the coast of Queensland, Australia. The Great Barrier Reef can be seen from outer space and is the world's
biggest single structure made by living organisms. This reef structure is composed of and built by billions of tiny organisms,
known as coral polyps.";

    public Example55_TextChunker(ITestOutputHelper output) : base(output)
    {
    }
}
