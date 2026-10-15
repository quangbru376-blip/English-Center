"""Package the report's selected screenshots and four-column control tables.

Uses only the Python standard library; reads the existing screenshot manifest
and Markdown. Does not start the demo, read credentials, or alter source files.
"""
from pathlib import Path
import hashlib
import html
import json
import re
import shutil
import zipfile

ROOT = Path(__file__).resolve().parent
WORKSPACE = ROOT.parent.parent
SOURCE = ROOT.parent / 'report-screenshots-20261015'
DOCUMENT = ROOT / 'MO_TA_27_GIAO_DIEN.md'
HEADER = ['STT', 'Loại', 'Ý nghĩa', 'Biến cố/Xử lý']


def inline(value):
    value = html.escape(value, quote=True)
    value = re.sub(r'!\[([^\]]*)\]\(([^)]+)\)',
                   r'<a class="original" href="\2" target="_blank"><img src="\2" alt="\1" loading="lazy"></a>', value)
    value = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'<a href="\2">\1</a>', value)
    value = re.sub(r'\*\*([^*]+)\*\*', r'<strong>\1</strong>', value)
    return re.sub(r'`([^`]+)`', r'<code>\1</code>', value)


def parse(markdown):
    lines = markdown.splitlines()
    output, sections, table_counts = [], [], []
    active = None
    cursor = 0
    while cursor < len(lines):
        line = lines[cursor].strip()
        if not line:
            cursor += 1
            continue
        if line.startswith('|'):
            table = []
            while cursor < len(lines) and lines[cursor].strip().startswith('|'):
                table.append([c.strip() for c in lines[cursor].strip().strip('|').split('|')])
                cursor += 1
            assert table[0] == HEADER, ('Unexpected table header', active, table[0])
            assert len(table) >= 3
            data = table[2:]
            assert all(len(row) == 4 for row in data), ('Expected four columns', active)
            assert [row[0] for row in data] == [str(i+1) for i in range(len(data))], ('STT', active)
            table_counts.append({'section': active or 'shared', 'rows': len(data)})
            output.append('<div class="table-wrap"><table><colgroup><col class="stt"><col class="kind"><col class="meaning"><col class="event"></colgroup><thead><tr>')
            output.extend('<th scope="col">'+inline(c)+'</th>' for c in table[0])
            output.append('</tr></thead><tbody>')
            for row in data:
                output.append('<tr>'+''.join('<td>'+inline(c)+'</td>' for c in row)+'</tr>')
            output.append('</tbody></table></div>')
            continue
        if line.startswith('# '):
            output.append('<h1>'+inline(line[2:])+'</h1>')
        elif line.startswith('## '):
            title = line[3:]
            match = re.match(r'(GD\d{2})\.\s+(.+)', title)
            if match:
                active = match[1]
                sections.append({'id': active, 'title': match[2]})
                anchor = active
            else:
                active = None
                anchor = 'part-'+str(cursor)
            output.append(f'<h2 id="{anchor}">'+inline(title)+'</h2>')
        elif line.startswith('- '):
            output.append('<p class="source">• '+inline(line[2:])+'</p>')
        else:
            output.append('<p>'+inline(line)+'</p>')
        cursor += 1
    return '\n'.join(output), sections, table_counts


