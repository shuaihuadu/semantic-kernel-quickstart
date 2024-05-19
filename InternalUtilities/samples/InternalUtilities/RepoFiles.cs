﻿// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

public static class RepoFiles
{
    /// <summary>
    /// Scan the local folders from the repo, looking for "prompt_template_samples" folder.
    /// </summary>
    /// <returns>The full path to prompt_template_samples folder.</returns>
    public static string SamplePluginsPath()
    {
        const string Folder = "prompt_template_samples";

        static bool SearchPath(string pathToFind, out string result, int maxAttempts = 10)
        {
            var currDir = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            bool found;
            do
            {
                result = Path.Join(currDir, pathToFind);
                found = Directory.Exists(result);
                currDir = Path.GetFullPath(Path.Combine(currDir, ".."));
            } while (maxAttempts-- > 0 && !found);

            return found;
        }

        if (!SearchPath(Folder, out var path))
        {
            throw new YourAppException("Plugins directory not found. The app needs the plugins from the repo to work.");
        }

        return path;
    }
}
