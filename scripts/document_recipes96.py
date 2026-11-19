"""Generate reviewable specifications and the evidence matrix from canonical data and actual reports."""
import json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]

def main():
    rows=json.loads((ROOT/'Assets/TweenGallery/Resources/recipe-catalog.json').read_text(encoding='utf-8'))['recipes']
    report_path=ROOT/'docs/validation/recipes96-1600.json'
    reports={c['id']:c for c in json.loads(report_path.read_text(encoding='utf-8'))['cases']} if report_path.exists() else {}
    text=['# 96 条动效场景配方规格','',
          '按原方案的 12 层级 × 8 条组织。每项是独立场景语义，数字编号不传给旧 MotionPlayer.Play。',
          '场景入口为 RecipeGallery.unity。操作直接作用于舞台对象；下方按钮提供可复现的引导操作。',
          '原 46 个预设和旧展馆保留，新的编号不替换旧资源身份。','',
          '“重播表现”不重复提交业务；“重置场景”恢复数据与对象。输入/拖拽/真实时钟没有可回放迁移时，时间轴禁用。',
          '拖拽回弹为解析弹簧；倒计时使用真实截止时间；普通过渡由 DOTween 驱动归一化表现时间。','']
    for row in rows:
        text.extend([f"## {row['id']} {row['name']}",'',
            f"- 层级与素材：{row['layer']}；{row['tags']}。",
            f"- 标签：对象 {', '.join(row['objectTags'])}；状态 {', '.join(row['stateTags'])}；类型 {', '.join(row['typeTags'])}。",
            f"- 初始状态：{row['initialState']}。",
            f"- 明确操作：{row['operation']}。",
            f"- 观察内容：{row['observe']}。",
            f"- 正确结果：{row['finalState']}。",
            f"- 推荐参数：表现基准时长 {row['duration']:.2f} 秒，强度 1，OutCubic；恒速旋转使用线性相位。按钮按下/释放分别使用基准的 0.35 / 0.6。实时输入、倒计时与弹簧使用各自的状态和时间模型。",
            f"- 正常验收：执行上述操作，状态与实际数据一致，能观察到描述的变化。",
            f"- 边界验收：{row['boundary']}。",
            f"- 减弱动态：{row['reducedMotion']}。",
            f"- [运行画面](previews/v080/cases/{row['id']}.png)。",''])
    (ROOT/'docs/recipes-96.md').write_text('\n'.join(text),encoding='utf-8')
    table=['# 96 条配方验收矩阵','',
        '五个阶段分别记录。自动测试覆盖操作结果、至少一个边界、采样不提交业务、减弱动态和关闭清理；不能代替真人交互体验。',
        '人工视觉列等待用户验收；另提供代理对全部场景截图的布局检查记录。目标平台当前指本机 Windows x64，Android / 微信 / 干净第二台电脑未验证。','',
        '| 编号 | 配方 | 设计 | 实现 | 自动测试 | 人工视觉 | 目标平台 |',
        '|---|---|---|---|---|---|---|']
    for row in rows:
        case=reports.get(row['id'],{})
        passed=all(case.get(k,False) for k in ('normal','boundary','sampling','reduced','cleanup'))
        table.append(f"| {row['id']} | {row['name']} | 完成 | 完成 | {'通过' if passed else '待验证'} | 待用户验收 | {'本机 Windows 通过' if passed else '待验证'} |")
    (ROOT/'docs/recipes-96-acceptance.md').write_text('\n'.join(table)+'\n',encoding='utf-8')
    print('Wrote 96 specifications and 96 independent acceptance rows')

if __name__=='__main__':main()
