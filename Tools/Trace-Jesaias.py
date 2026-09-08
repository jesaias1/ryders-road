"""Trace the user-approved flat emblem; regenerate Unity rasters from editable SVG polygons.
Requires Pillow, numpy and OpenCV. No generated replacement branding.
Usage: python Tools/Trace-Jesaias.py <approved source image> (initial trace)
       python Tools/Trace-Jesaias.py (rasterize the checked-in SVG)
"""
from pathlib import Path
import sys
import xml.etree.ElementTree as ET
import numpy as np
import cv2
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1]
source = root / 'Assets/Branding/Source/Jesaias_Emblem.svg'
if len(sys.argv) > 1:
    im = np.array(Image.open(sys.argv[1]).convert('RGB'))
    mask = ((im.min(axis=2) > 90) & (im.max(axis=2) > 140)).astype('uint8') * 255
    contours, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    paths = []
    for contour in contours:
        if abs(cv2.contourArea(contour)) < 30:
            continue
        pts = cv2.approxPolyDP(contour, .65, True).reshape(-1, 2)
        paths.append('M ' + ' L '.join(f'{x},{y}' for x, y in pts) + ' Z')
    source.write_text('<svg xmlns="http://www.w3.org/2000/svg" viewBox="450 245 770 430">\n'
                      '<title>Jesaias emblem — traced from user-approved September 8 source</title>\n'
                      '<path fill="#b4d8dc" fill-rule="evenodd" d="' + ' '.join(paths) + '"/>\n</svg>\n', encoding='utf-8')

# The SVG remains the editable authority; even-odd polygon rasterization preserves holes.
svg = ET.parse(source).getroot()
import re
d = svg.find('{http://www.w3.org/2000/svg}path').attrib['d']
mask = Image.new('1', (1540, 860))
for part in d.split('M ')[1:]:
    points = [(round((float(x)-450)*2), round((float(y)-245)*2)) for x,y in re.findall(r'([\d.]+),([\d.]+)', part)]
    layer = Image.new('1', mask.size)
    ImageDraw.Draw(layer).polygon(points, fill=1)
    from PIL import ImageChops
    mask = ImageChops.logical_xor(mask, layer)
rgba = Image.new('RGBA', mask.size, '#b4d8dc')
rgba.putalpha(mask.convert('L'))
rgba = rgba.resize((770, 430), Image.Resampling.LANCZOS)
out = root / 'Assets/Branding/Resources/Branding/Jesaias_Emblem.png'
rgba.save(out)
poster = Image.new('RGB', (1920, 1080), '#050e18')
small = rgba.resize((540,302), Image.Resampling.LANCZOS)
poster.paste(small,(690,389),small)
poster.save(root / 'Assets/Branding/Android/Jesaias_Startup.png')
print(source)
print(out)
