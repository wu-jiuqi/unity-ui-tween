"""Persist the normalized production prompt specifications beside the unmodified references."""
import json
from pathlib import Path
from register_recipe_art import SHEETS

ROOT=Path(__file__).resolve().parents[1]
style=('Quiet charcoal workbench frame with a large central demonstration stage; demo assets in saturated cobalt, cyan, orange, emerald, purple and gold. '
       'Premium chunky rounded painted 3D casual-game material, upper-left lighting, crisp silhouettes, controlled bevels, subtle short shadows. '
       'No pastel treatment. Dynamic text stays native and all independently animated parts must be separate.')
prototypes={
 'controls':'Game buttons in normal, pressed, selected and disabled states; independent icons, toggle track and knob, checkbox, radio, focus input and error, slider and 80 value bubble. Chinese context: 按钮与输入, 开始挑战, 领取奖励, 已禁用, 输入名字.',
 'surfaces':'Layered mission panel with header, task rows and buttons; side drawer, anchored dropdown and compact tooltip; navigation tabs and selection line; health, XP and quest progress. Chinese context: 面板与导航, 星光旅程, 探索任务, 任务详情.',
 'cards':'Portrait collectible fox-explorer card front with cobalt cape/orange scarf/cyan crystal; purple compass card back, independent rarity gem and gloss; inventory item and source/valid/invalid sockets. Chinese context: 卡牌与拖放, 星光旅者, 源槽, 可放置, 类型不匹配.',
 'rewards':'Blue-gold hinged chest, independent lid and body, coins and crystal; gold/unlit rating stars, trophy medal, level badge, ranking rows and wallet; spinner, hand, gleam and halo. Chinese context: 奖励与反馈, 旅程完成, 成就解锁, 等级提升, 继续.'
}
atlas_common=('Use attached approved prototype as style/shape reference. Generate transparent PNG sprite atlas only; front-facing orthographic assets, no complete interface, no grid lines or cell labels. '
              'One complete independent asset per equal cell with clear alpha gutter. Preserve the prototype material/light. '
              'No text or numbers on surfaces. All central plates solid and opaque; intentional halo/outline holes and gloss remain transparent. '
              'Read parts row-major. Chest body/lid, card frame/art/back/gem/gloss and control track/knob/tick are separate; do not combine.')
atlas_specs=[]
for key,parts in SHEETS.items():
    prototype='approved-reference' if key.startswith('approved-') else 'controls' if key in ('buttons','inputs','objects') else 'surfaces' if key in ('surfaces','bars') else 'cards' if key=='cards' else 'rewards'
    atlas_specs.append(dict(atlas=key,reference='approved-reference.png' if prototype=='approved-reference' else f'prototypes/{prototype}.png',columns=4 if len(parts)==16 else 3,rows=4 if len(parts)==16 else 3,
                            promptSpecification=atlas_common,parts=parts))
document=dict(format='Normalized production prompt specifications; full invocation wording remains in the authoring chat',tool='built-in imagegen',style=style,
              authority='approved-reference.png is the exact user-specified card-and-drag prototype. Its portrait and small tool icons use unmodified Sprite crops.',
              prototypes=[dict(name=k,promptSpecification=style+' '+v) for k,v in prototypes.items()],atlases=atlas_specs,
              standalone=[dict(name='approved-pointer',reference='approved-reference.png',promptSpecification='Only the exact cyan curved down-left drag arrow, actual transparent alpha, no other objects.'),dict(name='approved-ghost',reference='approved-reference.png',promptSpecification='Only the thin cyan dashed closed rounded-rectangle ghost slot outline, not four corner brackets; actual transparent center and background.')])
(ROOT/'ArtSource/Recipes-v070/prompts.json').write_text(json.dumps(document,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f'Recorded four prototypes, {len(atlas_specs)} atlases and two standalone production specifications')
