import argparse
import json
import sys
import requests

""" response data:
ComicResponse = {
    month:string
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
}
"""


def grab_comic(number):
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
        return data
    print('Error! {}'.format(r.text))
    return None


def print_comic(comic, output_format):
    if not comic:
        # error!
        return
    if output_format == 'json':
        print(json.dumps(comic, indent=2))
    else:
        print(comic['title'])
        print(comic['alt'])
        print(comic['img'])


def save_comic(comic):
    print('Saving comic...')
    imgage_url = comic.get('img')
    filename = 'xkcd_{}'.format(imgage_url.split('/')[-1])
    r = requests.get(imgage_url)
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


def validate_output(output_format):
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
    comic = grab_comic(args.n)
    if comic:
        print_comic(comic, args.o)
        if args.s:
            save_comic(comic)
