# Runtime-only integration package (unreleased work)

The default full Skill ZIP is unchanged in scope: it contains all 46 prefabs,
all presets, gallery code, scene, fonts, art, licenses and Unity `.meta` files.
Use it to explore or reuse the supplied visual templates.

For an existing game's own UI, build the smaller package:

```powershell
python -X utf8 scripts/package_skill.py --profile runtime --output validation-output/runtime.zip
```

This opt-in profile includes all Runtime scripts, Optional/TMP (disabled unless
`TWEEN_GALLERY_TMP` is defined), Shaders, 46 Presets, and
`Resources/motion-catalog.json`, with their original `.meta` GUIDs. It also
contains the Skill instructions and canonical integration references. It does
not include Gallery, Editor, Scenes, Prefabs, GeneratedArt, gallery fonts,
MotionArt, sound clips, or any other gallery Resources. No source assets are
deleted or renumbered. This is a runtime-integration source ZIP, not a Unity
Package Manager package or a runnable gallery.

1. In a backed-up branch of the target project, install/verify UGUI and DOTween
   free. Do not import a second DOTween copy. Baseline: Unity 6000.0.26f1,
   UGUI 2.0.0 and DOTween 1.3.045; other versions need their own validation.
2. Copy `unity-ui-tween/assets/TweenGallery` and `TweenGallery.meta` into
   `Assets/`, preserving all `.meta` files. If that folder exists, review the
   differences first; do not blindly overwrite another version. A runtime
   overlay does not remove old gallery assets already present in a project.
3. Bind your own UGUI elements. Follow [integration.md](integration.md) for the
   minimum A04 example, layout baseline capture, target ownership, cancellation,
   and effect-specific bindings. `MotionCatalog` requires the supplied catalog.
   Font and sprite selection belong to your game. Audio clips for
   `MotionSoundTimeline` must also be supplied by your game.
4. If using A08, retain `TweenGallery/UITextureBlur` in the player build via an
   included material or Always Included Shaders. It blurs one Image texture,
   not a whole page or scene. Do not run GalleryBuild.Prepare in your game.
5. Compile and test repeated play, interruption, disable/re-enable, pause,
   intended end states, and an actual target-platform build. ZIP validation
   does not establish Unity compilation or device/performance correctness.

The runtime package deliberately cannot provide the 46 demo prefabs. If a
Skill instruction asks for a supplied prefab, switch to the full package; do
not claim the demo assets are present in this profile. Both profiles preserve
the same A01-F06 IDs and full runtime behavior; the lean profile is not a
per-effect code stripper. See [skill-packaging.md](skill-packaging.md) for
reproducibility and validation commands.
