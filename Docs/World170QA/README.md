# 0.17.0 evidence

[Before/after gallery](index.html) · [Route notes and limitations](ROUTES.md) · [Asset/provenance report](ASSETS.md)

All 252 EditMode and 156 PlayMode tests pass. Full XML and compact results with source hashes are in `tests/`. The `routes/` CSVs contain 180 sequence entry/error outcomes and ten complete Campaign runs. Windward's unchanged 72-case matrix is covered by the complete PlayMode suite. No physical S23 testing is claimed.

Thirty before and thirty after images cover opening, mid-route, hero vista, fast approach and destination. These are desktop environment captures; they do not include the overlay HUD. Route changes move analogous camera beats. Visual inspection confirms readable primary lanes and retained world silhouettes; reference-quality matching, phone readability and subjective acceptance remain open.

3DAIStudio was unavailable. `kit-provenance.csv`, `refit-provenance.csv` and `mesh-inventory.csv` distinguish new Editor-authored geometry from processed production assets. No externally generated asset is claimed.

## Scene inventory

Static combined meshes are counted once. Counts include scene geometry, not just visible geometry or actual draw calls. Materials and shadow casters are instantiated scene counts. Existing worlds have no LOD groups in this inventory.

| World | Triangles | Mesh instances | Materials | Shadow casters | Desktop render CPU median |
|---|---:|---:|---:|---:|---|
| Sky City | 151,573 | 172 | 18 | 111 | 0.870 ms |
| Mountain | 119,750 | 331 | 25 | 299 | 0.871 ms |
| Ancient Abyss | 242,805 | 200 | 32 | 155 | 0.859 ms |
| Solar Foundry | 60,230 | 181 | 30 | 155 | 0.686 ms |
| Windward Observatory | 24,619 | 119 | 14 | 101 | 0.514 ms |
| The Spiral | 319,802 | 331 | 24 | 208 | 0.500 ms |

CPU values are twelve warm Camera.Render samples at 1560×720 on an RTX 5070 desktop. They are not GPU timing, device frame rate or sustained S23 performance. Texture dimensions, shader names and instancing flags appear in each detailed budget CSV. Phone testing must determine whether the 65 m / two-cascade shadow configuration is affordable.
