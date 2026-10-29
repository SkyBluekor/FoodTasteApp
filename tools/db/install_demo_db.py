#!/usr/bin/env python3
"""Non-destructive FoodTasteApp local MySQL demo installer (Python stdlib only)."""
import argparse
import datetime
import getpass
import glob
import os
from pathlib import Path
import random
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

PROJECT = Path(__file__).resolve().parents[2]
XML = PROJECT / "Assets" / "Resources" / "XML" / "전국통합식품영양성분정보(음식)표준데이터.xml"
SCHEMA = Path(__file__).with_name("schema.sql")


def mysql_executable(override):
    candidates = [override, shutil.which("mysql"), shutil.which("mariadb")]
    if os.name == "nt":
        for pattern in (
            r"C:\Program Files\MySQL\MySQL Server *\bin\mysql.exe",
            r"C:\Program Files\MariaDB *\bin\mysql.exe",
            r"C:\xampp\mysql\bin\mysql.exe",
            r"C:\wamp64\bin\mysql\mysql*\bin\mysql.exe",
        ):
            candidates.extend(glob.glob(pattern))
    for path in candidates:
        if path and Path(path).is_file():
            return str(path)
    raise RuntimeError("mysql.exe not found. Install and start local MySQL Server, or use --mysql PATH.")


def literal(value):
    """Encode text as UTF-8 SQL hex expression to avoid quoting ambiguity."""
    return "CONVERT(0x" + str(value).encode("utf-8").hex() + " USING utf8mb4)"


def extract_foods():
    if not XML.exists():
        raise RuntimeError("Original food XML missing: " + str(XML))
    root = ET.parse(XML).getroot()
    result = []
    for node in root.iter():
        if node.tag.rsplit("}", 1)[-1] != "record":
            continue
        fields = {
            child.tag.rsplit("}", 1)[-1]: (child.text or "").strip()
            for child in node
        }
        name = fields.get("식품명", "").strip()
        if name:
            result.append((
                len(result) + 1, name[:255],
                fields.get("에너지kcal", "")[:64],
                fields.get("식품중량", "")[:128],
            ))
    if len(result) < 160:
        raise RuntimeError("Too few food records in XML: " + str(len(result)))
    return result


def execute(exe, password, host, port, sql):
    env = os.environ.copy()
    env["MYSQL_PWD"] = password  # Password is not included in command line.
    args = [
        exe, "--protocol=tcp", "--host=" + host, "--port=" + str(port),
        "--user=root", "--default-character-set=utf8mb4",
        "--batch", "--raw", "--skip-column-names",
    ]
    p = subprocess.run(args, input=sql, text=True, encoding="utf-8",
                       errors="replace", capture_output=True, env=env)
    if p.returncode != 0:
        raise RuntimeError((p.stderr or p.stdout or "MySQL error").strip()[:2000])
    return p.stdout.strip()


def import_foods(run, foods):
    for start in range(0, len(foods), 100):
        values = ",".join(
            "(" + str(n) + "," + literal(name) + "," +
            literal(kcal) + "," + literal(gram) + ")"
            for n, name, kcal, gram in foods[start:start + 100]
        )
        run("USE tasteapp; INSERT IGNORE INTO fooddata "
            "(FoodNum,FoodName,FoodKcal,FoodGram) VALUES " + values + ";")


