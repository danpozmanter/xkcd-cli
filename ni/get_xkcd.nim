import argparse
import httpClient
import json
import strformat

type 
    ComicResponse = object
        month: string
        num: int
        link: string
        year: string
        news: string
        safe_title: string
        transcript: string
        alt: string
        img: string
        title: string
        day: string

type
    WebResponse = object
        comic: ComicResponse
        error: string
        

proc get_comic_response(number: int): WebResponse =
    let client = newHttpClient()
    var url = ""
    if (number == -1):
        echo "Fetching latest comic"
        url = "http://xkcd.com/info.0.json"
    else:
        echo &"Fetching comic # {number}"
        url = &"http://xkcd.com/{number}/info.0.json"
    let r = client.get(url)
    if (r.status == "200 OK"):        
        let cr = to(parseJson(r.body), ComicResponse)
        return WebResponse(comic:cr)
    return WebResponse(error:r.status)

proc print_comic_response(comic: ComicResponse, output_format: string): void =
    if output_format == "json":
        echo pretty(%* comic)
    else:
        echo comic.title
        echo comic.alt
        echo comic.img

proc save_comic(comic: ComicResponse): void =
    echo "Saving comic..."
    let image_url = comic.img
    let image_name = image_url.split("/")[^1]
    let filename = &"xkcd_{image_name}"
    let client = newHttpClient()
    let r = client.get(image_url)
    let image = r.body()
    let f = open(filename, FileMode.fmWrite)
    defer: f.close()
    f.write(image)

proc validate_output(output_format: string): bool =
    if (output_format != "text") and (output_format != "json"):
        echo &"Unrecognized output format {output_format}"
        return false
    return true

var p = newParser("Get XKCD"):
    help("Fetch XKCD from the XKCD API")
    flag("-s", "--save")
    option("-o", "--output_format", default="text")
    option("-n", "--number", default="-1")
let opts = p.parse()

if opts.help == true:
    quit(0)

if validate_output(opts.output_format) != true:
    quit(1)

let response = get_comic_response(parseInt(opts.number))
if response.error != "":
    echo "Error!"
    echo response.error
else:
    print_comic_response(response.comic, opts.output_format)
    if opts.save == true:
        save_comic(response.comic)
