using System;
using System.IO;
using System.Net.Http;
using CommandLine;
using System.Text.Json;

namespace cs
{
    class Arguments
    {
        [Option('n', "number", Default=-1, HelpText = "comic number to retrieve, latest by default")]
        public int Number { get; set; }
        [Option('o', "outputFormat", Default="text", Required = false, HelpText = "output format: text or json")]
        public string OutputFormat { get; set; }
        [Option('s', "save", Required = false, HelpText = "save comic locally")]
        public bool Save { get; set; }
    }

    class ComicResponse {
        public string Month { get; set; }
        public int Num { get; set; }
        public string Link { get; set; }
        public string Year { get; set; }
        public string News { get; set; }
        public string Safe_Title { get; set; }
        public string Transcript { get; set; }
        public string Alt { get; set; }
        public string Img { get; set; }
        public string Title { get; set; }
        public string Day { get; set; }
    }

    class Program
    {

        static ComicResponse GetComicResponse(HttpClient client, int number) {
            var url = "";
            if (number == -1) {
                Console.WriteLine("Fetching latest comic");
                url = string.Format("http://xkcd.com/info.0.json");
            } else {
                Console.WriteLine(String.Format("Fetching comic #{0}", number));
                url = string.Format("http://xkcd.com/{0}/info.0.json", number);
            }
            try {
                var resp = client.GetStringAsync(url).Result;
                var opts = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true};
                var data = JsonSerializer.Deserialize<ComicResponse>(resp, opts);
                return data;
            }
            catch(HttpRequestException e) {
                Console.WriteLine(String.Format("Error! {0}", e.Message));
            }
            catch(System.AggregateException e) {
                Console.WriteLine(String.Format("Error! {0}", e.Message));
            }
            return null;
        }

        static void PrintComicResponse(ComicResponse comic, string outputFormat) {
            if (outputFormat == "json") {
                var opts = new JsonSerializerOptions{ WriteIndented = true};
                Console.WriteLine(JsonSerializer.Serialize(comic, opts));
            }
            else {
                Console.WriteLine(comic.Title);
                Console.WriteLine(comic.Alt);
                Console.WriteLine(comic.Img);
            }
        }

        static void SaveComic(HttpClient client, ComicResponse comic) {
            Console.WriteLine("Saving comic...");
            var imagesplitname = comic.Img.Split("/");
            var imagename = imagesplitname[imagesplitname.Length - 1];
            var filename = String.Format("xkcd_{0}", imagename);
            try {
                var resp = client.GetByteArrayAsync(comic.Img).Result;
                File.WriteAllBytes(filename, resp);
            }
            catch(HttpRequestException e) {
                Console.WriteLine(String.Format("Error! Retrieving comic: {0}", e.Message));
            }
            catch(System.AggregateException e) {
                Console.WriteLine(String.Format("Error! Retrieving comic:  {0}", e.Message));
            }
            catch(System.Exception e) {
                Console.WriteLine(String.Format("Error! Saving comic: {0}", e.Message));
            }
        }

        static bool ValidateOutput(string outputFormat) {
            if ((outputFormat != "text") && (outputFormat != "json")) {
                Console.WriteLine(String.Format("Unrecognized output format: {0}", outputFormat));
                return false;
            }
            return true;
        }

        static void Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<Arguments>(args).WithParsed<Arguments>(a => {
                if (!ValidateOutput(a.OutputFormat)) {
                    Environment.Exit(1);
                }
                var client = new HttpClient();
                var comic = GetComicResponse(client, a.Number);
                if(comic != null) {
                    PrintComicResponse(comic, a.OutputFormat);
                    if (a.Save) {
                        SaveComic(client, comic);
                    }
                }
            });            
        }
    }
}
