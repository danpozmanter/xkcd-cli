import argparse
import json
import sys
from typing import cast, Union
import requests


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

    @classmethod
    def from_dict(cls, data: dict):
        instance = cls()
        for key in data:
            setattr(instance, key, data[key])
        return instance
    
    def to_json(self):
        return json.dumps(self.__dict__, indent=2)


WebResponse = Union[ComicResponse, str]


def handle_error(error: str):
    print('Error! {}'.format(error))


def grab_comic(number: str) -> WebResponse:
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
        return ComicResponse.from_dict(data)
    return r.text


def print_comic(comic: ComicResponse, output_format: str):
    if output_format == 'json':
        print(comic.to_json())
    else:
        print(comic.title)
        print(comic.alt)
        print(comic.img)


def save_comic(comic):
    print('Saving comic...')
    imgage_url = comic.img
    filename = 'xkcd_{}'.format(imgage_url.split('/')[-1])
    r = requests.get(imgage_url)
    if not r.ok:
        print('Error retrieving comic')
        print(r.status_code)
        print(r.text)
        sys.exit(1)
    image = r.content
    with open(filename, 'wb') as f:
        f.write(image)


def validate_output(output_format: str):
    if output_format not in ('text', 'json'):
        print('Unrecognized output format: {}'.format(output_format))
        return False
    return True


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument(
        '-n', type=int, metavar='number',
        help='comic number to retrieve',
        required=True
    )
    parser.add_argument(
        '-o', default='text',
        type=str, metavar='output_format',
        help='output format: text or json'
    )
    parser.add_argument(
        '-s',
        action='store_true',
        help='save comic to local filesystem'
    )
    args = parser.parse_args()
    if not validate_output(args.o):
        sys.exit(1)
    response = grab_comic(args.n)
    if type(response) == ComicResponse:
        comic = cast(ComicResponse, response)
        print_comic(comic, args.o)
        if args.s:
            save_comic(comic)
    else:
        handle_error(cast(str, response))
