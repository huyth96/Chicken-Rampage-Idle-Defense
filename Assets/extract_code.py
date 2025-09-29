#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Quét toàn bộ .cs trong Assets/Scripts,
ghi ra 1 file txt ngay trong thư mục Scripts.
"""

import os
from datetime import datetime

ROOT_DIR = "Scripts"  # chạy script từ Assets => chỉ cần "Scripts"
EXTS = [".cs"]
OUTPUT_FILE = os.path.join(ROOT_DIR, "unity_scripts_dump.txt")

def collect_files(root):
    for cur, dirs, files in os.walk(root):
        for f in files:
            if os.path.splitext(f)[1].lower() in EXTS:
                yield os.path.join(cur, f)

def read_text(path):
    encs = ["utf-8-sig", "utf-8", "utf-16", "latin-1"]
    for e in encs:
        try:
            with open(path, "r", encoding=e) as fp:
                return fp.read()
        except Exception:
            continue
    with open(path, "r", encoding="utf-8", errors="replace") as fp:
        return fp.read()

def main():
    if not os.path.isdir(ROOT_DIR):
        raise SystemExit(f"❌ Không tìm thấy thư mục {ROOT_DIR}")

    files = sorted(collect_files(ROOT_DIR))
    total_lines = total_bytes = 0

    with open(OUTPUT_FILE, "w", encoding="utf-8", newline="\n") as out:
        out.write("========== UNITY SCRIPT DUMP ==========\n")
        out.write(f"Time : {datetime.now().isoformat(timespec='seconds')}\n")
        out.write(f"Root : {ROOT_DIR}\n")
        out.write(f"Ext  : {', '.join(EXTS)}\n")
        out.write("=======================================\n\n")

        for idx, path in enumerate(files, 1):
            rel = os.path.relpath(path)
            text = read_text(path)
            size = os.path.getsize(path)
            lines = text.count("\n") + (0 if text.endswith("\n") else 1)

            total_lines += lines
            total_bytes += size

            out.write(f"# ==== FILE {idx}/{len(files)}: {rel}\n")
            out.write(f"# SIZE: {size} bytes | LINES: {lines}\n")
            out.write("# ---- BEGIN ----\n")
            out.write(text)
            if not text.endswith("\n"):
                out.write("\n")
            out.write("# ---- END ----\n\n")

        out.write("========== SUMMARY ==========\n")
        out.write(f"{len(files)} file(s), {total_lines} lines, {total_bytes} bytes\n")

    print(f"✅ Đã xuất {len(files)} file vào {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
