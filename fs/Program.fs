open System
open System.IO
open System.Net.Http
open System.Text.Json
open CommandLine

type Arguments = {
    [<Option('n', Default=(-1), HelpText = "comic number to retrieve, latest by default")>] Number : int;
    [<Option('o', Default="text", HelpText = "output format: text or json")>] OutputFormat : string;
    [<Option('s', HelpText = "save comic locally")>] Save : bool;
}

[<CLIMutable>]
type ComicResponse = {
    Month: string
    Num: int
    Link: string
    Year: string
    News: string
    Safe_Title: string
    Transcript: string
    Alt: string
    Img: string
    Title: string
    Day: string
}

type ErrorResponse = {
    Message: string
}

type WebResponse =
| Success of ComicResponse
| Error of ErrorResponse

let HandleError (error:string) =
    printfn "Error! %A" error

let GetComicResponse (client:HttpClient, number:int) = 
    let url = 
        match number with
        | -1 -> 
            printfn "Fetching latest comic"
            String.Format("http://xkcd.com/info.0.json")
        | _ -> 
            printfn "Fetching comic # %A" number
            String.Format("http://xkcd.com/{0}/info.0.json", number)
        
    try
        let resp = client.GetStringAsync(url).Result
        let opts = JsonSerializerOptions()
        opts.PropertyNameCaseInsensitive <- true
        let data = JsonSerializer.Deserialize<ComicResponse> (resp, opts)
        data |> WebResponse.Success
    with
    | :? HttpRequestException as e -> WebResponse.Error <| {Message=e.Message}
    | :? System.AggregateException as e -> WebResponse.Error <| {Message=e.Message}

let PrintComicResponse comic outputFormat = 
    if outputFormat = "json" then
        let opts = JsonSerializerOptions()
        opts.WriteIndented <- true
        let serialized = JsonSerializer.Serialize (comic, opts)
        printfn "%O" serialized
    else
        printfn "Title: %s" comic.Title
        printfn "Alt: %s" comic.Alt
        printfn "Img: %s" comic.Img

let SaveComic (client:HttpClient, comic) =
    printf "Saving comic...\n"
    let imagesplitname = comic.Img.Split("/")
    let imagename = imagesplitname.[imagesplitname.Length - 1]
    let filename = String.Format("xkcd_{0}", imagename)
    try
        let resp = client.GetByteArrayAsync(comic.Img).Result
        File.WriteAllBytes(filename, resp)
    with
    | :? HttpRequestException as e -> HandleError ("Retrieving comic: " + e.Message)
    | :? System.AggregateException as e -> HandleError ("Retrieving comic: " + e.Message)
    | e -> HandleError ("Saving comic: " + e.Message)

let ValidateOutput outputFormat =
    List.contains outputFormat ["text"; "json"]

[<EntryPoint>]
let main argv =
    let result = CommandLine.Parser.Default.ParseArguments<Arguments>(argv)
    match result with
        | :? Parsed<Arguments> as args ->
            if not (ValidateOutput args.Value.OutputFormat) then
                printfn "Unrecognized output format: %A" args.Value.OutputFormat
                Environment.Exit(1)
            let client = new HttpClient()
            match GetComicResponse(client, args.Value.Number) with
            | WebResponse.Success s ->
                PrintComicResponse s args.Value.OutputFormat
                if args.Value.Save then
                    SaveComic(client, s)
            | WebResponse.Error e -> HandleError e.Message     
        | _ -> ()
    0