def add_demo(run, foods):
    rng = random.Random(20261029)
    foodnames = {n: name for n, name, _, _ in foods}
    size = min(160, len(foods))
    today = datetime.date.today()
    statements = ["USE tasteapp;"]

    for i in range(1, 21):
        uid = "demo%02d" % i
        nick = "데모사용자%02d" % i
        region = ("부산광역시", "서울특별시", "경기도", "대구광역시", "인천광역시")[i % 5]
        statements.append(
            "INSERT IGNORE INTO user (Id,Pwd,NickName,Age,Region) VALUES "
            "(" + literal(uid) + "," + literal("demo1234") + "," +
            literal(nick) + "," + str(20 + (i * 3) % 24) + "," +
            literal(region) + ");"
        )
        shared = set(range(1, 13))
        selected = set(rng.sample(range(13, size + 1), 40))
        group = (i - 1) % 4
        rated = []
        for food_num in sorted(shared | selected):
            category = food_num % 4
            base = (5, 4, 2, 3)[(category - group) % 4]
            rating = max(1, min(5, base + rng.choice((-1, 0, 0, 0, 1))))
            rated.append("(" + literal(uid) + "," + str(food_num) + "," +
                         str(rating) + "," + literal(foodnames[food_num]) + ")")
        statements.append(
            "INSERT IGNORE INTO foodtastescore "
            "(id,foodnum,foodscore,foodname) VALUES " + ",".join(rated) + ";"
        )

        for days in (0, 1, 2, 3, 5):
            date = (today - datetime.timedelta(days=days)).isoformat()
            for k, meal in enumerate(("morning", "afternoon", "night")):
                food_num = 1 + (((i - 1) * 7 + days * 11 + k * 3) % size)
                statements.append(
                    "INSERT IGNORE INTO search_history "
                    "(user_id,tag,foodname,search_date,eaten_tag) VALUES (" +
                    literal(uid) + ",'eaten'," + literal(foodnames[food_num]) +
                    ",'" + date + "','" + meal + "');"
                )
            food_num = 1 + (((i - 1) * 11 + days * 7) % size)
            statements.append(
                "INSERT IGNORE INTO search_history "
                "(user_id,tag,foodname,search_date,eaten_tag) VALUES (" +
                literal(uid) + ",'search'," + literal(foodnames[food_num]) +
                ",'" + date + "','');"
            )

        score = 12 + ((i - 1) * 7) % 53
        statements.append(
            "INSERT INTO game_scores (player_id,score) VALUES (" +
            literal(uid) + "," + str(score) + ") ON DUPLICATE KEY UPDATE "
            "score=GREATEST(score,VALUES(score));"
        )

    for i in range(1, 10):
        a, b = "demo%02d" % i, "demo%02d" % (i + 1)
        statements.append(
            "INSERT INTO friendships(user_id_1,user_id_2,status) "
            "SELECT " + literal(a) + "," + literal(b) +
            ",'acceptance' WHERE NOT EXISTS "
            "(SELECT 1 FROM friendships WHERE "
            "(user_id_1=" + literal(a) + " AND user_id_2=" + literal(b) + ") OR "
            "(user_id_1=" + literal(b) + " AND user_id_2=" + literal(a) + "));"
        )
    for i in range(10, 15):
        a, b = "demo%02d" % i, "demo%02d" % (i + 1)
        statements.append(
            "INSERT INTO friend_requests(sender_id,receiver_id,status) "
            "SELECT " + literal(a) + "," + literal(b) +
            ",'pending' WHERE NOT EXISTS (SELECT 1 FROM friend_requests WHERE "
            "sender_id=" + literal(a) + " AND receiver_id=" + literal(b) + ");"
        )
    for i in range(1, 8):
        a, b = "demo%02d" % i, "demo%02d" % (i + 1)
        message = str(i) + "번 음식 좋아하세요?"
        statements.append(
            "INSERT INTO messages(sender_id,receiver_id,message,send_at) "
            "SELECT " + literal(a) + "," + literal(b) + "," +
            literal(message) + ",NOW() WHERE NOT EXISTS "
            "(SELECT 1 FROM messages WHERE sender_id=" + literal(a) +
            " AND receiver_id=" + literal(b) +
            " AND message=" + literal(message) + ");"
        )
    for i in range(1, 9):
        uid = "demo%02d" % i
        foodname = foodnames[1 + ((i * 3) % size)]
        content = "데모 리뷰 %d: 다음에도 먹고 싶어요." % i
        statements.append(
            "INSERT INTO comments(foodname,id,replydate,content) "
            "SELECT " + literal(foodname) + "," + literal(uid) +
            ",NOW()," + literal(content) +
            " WHERE NOT EXISTS (SELECT 1 FROM comments WHERE id=" +
            literal(uid) + " AND foodname=" + literal(foodname) +
            " AND content=" + literal(content) + ");"
        )
    run("\n".join(statements))


def main():
    parser = argparse.ArgumentParser(description="FoodTasteApp one-click MySQL demo setup")
    parser.add_argument("--mysql", help="Explicit path to mysql.exe")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=3306)
    parser.add_argument("--check-only", action="store_true")
    args = parser.parse_args()
    try:
        exe = mysql_executable(args.mysql)
        password = os.environ.get("FOODTASTE_DB_PASSWORD")
        if password is None:
            password = getpass.getpass("Local MySQL root password (blank if none): ")
        run = lambda sql: execute(exe, password, args.host, args.port, sql)
        print("[1/5] Checking local MySQL ...")
        print("MySQL version: " + run("SELECT VERSION();").splitlines()[-1])
        if args.check_only:
            print("Connection OK; no changes made.")
            return 0
        if not SCHEMA.exists():
            raise RuntimeError("schema.sql missing: " + str(SCHEMA))
        # Read/validate dataset before changing the database.
        foods = extract_foods()
        print("[2/5] Creating 10 tables if missing ...")
        run(SCHEMA.read_text(encoding="utf-8"))
        print("[3/5] Importing " + str(len(foods)) + " food records ...")
        import_foods(run, foods)
        print("[4/5] Adding 20 demo users, 1,040 ratings and demo history ...")
        add_demo(run, foods)
        print("[5/5] Checking table counts ...")
        counts = run("""USE tasteapp;
SELECT 'user',COUNT(*) FROM user UNION ALL
SELECT 'fooddata',COUNT(*) FROM fooddata UNION ALL
SELECT 'foodtastescore',COUNT(*) FROM foodtastescore UNION ALL
SELECT 'search_history',COUNT(*) FROM search_history UNION ALL
SELECT 'comments',COUNT(*) FROM comments UNION ALL
SELECT 'friend_requests',COUNT(*) FROM friend_requests UNION ALL
SELECT 'friendships',COUNT(*) FROM friendships UNION ALL
SELECT 'messages',COUNT(*) FROM messages UNION ALL
SELECT 'chat',COUNT(*) FROM chat UNION ALL
SELECT 'game_scores',COUNT(*) FROM game_scores;""")
        for line in counts.splitlines():
            print("  " + line.replace("\t", ": "))
        print("SUCCESS: tasteapp DB is ready. Demo login: demo01 / demo1234")
        print("Launch Unity with FOODTASTE_DB_PASSWORD set to the same password.")
        return 0
    except (RuntimeError, OSError, ET.ParseError) as exc:
        print("FAILED: " + str(exc), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
