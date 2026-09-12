# Route validation and authoring notes

| World | Route work | Optional mastery and identity | Content |
|---|---|---|---|
| Sky City | Two broad sequences separated by a recovery terrace; shallow offsets and independently sized catches | Held-hop skips use the ordinary ivory architectural route; towers and gardens frame the city | 4 |
| Mountain | Lower shelves, waterfall recovery, ridge sequence, diagonal moving climb and summit chain | Inner ridge and an outside lift bypass; ten metres of physical ascent, geological depth and waterfalls | 5 |
| Ancient Abyss | Arrival chain, sanctuary bridge, ferry crossing, crumbling temple, Boost and final ascent | Arrival cut and ferry bypass remain; two conflicting temple ledges retired; masonry, cyan ruins and vertical void | 9 |
| Solar Foundry | Intake rhythm, inspection recovery, moving transfer, furnace court, cooling sequence and crown | Outside transfer brackets plus optional cooling-vent Boost and surf spillway; metal, hot structures and a solar collector | 5 |
| Windward Observatory | Accepted content-6 route geometry preserved | Existing four coherent sequences and broad recovery; new ivory skins, blue altitude sky and shared lighting | 6 |
| The Spiral | Modest width catches on easy/setup/flow pieces; helix preserved | Separate vertical challenge, existing advanced mechanics and celestial ruins; own visual/environment profile | 3 |

The stable-ID JSON recipe is an explicit Editor authoring input, not a runtime route generator. Camera-hint positions, optional-line endpoints, pickup positions, moving paths and bound Restore/Patch poses follow the authored supports. Existing save history is retained in its original content-version buckets; thresholds remain provisional and any valid Campaign completion still earns at least Bronze.

## What the tests establish

`AllWorldFlowTests` covers ten sequences in the first four worlds, with slow / flow / expert incoming speed and six entry cases: clean, left, right, early, late and heading error. Each case carries the real motor state between hops. The 180 outcomes and per-flight CSVs include actual speed, flight distance, skipped supports, landing offsets and recovery/fatal outcome. Initial speed is supplied by the fixture, so this is local sequence feasibility, not proof that a human earns every entry speed in a full run. Windward's separate 72-case accepted matrix remains unchanged.

`WorldContinuousRouteTests` starts once at rest on each of the five Campaign worlds and reaches the actual Patch trigger at 80% and 100% ground input. Moving platforms advance on their actual paths, crumbles tick and Boost triggers run through physics. The pilot supplies explicit stick input and aerial correction; it does not teleport between hops, move the route under the player or replace player velocity after the initial start. Times are automated-pilot measurements, not rank calibration.

Separate tests sweep moving bodies and riding clearance, raycast visible/support truth, validate optional brackets and the Mountain/Abyss bypasses, and exercise Foundry Boost landing plus valid surf contact and manual jump detachment. The Foundry surf test does not establish a complete expert speedrun through that branch. Spiral is covered by the existing regression suite; the ten-run Campaign matrix does not include it.

Some earlier fixtures assumed tiny platforms and jumped from their centers. They now walk to an actual takeoff edge, retaining landing/support assertions. Short adjacent lateral hops retain their short runup. The obsolete Sky City FlowManual comparison fixture now checks the shared Foundation trial profile used by this release. Sparse combined foundations are sampled at triangle centroids if a coarse bounds grid misses their tops. Foundation depth is checked against the local route height, and collision-truth assertions remain active.

## Physical feedback still needed

Play all five Campaign worlds in Classic and Easy, then Spiral. Check hold-jump/drag comfort, recovery after imperfect strafes, discovery and difficulty of optional lines, scenery readability, Restore/Patch visibility, both landscape safe areas and sustained thermal/frame pacing on S23. No physical-device test has been performed. The new lighting and art are review candidates; visual acceptance is not established by automated completion.
