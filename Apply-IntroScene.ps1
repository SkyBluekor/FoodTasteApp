# FoodTasteApp IntroScene artwork installation - does NOT require git, and does NOT alter scripts or DB.
# Run this script from the Unity project root after extracting the included ZIP.
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$scene = Join-Path $projectRoot 'Assets\Scenes\IntroScene.unity'
$art = Join-Path $projectRoot 'Assets\Resources\FoodTasteIntro'
$required = @{
    'IntroBackground.png' = 'feadc0a56ccd4315a70b6dca9781b2c9'
    'FoodTasteAppLogo.png' = '2fc547f07580486fb687febceb64c2b1'
    'IdLabel.png' = '106262f2762241f588162602ed983ed2'
    'PasswordLabel.png' = 'a07c638b92154d71b79d9b0ea210d4c4'
    'SignupButton.png' = '947fe45043e44da7ba36d171528fae28'
    'LoginButton.png' = 'dd814e235f4b414cbb03ad152d3a707d'
}
if (-not (Test-Path -LiteralPath $scene)) { throw "IntroScene not found: $scene" }
foreach ($name in $required.Keys) {
    $png = Join-Path $art $name
    $meta = "$png.meta"
    if (-not (Test-Path -LiteralPath $png)) { throw "Image missing: $png" }
    if (-not (Test-Path -LiteralPath $meta)) { throw "Unity .meta missing: $meta" }
    $metaContent = [System.IO.File]::ReadAllText($meta)
    if (-not $metaContent.Contains("guid: $($required[$name])")) { throw "Unexpected GUID in $meta" }
}
$url = 'https://raw.githubusercontent.com/SkyBluekor/FoodTasteApp/main/Assets/Scenes/IntroScene.unity'
$temp = Join-Path $env:TEMP ('FoodTasteIntroScene_' + [guid]::NewGuid().ToString('N') + '.unity')
try {
    Invoke-WebRequest -Uri $url -OutFile $temp -UseBasicParsing
    $text = [System.IO.File]::ReadAllText($temp)
    if (-not $text.Contains('--- !u!1 &129456995')) { throw 'Downloaded scene does not contain the expected IntroPanel.' }
    foreach ($guid in $required.Values) {
        if (-not $text.Contains("guid: $guid")) { throw "Downloaded scene missing sprite GUID: $guid" }
    }
    if ($text -notmatch '--- !u!114 &699061144[\s\S]*?m_Enabled: 1') { throw 'New title image is not enabled.' }
    $backupDir = Join-Path $projectRoot 'Library\FoodTasteIntroBackup'
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    $backup = Join-Path $backupDir ('IntroScene_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '.unity')
    Copy-Item -LiteralPath $scene -Destination $backup -Force
    Copy-Item -LiteralPath $temp -Destination $scene -Force
    $installed = [System.IO.File]::ReadAllText($scene)
    foreach ($guid in $required.Values) {
        if (-not $installed.Contains("guid: $guid")) { throw "Verification failed for $guid" }
    }
    Write-Host '[PASS] Actual IntroScene.unity has been replaced with the GitHub artwork scene.' -ForegroundColor Green
    Write-Host "[BACKUP] $backup"
    Write-Host 'Open Unity and reload Assets/Scenes/IntroScene.unity. Game view: 1920x1080.'
    Write-Host 'No Tools menu action is required.'
}
finally { Remove-Item -LiteralPath $temp -ErrorAction SilentlyContinue }
