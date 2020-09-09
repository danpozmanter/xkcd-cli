import argparse
from dataclasses import dataclass
import json
import sys
from pysumtypes import sumtype
import requests


@dataclass
class ComicResponse:
    month: str
    num: int
    link: str
    year: str
    news: str
    safe_title: str
    transcript: str
    alt: str
    img: str
    title: str
    day: str

    def to_json(self):
        return json.dumps(self.__dict__, indent=2)


@dataclass
class ErrorResponse:
    message: str


@sumtype
class WebResponse:
    comic: ComicResponse
    error: ErrorResponse


def handle_error(error: ErrorResponse):
    print('Error! {}'.format(error.message))


def get_comic_response(number: str) -> WebResponse:
    url = None
    if number == -1:
        print('Fetching latest comic')
        url = 'http://xkcd.com/info.0.json'
    else:
        print('Fetching comic # {}'.format(number))
        url = 'http://xkcd.com/{}/info.0.json'.format(number)
    r = requests.get(url)
    if r.ok:
        data = r.json()
        return WebResponse(ComicResponse(**data))
    return WebResponse(ErrorResponse(r.text))


def print_comic_response(comic: ComicResponse, output_format: str):
    if output_format == 'json':
        print(comic.to_json())
    else:
        print(comic.title)
        print(comic.alt)
        print(comic.img)


def save_comic(comic: ComicResponse):
    print('Saving comic...')
    image_url = comic.get('img')
    filename = 'xkcd_{}'.format(image_url.split('/')[-1])
    r = requests.get(image_url)
    if not r.ok:
        print('Error retrieving comic')
        print(r.status_code)
        print(r.text)
        sys.exit(1)
    image = r.content
    try:
        with open(filename, 'wb') as f:
            f.write(image)
    except Exception as e:
        print(e)


def validate_output(output_format: str):
    if output_format not in ('text', 'json'):
        print('Unrecognized output format: {}'.format(output_format))
        return False
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument(
        '-n', type=int, metavar='number', default=-1,
        help='comic number to retrieve, latest by default'
    )
    parser.add_argument(
        '-o', default='text',
        type=str, metavar='output_format',
        help='output format: text or json'
    )
    parser.add_argument(
        '-s', action='store_true',
        help='save comic to local filesystem'
    )
    args = parser.parse_args()
    if not validate_output(args.o):
        sys.exit(1)
    response = get_comic_response(args.n)
    if response.match(ComicResponse):
        comic = response.unwrap()
        print_comic_response(comic, args.o)
        if args.s:
            save_comic(comic)
    else:
        handle_error(response.unwrap())
