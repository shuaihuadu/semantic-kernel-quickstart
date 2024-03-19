namespace KernelSyntaxExamples;

public class Example55_TextChunker(ITestOutputHelper output) : BaseTest(output)
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

        Stopwatch sw = new();
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

    public class SharpTokenTokenCounter
    {
        private readonly GptEncoding _encoding;

        public SharpTokenTokenCounter()
        {
            this._encoding = GptEncoding.GetEncoding("cl100k_base");
        }

        public int Count(string input)
        {
            List<int> tokens = this._encoding.Encode(input);

            return tokens.Count;
        }
    }

    public class MicrosoftMLTokenCounter
    {
        private readonly Tokenizer _tokenizer;

        public MicrosoftMLTokenCounter()
        {
            this._tokenizer = new(new Bpe());
        }

        public int Count(string input)
        {
            IReadOnlyList<string> tokens = this._tokenizer.Encode(input).Tokens;

            return tokens.Count;
        }
    }

    public class MicrosoftMLRobertaTokenCounter
    {
        private readonly Tokenizer _tokenizer;

        public MicrosoftMLRobertaTokenCounter()
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

            this._tokenizer = new(model, new RobertaPreTokenizer());
        }

        public int Count(string input)
        {
            IReadOnlyList<string> tokens = _tokenizer.Encode(input).Tokens;

            return tokens.Count;
        }
    }

    public class DeepDevTokenCounter
    {
        private readonly ITokenizer _tokenizer;

        public DeepDevTokenCounter()
        {
            this._tokenizer = TokenizerBuilder.CreateByEncoderNameAsync("cl100k_base").GetAwaiter().GetResult();
        }

        public int Count(string input)
        {
            List<int> tokens = this._tokenizer.Encode(input, []);

            return tokens.Count;
        }
    }

    private static readonly Func<TokenCounterType, TokenCounter> tokenCounterFactory = (TokenCounterType counterType) =>
        counterType switch
        {
            TokenCounterType.SharpToken => new SharpTokenTokenCounter().Count,
            TokenCounterType.MicrosoftML => new MicrosoftMLTokenCounter().Count,
            TokenCounterType.DeepDev => new DeepDevTokenCounter().Count,
            TokenCounterType.MicrosoftMLRoberta => new MicrosoftMLRobertaTokenCounter().Count,
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
}
