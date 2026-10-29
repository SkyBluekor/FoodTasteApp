#!/usr/bin/env python3
"""Audit which XML-linked FoodNum images exist in Assets/Images/FoodImg.

This does not change the database, images, or project files.
Run from project root: py -3 tools/food_images/check_image_coverage.py
"""
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
PHOTOS = ROOT / "Assets" / "Images" / "FoodImg"
XML = ROOT / "Assets" / "Resources" / "XML" / "전국통합식품영양성분정보(음식)표준데이터.xml"


def main():
    if not XML.is_file():
        print("ERROR: food XML missing:", XML)
        return 2
    if not PHOTOS.is_dir():
        print("ERROR: FoodImg folder missing:", PHOTOS)
        return 2
    root = ET.parse(XML).getroot()
    records = [e for e in root.iter() if e.tag.rsplit("}", 1)[-1] == "record"]
    food_names = [next((child.text or "" for child in record
        if child.tag.rsplit("}", 1)[-1] == "식품명"), "") for record in records]
    ids = {
        int(p.stem) for p in PHOTOS.iterdir()
        if p.is_file()
        and p.suffix.lower() in {".jpg", ".jpeg", ".png"}
        and p.stem.isdigit()
    }
    expected = set(range(1, len(records) + 1))
    missing = sorted(expected - ids)
    extras = sorted(ids - expected)
    print("Food XML records:", len(records))
    print("Numbered food photos:", len(ids))
    print("Available for FoodNum 1..%d:" % len(records), len(expected & ids))
    print("Missing numbered photos:", len(missing))
    print("Photos outside XML numbering:", len(extras))
    if missing:
        print("First 40 missing photos (FoodNum: FoodName):")
        for food_num in missing[:40]:
            print("  %d: %s" % (food_num, food_names[food_num - 1]))
        print("Missing foods named '카스텔라' / '카스테라':")
        for i, name in enumerate(food_names, 1):
            if ("카스텔라" in name or "카스테라" in name) and i in missing:
                print("  %d: %s" % (i, name))
    print("NOTE: This audits XML numbering, not the live MySQL table.")
    print("      To match displayed food names, Fooddata.FoodNum must correspond")
    print("      to the same numbered images as the original XML import.")
    return 0 if not missing else 1


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (ET.ParseError, OSError) as exc:
        print("ERROR:", exc)
        sys.exit(2)
