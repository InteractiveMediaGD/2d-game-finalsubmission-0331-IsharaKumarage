$b = "$env:USERPROFILE\.gemini\antigravity\brain\4b2d56ec-f333-4cd0-b540-12322aaee8f2"
$a = "d:\Unity Projects\2D Game Assignment\Assets"

$items = @(
    @{ src = "broken_warrior_logo_1774377142789.png";  dst = "$a\Art\UI\BrokenWarrior_Logo.png" },
    @{ src = "dojo_background_1774377157421.png";      dst = "$a\Art\Backgrounds\DojoBG.png" },
    @{ src = "fighter_spritesheet_1774377172723.png";  dst = "$a\Art\Characters\FighterSpriteSheet.png" },
    @{ src = "health_bar_ui_1774377188622.png";        dst = "$a\Art\UI\HealthBarUI.png" },
    @{ src = "hit_effects_1774377222036.png";          dst = "$a\Art\Effects\HitEffects.png" },
    @{ src = "menu_buttons_1774377237701.png";         dst = "$a\Art\UI\MenuButtons.png" }
)

foreach ($item in $items) {
    $srcFull = Join-Path $b $item.src
    Copy-Item -LiteralPath $srcFull -Destination $item.dst -Force
    Write-Output "OK: $($item.src)"
}
Write-Output "All assets copied."
