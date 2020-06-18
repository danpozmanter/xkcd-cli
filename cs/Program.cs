using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using CommandLine;
// System.Text.Json would be preferable but is not yet feature complete.

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
        public string Month;
        public int Num;
        public string Link;
        public string Year;
        public string News;
        public string Safe_Title;
        public string Transcript;
        public string Alt;
        public string Img;
        public string Title;
        public string Day;
    }

    class Program
    {

        static ComicResponse GetComicResponse(HttpClient client, int number) {
            var url = "";
            if (number == -1) {
                Console.WriteLine("Fetching latest comic");
                url = string.Format("http://xkcd.com/info.0.json", number);
            } else {
                Console.WriteLine(String.Format("Fetching comic #{0}", number));
                url = string.Format("http://xkcd.com/{0}/info.0.json", number);
            }
            try {
                var resp = client.GetStringAsync(url).Result;
                var data = JsonConvert.DeserializeObject<ComicResponse>(resp);
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
                Console.WriteLine(JsonConvert.SerializeObject(comic, Formatting.Indented));
            }
            else {
                Console.WriteLine(comic.Title);
                Console.WriteLine(comic.Alt);
                Console.WriteLine(comic.Img);
            }
        }

        static void SaveComic(HttpClient client, ComicResponse comic) {
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
