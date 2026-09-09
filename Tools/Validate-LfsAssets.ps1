$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$missing = @()
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets') -Recurse -File |
    Where-Object { $_.Extension -in '.fbx','.glb','.zip','.mp4','.wav' -and $_.Length -lt 1024 } |
    ForEach-Object {
        if ([System.IO.File]::ReadAllText($_.FullName).StartsWith('version https://git-lfs.github.com/spec/v1')) {
            $missing += $_.FullName
        }
    }
if ($missing.Count -gt 0) {
    throw "Unhydrated Git LFS assets. Run git lfs pull and git lfs checkout before Unity:`n$($missing -join "`n")"
}
Write-Output 'Git LFS asset hydration: PASS'