def build():
    manifest = json.loads((SOURCE / 'manifest.json').read_text(encoding='utf-8'))
    images = {record['id']: record for record in manifest['images']}
    text = DOCUMENT.read_text(encoding='utf-8')
    body, sections, tables = parse(text)
    assert [s['id'] for s in sections] == [f'GD{i:02}' for i in range(1,28)]
    assert len(tables) == 28 and len({t['section'] for t in tables}) == 28
    refs = sorted(set(re.findall(r'images/(\d{3})\.png', text)))
    (ROOT / 'images').mkdir(exist_ok=True)
    selected = []
    for image_id in refs:
        record = images[image_id]
        source = SOURCE / record['file']
        expected = record['sha256']
        assert hashlib.sha256(source.read_bytes()).hexdigest() == expected, image_id
        target = ROOT / 'images' / (image_id+'.png')
        shutil.copyfile(source, target)
        assert hashlib.sha256(target.read_bytes()).hexdigest() == expected
        selected.append({'id': image_id, 'file': 'images/'+image_id+'.png', 'sourceFile': record['file'], 'label': record['label'], 'sha256': expected})
    main_ids = re.findall(r'!\[GD\d{2}[^\]]*\]\(images/(\d{3})\.png\)', text)
    assert len(main_ids) == 27 and len(set(main_ids)) == 27
    assert all(images[i]['mode'] == 'desktop' for i in main_ids)
    source_paths = sorted(set(re.findall(r'\]\((\.\./[^)]+)\)', text)))
    sources = []
    for relative in source_paths:
        source = (ROOT / relative).resolve()
        assert source.is_file(), relative
        sources.append({'path': str(source.relative_to(WORKSPACE)).replace('\\','/'), 'sha256': hashlib.sha256(source.read_bytes()).hexdigest()})
    summary = {'date':'2026-10-15', 'mainScreens':27, 'tables':len(tables), 'controlRows':sum(t['rows'] for t in tables), 'tablesByScreen':tables, 'imagesCopied':len(refs), 'mainImageIds':main_ids, 'selectedImages':selected, 'sources':sources, 'scope':'Documentation only; no demo commands, credential reads, Word edits or product changes.'}
    (ROOT/'selection.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    toc = '<nav><h2>27 giao diện chính</h2><ol>'+''.join(f'<li><a href="#{s["id"]}">{s["id"]} — {html.escape(s["title"])}</a></li>' for s in sections)+'</ol><p><a href="MO_TA_27_GIAO_DIEN.md">Bản Markdown</a> · <a href="README.md">Hướng dẫn</a></p><button onclick="window.print()">In tài liệu</button></nav>'
    css = '''*{box-sizing:border-box}html{scroll-behavior:smooth}body{margin:0;color:#1d3035;background:#f4f7f6;font:16px/1.65 system-ui,sans-serif}nav{position:fixed;inset:0 auto 0 0;width:290px;overflow:auto;background:#fff;border-right:1px solid #d6e2df;padding:22px;font-size:13px}nav h2{font-size:18px}nav ol{padding-left:20px}nav li{margin-bottom:8px}nav a{color:#126c6c;text-decoration:none}nav button{padding:10px;background:#0c7676;color:#fff;border:0;border-radius:6px;cursor:pointer}main{max-width:1400px;margin-left:290px;padding:36px 4vw 80px}h1{font-size:29px;line-height:1.3}h2{scroll-margin-top:20px;border-top:2px solid #d1dfdb;padding-top:30px;margin-top:55px;font-size:23px;color:#0b6666}a{color:#086e71}.original{display:block;max-height:620px;overflow:auto}img{display:block;width:100%;height:auto;max-width:100%;margin:0 auto;border:1px solid #d2dfdc;background:#fff}p:has(>.original){background:#e7eeeb;border-radius:8px;padding:10px}.table-wrap{overflow-x:auto;background:#fff}table{width:100%;border-collapse:collapse;font:15px/1.6 Georgia,"Times New Roman",serif;min-width:700px}th,td{border:1px solid #485b5d;padding:9px 11px;text-align:left;vertical-align:top}th{background:#e9f1ee;font-weight:bold}.stt{width:6%}.kind{width:15%}.meaning{width:30%}.event{width:49%}tbody td:first-child{text-align:center}code{font-size:.9em}.source{font-size:14px}@media(max-width:1000px){nav{position:static;width:auto;max-height:300px}main{margin-left:0;padding:24px}nav ol{columns:2}table{font-size:14px}}@media print{@page{size:A4 portrait;margin:15mm}body{background:#fff;font:12pt/1.4 "Times New Roman",serif;color:#000}nav{display:none}main{margin:0;padding:0;max-width:none}h1{font-size:20pt}h2{font-size:16pt;break-before:page;border:0;margin:0;padding:0;color:#000}.original{max-height:none;overflow:visible}img{width:auto;max-height:110mm;border:1px solid #777}p:has(>.original){background:none;padding:0}.table-wrap{overflow:visible}table{min-width:0;font:11pt/1.4 "Times New Roman",serif}th,td{padding:5px}th{background:none}thead{display:table-header-group}tr{break-inside:avoid}a{color:#000;text-decoration:none}.source{font-size:10pt}}'''
    document = '<!doctype html><html lang="vi"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>27 giao diện quan trọng — mô tả thành phần</title><style>'+css+'</style></head><body>'+toc+'<main>'+body+'</main></body></html>'
    (ROOT/'index.html').write_text(document, encoding='utf-8')
    lines = ['# Bộ 27 giao diện quan trọng và bảng mô tả thành phần','',
             'Mở [bản xem có ảnh và bảng](index.html) hoặc [bản Markdown để biên soạn](MO_TA_27_GIAO_DIEN.md).', '',
             f'Có **27 giao diện chính**, **27 bảng riêng + 1 bảng dùng chung**, **{summary["controlRows"]} dòng mô tả** theo đúng bốn cột STT – Loại – Ý nghĩa – Biến cố/Xử lý. Đủ khách và bảy vai trò. Đây là 27 mục giao diện, không cam kết tài liệu in chỉ dài 27 trang.', '',
             f'Thư mục `images` có **{len(refs)} ảnh được chọn**, gồm 27 ảnh chính và các ảnh bổ sung/ảnh thành phần chung. Ảnh sao chép nguyên byte từ bộ 117 ảnh; mã ảnh nguồn được giữ để đối chiếu. Bấm ảnh trong HTML để mở PNG gốc. Các trạng thái bổ sung được giải thích trong cùng mục, không tăng thành giao diện hoặc use case mới.', '',
             '| Mã | Giao diện chính | Ảnh nguồn |','|---|---|---|']
    for s, image_id in zip(sections, main_ids):
        lines.append(f'| {s["id"]} | {s["title"]} | [Ảnh {image_id}](images/{image_id}.png) |')
    lines += ['', '## Cách dùng trong báo cáo', '',
              'Chèn ảnh chính rồi đặt bảng ngay dưới. Có thể sao chép bảng từ HTML sang Word và chọn giữ định dạng. Chỉ thêm ảnh phụ khi cần giải thích trạng thái tạo/sửa/xác nhận. Với ảnh quá dài, chia hình thành a/b trong cùng mục. Bản máy tính là nội dung chính; bộ 117 ảnh vẫn giữ các ảnh điện thoại để đưa vào phần thích ứng thiết bị.', '',
              'Nội dung đã đối chiếu handler/điều hướng trong mã nguồn; không ghi các nút xem chi tiết là luôn chuyển trang, hoặc các bộ lọc là luôn tự truy vấn. Các nút lưu/xác nhận được mô tả từ mã, không được thực thi để tạo tài liệu. Word gốc, sản phẩm và dữ liệu demo không bị sửa.', '',
              'Bản thảo cần chủ dự án duyệt câu chữ trước khi đưa vào Word cuối. Khoảng thiếu UI NV04 và các trạng thái chưa có ảnh được ghi trong tài liệu chi tiết. Không bổ sung thành phần tưởng tượng vào ảnh.', '',
              '## Đối chiếu và tái tạo', '',
              '`selection.json` lưu 27 ảnh chính, hash ảnh đã sao chép, số dòng bảng và hash các nguồn đối chiếu. Hai mươi bảy bảng riêng có STT bắt đầu lại từ 1. Liên kết nguồn trong Markdown dùng khi mở ở workspace; ảnh/bảng của bộ ZIP là tự chứa.', '',
              'Chạy `python build_guide.py` trong workspace để tạo lại HTML, mục lục, bản chọn ảnh và ZIP từ Markdown/bộ ảnh hiện có. Chỉ dùng thư viện chuẩn Python; không mở demo hoặc đọc thông tin đăng nhập. Sau khi giải nén riêng, mở `index.html` để đọc; cần workspace gốc nếu muốn tái tạo.', '']
    (ROOT/'README.md').write_text('\n'.join(lines), encoding='utf-8')
    archive = ROOT.parent/(ROOT.name+'.zip')
    with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
        for file in sorted(ROOT.rglob('*')):
            if file.is_file() and '__pycache__' not in file.parts:
                z.write(file, file.relative_to(ROOT.parent))
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None
    print(json.dumps({k:summary[k] for k in ['mainScreens','tables','controlRows','imagesCopied']}, ensure_ascii=False))


if __name__ == '__main__':
    build()
