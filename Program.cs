using System.Net;
using System.Runtime.Caching;
using ICSharpCode.SharpZipLib.Zip;

// Initialize cache
MemoryCache cache = MemoryCache.Default;

// Create a new HttpListener instance
HttpListener listener = new HttpListener();
const string FilePath = "./Files";

// Add prefix for listening on port 80
listener.Prefixes.Add("http://*:8080/");

// Start the listener
listener.Start();
printNote("Listening on port 8080...");

// Handle incoming requests
Thread server = new Thread(() =>
{
    while (listener.IsListening)
    {
        try
        {
            // Wait for an incoming request
            HttpListenerContext context = listener.GetContext();
            Thread request = new Thread(() => handleRequest(context));
            request.Start();
        }
        catch (Exception ex)
        {
            printError(ex.Message);
        }
    }
});

server.Start();
// Wait for the user to press a key before quitting
printNote("Press any key to stop the server...");
Console.ReadKey();
printNote("Key pressed. Stopping server...");

// Stop the listener
listener.Stop();

void handleRequest(HttpListenerContext context) {
    string arguments = context.Request.RawUrl ?? "";

    // Check if there are arguments
    if(arguments == "" || arguments == "/") {
        returnText("No Files Requested", context);
        printWarning("No Files Requested.");
        return;
    }

    // Do not process favicon request
    if(arguments == "/favicon.ico") return;

    byte[] cachedZip = (byte[])cache.Get(arguments);
    if(cachedZip != null) {
        downloadZip(cachedZip, context);
        printNote("Request is already cached.");
        return;
    }
    else
    {
        printWarning("Request not cached.");
    }

    string[] requestedFiles = arguments.Substring(1).Split("&");

    List<string> paths = new List<string>();

    // Find what files are present
    foreach(var item in requestedFiles)
    {   
        
        printNote($"File {item} requested.");
        string path = Path.Combine(FilePath, item);
        if(File.Exists(path))
        {
            paths.Add(path);
            printNote($"File {item} found.");
        }
        else
        {
            printWarning($"File {item} not found.");
        }
    }

    if(paths.Count == 0) 
    {
        returnText("No Files Found!", context);
        printWarning("No Files Found.");
        return;
    }

    List<Thread> threads = new();
    List<(byte[], string)> results = new();
    
    // Read all files in parallel
    foreach(string path in paths) 
    {
        Thread readFile = new Thread(() => {
            byte[] file = File.ReadAllBytes(path);
            printNote($"File {Path.GetFileName(path)} read.");
            results.Add((file, path));
        });
        readFile.Start();
        threads.Add(readFile);
    }
    foreach(var thread in threads) 
    {
        thread.Join();
    }

    // Combine all files and zip them
    Thread zipFiles = new Thread(() => zip(results, context, arguments));  
    zipFiles.Start(); 
}

void returnText(string text, HttpListenerContext context) {
    string responseString = $"<html><body>{text}</body></html>";
    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
    context.Response.ContentLength64 = buffer.Length;
    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
    context.Response.Close();
}

void zip(List<(byte[], string)> results, HttpListenerContext context, string request) {
    using(var zip = new MemoryStream()) 
    {
        using (var zipStream = new ZipOutputStream(zip))
        {
            zipStream.SetLevel(5);
            foreach(var result in results) {
                zipStream.PutNextEntry(new ZipEntry(Path.GetFileName(result.Item2)));
                zipStream.Write(result.Item1, 0, result.Item1.Length);
                zipStream.CloseEntry();
            }
            zipStream.Finish();
            byte[] zippedFile = zip.ToArray();
            printNote("Files zipped.");

            // Add zipped file to cache
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
            };
            cache.Add(request, zippedFile, policy);
            printNote("Zipped file cached.");

            downloadZip(zippedFile, context);
        }
    }
}

void downloadZip(byte[] zip, HttpListenerContext context)
{
    printNote("Downloading zip...");
    context.Response.ContentType = "appliaction/zip";
    context.Response.ContentLength64 = zip.Length;
    context.Response.Headers.Add("Content-Disposition", "attachment; filename=\"archive.zip\"");
    context.Response.OutputStream.Write(zip, 0, zip.Length);
    context.Response.Close();
}

void printWarning(string text) 
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("WARN: ");
    Console.ResetColor();
    Console.WriteLine(text);
}

void printNote(string text) 
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("NOTE: ");
    Console.ResetColor();
    Console.WriteLine(text);
}

void printError(string text) 
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("ERROR: ");
    Console.ResetColor();
    Console.WriteLine(text);
}