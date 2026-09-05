# Changelog

Each released version gets a section here. The publish pipeline copies the section for the
version being released into the GitHub release notes and into the Tea Time plugin repo, which
is what the in-game changelog (`/xlplugins` -> Changelog) shows. Dalamud renders that text as
plain text, so keep it to short `-` bullets: no tables, no links, no bold.

## 0.1.0.0

First release.

- Auto Priority Aetheryte Pass: uses a pass in the overworld when no teleport-cost
  reduction buff is active. Toggle with /passauto.
- No Jog: cancels the Jog buff the game applies after Sprint expires. Toggle with /nojog.
- Both features are skipped inside duties, and Auto Priority Aetheryte Pass is also
  skipped in PvP zones.
