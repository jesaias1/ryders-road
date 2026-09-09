"""Independent arithmetic calibration, NOT an engine port or game executable test.
No collision, weapon state, stamina, input pipeline or prediction is modeled.
"""
import json
import math
from pathlib import Path
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
HZ=120
DT=1/HZ
# Explicit comparison scenarios, not claims about every server's defaults.
SCENARIOS={"cs_projection":dict(run=7.8,projection=30*7.8/250,air=10*7.8,
    ground=5*7.8,friction=4,stop=75*7.8/250),
    "quake3_projection":dict(run=7.8,projection=7.8,air=7.8,
    ground=10*7.8,friction=6,stop=100*7.8/320)}


def push(v,w,cap,rate):
    allowance=max(0,cap-float(v@w))
    return v+w*min(allowance,rate*DT)


def measure(config):
    speed=0
    steps=0
    while speed<config['run']-.001 and steps<2400:
        speed=max(0,speed-max(speed,config['stop'])*config['friction']*DT)
        speed=min(config['run'],speed+config['ground']*DT)
        steps+=1
    acceleration=steps*DT
    steps=0
    while speed>.01 and steps<2400:
        speed=max(0,speed-max(speed,config['stop'])*config['friction']*DT);steps+=1
    rows=[]
    for speed in [7.8,10.8,14,17]:
        row={'initial':speed}
        for label,angle in [('small',10),('side',90),('reverse',180)]:
            v=np.array([0.,speed]);w=np.array([math.sin(math.radians(angle)),math.cos(math.radians(angle))])
            for _ in range(HZ//2):v=push(v,w,config['projection'],config['air'])
            row[label]=dict(heading=math.degrees(math.atan2(v[0],v[1])),speed=float(np.linalg.norm(v)),forward=float(v[1]))
        v=np.array([0.,speed])
        for _ in range(HZ):
            angle=math.atan2(v[0],v[1])+math.radians(89)
            v=push(v,np.array([math.sin(angle),math.cos(angle)]),config['projection'],config['air'])
        row['explicit89DegreeStrafeSpeed']=float(np.linalg.norm(v))
        row['neutralAirSpeed']=speed
        rows.append(row)
    return dict(parameters=config,secondsToRun=acceleration,groundReleaseSeconds=steps*DT,air=rows)


if __name__=='__main__':
    result=dict(method='Independent arithmetic at 120 Hz; no actual engine run',
        unitConvention='CS 250 u/s and Q3 320 u/s normalized separately to 7.8 m/s',
        scenarios={key:measure(value) for key,value in SCENARIOS.items()})
    target=ROOT/'Docs/Movement150QA/reference-calibration.json';target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
