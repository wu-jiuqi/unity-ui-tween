# Unity UI Motion Gallery — implementation contract

Unity 6000.0.26f1, UGUI, DOTween free, built-in render pipeline. Namespace `TweenGallery`.

## 0.6 presentation layer

The main catalog presents 50 choices: 46 stable MotionCatalog IDs, G01/G02 input recipes, and P01/P02 page recipes. G/P IDs are gallery definitions, never passed to MotionPlayer.Play. All controls remain in the main gallery. Legacy lab/workshop fixtures are enabled only by explicit test flags.

MotionStyleProfile clones canonical MotionSettings into restrained/standard/celebratory variants. Exported values are effective; profile metadata must not be applied a second time. MotionPageSequence owns a deterministic staged visual clock, silent Seek/Sample, pause/resume, exact baseline restore and a visual-only completion event. It never grants rewards. MotionResponsiveLayout repositions existing controls within host-supplied safe insets, retains scroll state and leaves selection/business state with the host. See docs/motion-profiles.md, page-recipes.md and responsive-layout.md.

Cream/gold raster nine-slice panels, a transparent medal and a four-cell reward atlas are in Resources/StyleArt. Native text and hit regions remain independent. These are generated PNG assets, with exact prompts and hashes under ArtSource/Style-System-v060. Gallery-only art is excluded from the runtime package. The approved concept boards are retained under ArtSource/Skill-Next-Concepts-v1.

## Shared catalog
`Assets/TweenGallery/Resources/motion-catalog.json`: wrapper `{ "effects": [...] }`. Each entry has `id`, `name` (Chinese), `english`, `category` (A/B/C/D/E), `kind`, `description`, `useCase`, `difference`, `dependencies`, `binding`, `duration` (float), `strength` (float, default 1), `distance` (float), `stagger` (float), `ease` (string), `prompt`.
IDs: A01 Fade, A02 Slide, A03 Scale, A04 Pop, A05 Bounce, A06 Flip In, A07 Reveal, A08 Blur; B01 Press, B02 PressBounce, B03 Pulse, B04 Shake, B05 Wiggle, B06 Punch, B07 Selection, B08 Toggle; C01 DropSettle, C02 Drawer, C03 Collapse, C04 SharedElement, C05 SharedAxis, C06 FadeThrough, C07 Crossfade; D01 Count, D02 Fill, D03 Fly, D04 BurstCollect, D05 SequentialCount, D06 Stars, D07 CardFlip, D08 Chest, D09 FloatingText, D10 Impact; E01 Float, E02 Rotate, E03 Shine, E04 Blink, E05 Stagger, E06 Trail, E07 Gesture.

## Runtime API (runtime agent owns all Runtime/*.cs and Shaders/*)
`[Serializable] MotionDefinition` contains catalog fields above. `MotionCatalog.All` returns `MotionDefinition[]`; `MotionCatalog.Find(string id)`; `MotionSettings.FromDefinition(MotionDefinition)`.
`[Serializable] MotionSettings`: `float duration, strength, distance, stagger; string ease; bool reverse, loop, unscaledTime = true; float speed = 1`.
`[Serializable] MotionBindings`: `RectTransform target, secondary, destination, mask, shine, lid; CanvasGroup group, secondaryGroup; Image image, secondaryImage, fill; Text label; RectTransform[] items; Text[] labels; CanvasGroup[] itemGroups`.
`MotionPlayer : MonoBehaviour`: public `MotionBindings bindings`, `MotionSettings settings`, `string effectId`; `Play(string id, MotionSettings options = null)`, `Pause()`, `Resume()`, `StopAndRestore()`, `CaptureBaseline()`, `Sample(float normalizedTime)` for deterministic tests (pauses tween, applies same evaluator); `bool IsPlaying`, `float Progress`. Owns only its tween. Captures/restore position/scale/rotation/size/colors/groups/fill/labels; disable/destroy cleans and disable restores. Reentrant Play returns to captured baseline then starts. No global DOTween.KillAll or changes to global timeScale. Snapshot all bound elements. Hidden groups block no raycasts. Use tweening normalized time + deterministic evaluator to make sample/test easy. Extra shaders only when required, blur is actual texture blur with `_Blur` material parameter (document only Image texture, not arbitrary live hierarchy). Outline invalid binding errors gracefully. B01/B02 runtime pointer binding component needed for true press/release input with simple attach workflow; generic Play demo can cycle press/release.

## Gallery API (gallery agent owns Gallery/*.cs)
`GalleryBootstrap : MonoBehaviour`: builds complete gallery in Start. Must NOT auto-bootstrap empty scenes at runtime; editor builder attaches to scene.
`DemoFactory.Create(string effectId, Transform parent, Vector2 size)` returns configured `MotionPlayer` with all required bindings and baseline captured; rooted under parent; creates attractive high saturation game UI fixtures in consistent style. Individual effects distinct. No motion algorithm in factory: shared MotionPlayer only.
`GalleryUI` / bootstrap left search/category/scroll list, center demo (switch single and 3-up compare), right controls, bottom use/copy prompt. Dynamic Canvas 1600x900 with scale-with-screen-size. Chinese font loaded `Resources.Load<Font>("GalleryFont")`, fallback LegacyRuntime.ttf. Current visual design uses cream panels, cocoa text, mint, lavender and warm gold, with native Chinese text. Buttons pause/resume/replay, slow 0.25x/1x, loop, reverse for supported effects, restore defaults; duration/strength/distance/stagger sliders; ease dropdown; copy exact current parameters + ID prompt. Compare groups include A03/A04/A05 and B04/B05/B06 and C05/C06/C07. Desktop validation at 1280x720 and 1600x900. Provide Escape exit.

## Editor/build/validation (root owns Editor/*.cs and scripts)
Build method creates scene and sample prefabs/presets, validates representative effects first, then builds Win64 Mono. Runtime self-test coroutine attached separately by root, enabled command line `--self-test` with output directory arg; no automatic test on normal run. Records 40-ID behavior, repeat/restore/disable/destroy, screenshot evidence, exit code. Human/external clean-machine tests remain explicitly unverified if unavailable.

## Deliverables
Portable Windows zip, Unity source zip with dependencies/license, runtime .unitypackage, self-contained skill zip including runtime/assets docs, Chinese documentation. No existing project overwritten. Publish repository must be confirmed if ambiguous.
