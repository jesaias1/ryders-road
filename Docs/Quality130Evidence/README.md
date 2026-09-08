# S23 gameplay-quality evidence — September 9, 2026

These are the user's actual device captures, not concept art or tests performed
by Codex. They supersede the assumption that 0.12.0 routes are close to accepted.
The user reports first-try Diamond across the slice with little stimulation.

| Evidence | Observed failure | Implementation investigation |
| --- | --- | --- |
| [1](s23-1.png) | Small pale plate atop an ordinary pad | Ancient Abyss: legacy Restore shrine scaling compresses its Y axis to 20% of intended height. Inspect actual Restore supports, not just generic block art. |
| [2](s23-2.png) | Bright solid shapes obscure the Boost | Ancient Abyss: runner adds opaque direction plates, five vertical bars and a pulse over the existing mechanical asset. |
| [3](s23-3.png) | Repeated pad field fills broad landing | Ancient Abyss: 8 x 8 m Boost landing resolves to ModularTile, repeating the same small pad. |
| [4](s23-4.png) | Nearly touching platforms in Foundry | Foundry: short gaps and similar supports undermine rhythm despite working movement. |

Identification is supported by the visible 76-second Diamond target (Abyss) and
Foundry architecture. Initial informal identification as Spiral was corrected
against ModuleDefinition data. The captures do not provide input traces, complete
run durations, APK hash, thermal data or evidence of which shortcuts were used.
Visible historical PBs are 45.379 seconds (Abyss) and 28.421 seconds (Foundry).
These are evidence against old thresholds, not calibrated targets for new routes.

The broader correction is route rhythm, worthwhile faster lines, clear mechanical
purpose and authored places. The three September concepts in
[VisualReferences](../VisualReferences/README.md) remain aspirational art direction.
Do not treat these four failures as four isolated cosmetic tickets.

Original PNGs copied byte-for-byte from user-provided clipboard files:

- `s23-1.png`: SHA256 `5FE7417A0834C54AAA4CFCD183397FB5084F3EE6FB0EE5E865EC7CAA1179629A`
- `s23-2.png`: SHA256 `BAFEC7D6ED49AFDF6208A05B13DB624DD7F3B8940B878DC6936E23AF31F6CFA3`
- `s23-3.png`: SHA256 `784BA2F5544E4CF56725F25B6A91A813C08C817E6D3FB43381931509172A2D7F`
- `s23-4.png`: SHA256 `BA73CD230A5A5320CEFB17AF69B16A463E84F885D20C0F6A7169484039B9EF77`
