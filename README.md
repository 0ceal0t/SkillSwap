# SkillSwap

A [XIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher) plugin to create animation replacement mods. Mods can be exported either as a Textools modpack or Penumbra mod

Icon by [PAPACHIN](https://www.xivmodarchive.com/user/192152)

## Issues with certain skills
SkillSwap does some automatic renaming of animation ids to prevent conflicts. However, sometimes it doesn't work. If you are having issues, consider using [VFXEditor](https://github.com/0ceal0t/Dalamud-VFXEditor) instead, which allows you to manually swap `.tmb` and `.pap` files. Note that you may need to manually change some pap animation ids.

## Usage
1. Install using `/xlplugins`
2. Open menu using `/skillswap`

![skillswapdemo](https://user-images.githubusercontent.com/18051158/123883902-f56d7a80-d917-11eb-8536-abd12629e545.png)

## Notes
1. Don't modify movement actions (jumps, dashes, backflips, etc.), as this is potentially detectable.
2. Some combinations of actions can't be swapped because their structure doesn't quite line up (for example, one might have a `.pap` animation file associated with, while another does not)
3. Seriously, don't modify movement actions