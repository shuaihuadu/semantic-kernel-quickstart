namespace KernelSyntaxExamples.RepoUtils;

public static class RepoFiles
{
    public static string SamplePluginsPath()
    {
        const string Parent = "samples";
        const string Folder = "plugins";

        bool SearchPath(string pathToFind, out string result, int maxAttempts = 10)
        {
            var currentDirctory = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);

            bool found;

            do
            {
                result = Path.Join(currentDirctory, pathToFind);

                found = Directory.Exists(result);

                currentDirctory = Path.GetFullPath(Path.Combine(currentDirctory, ".."));
            }
            while (maxAttempts-- > 0 && !found);

            return found;
        }

        if (!SearchPath(Parent + Path.DirectorySeparatorChar + Folder, out string path)
            && !SearchPath(Folder, out path))
        {
            throw new YourAppException("Plugins directory not found. The app needs the plugins from the repo to work.");
        }

        return path;
    }
}
