#!/usr/bin/env python3
"""Add a connected, repeatable local demo without changing existing records."""
import argparse
import getpass
import os

from install_demo_db import execute, literal as nonempty_literal, mysql_executable


def literal(value):
    return "''" if value == '' else nonempty_literal(value)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--mysql')
    parser.add_argument('--host', default='127.0.0.1')
    parser.add_argument('--port', type=int, default=3306)
    parser.add_argument('--check-only', action='store_true')
    args = parser.parse_args()
    password = os.environ.get('FOODTASTE_DB_PASSWORD')
    if password is None:
        password = getpass.getpass('MySQL root password: ')
    run = lambda sql: execute(mysql_executable(args.mysql), password, args.host, args.port, sql)

    users = [('ftdemo%02d' % i, '시식친구%02d' % i) for i in range(1, 31)]
    expected = ' OR '.join('(id=%s AND nickname<>%s) OR (nickname=%s AND id<>%s)' %
                           (literal(uid), literal(nick), literal(nick), literal(uid))
                           for uid, nick in users)
    result = run('USE tasteapp; SELECT COUNT(*) FROM user WHERE ' + expected + ';')
    if int(result.splitlines()[-1]):
        raise RuntimeError('Demo ID or nickname already belongs to a different account; no changes made.')
    existing = run("USE tasteapp; SELECT COUNT(*) FROM user WHERE id='demo01';")
    if int(existing.splitlines()[-1]) != 1:
        raise RuntimeError('demo01 does not exist; no changes made.')
    if args.check_only:
        print('Preflight succeeded; no changes made.')
        return

    statements = ['USE tasteapp;', 'START TRANSACTION;']
    for i, (uid, nick) in enumerate(users, 1):
        statements.append('INSERT INTO user (id,pwd,nickname,age,region) '
                          'SELECT %s,%s,%s,%d,%s WHERE NOT EXISTS '
                          '(SELECT 1 FROM user WHERE id=%s);' %
                          (literal(uid), literal('demo1234'), literal(nick),
                           20 + i % 25, literal(('서울', '부산', '경기', '대전', '대구')[i % 5]), literal(uid)))

    # The two ratings for each food are distributed across 30 people. The
    # duplicate-name offset keeps each reviewer's rating unique per name, which
    # matters because the application's comment query joins on name and user.
    food_rows = ("SELECT f.foodnum,f.foodname, "
                 "(SELECT COUNT(*) FROM fooddata d WHERE d.foodname=f.foodname AND d.foodnum<f.foodnum) AS dup_index "
                 "FROM fooddata f")
    for offset in range(2):
        reviewer = "CONCAT('ftdemo',LPAD(MOD(CRC32(f.foodname)+2*f.dup_index+%d,30)+1,2,'0'))" % offset
        score = "2+MOD(CRC32(CONCAT(f.foodname,'-',%d)),4)" % offset
        statements.append("INSERT INTO foodtastescore(id,foodnum,foodscore,foodname) "
                          "SELECT %s,f.foodnum,%s,f.foodname FROM (%s) f "
                          "WHERE NOT EXISTS (SELECT 1 FROM foodtastescore r WHERE r.id=%s AND r.foodnum=f.foodnum);" %
                          (reviewer, score, food_rows, reviewer))

    # One new review only for food names that have no comment already.
    # Select a score belonging to a demo reviewer so the UI shows its score.
    statements.append("""INSERT INTO comments(foodname,id,replydate,content)
        SELECT f.foodname,r.id,DATE_SUB(NOW(),INTERVAL MOD(CRC32(f.foodname),25) DAY),
        CASE MOD(CRC32(f.foodname),8)
          WHEN 0 THEN '부담 없이 먹기 좋았어요.'
          WHEN 1 THEN '생각보다 맛있어서 다음에 또 먹고 싶어요.'
          WHEN 2 THEN '식사로 먹기 괜찮았어요.'
          WHEN 3 THEN '간이 잘 맞아서 맛있게 먹었어요.'
          WHEN 4 THEN '취향에 잘 맞는 맛이었어요.'
          WHEN 5 THEN '한 번쯤 먹어볼 만해요.'
          WHEN 6 THEN '친구와 함께 맛있게 먹었어요.'
          ELSE '다음에도 다시 먹어보고 싶어요.' END
        FROM (SELECT MIN(foodnum) foodnum,foodname FROM fooddata GROUP BY foodname) f
        JOIN foodtastescore r ON r.foodnum=f.foodnum
          AND r.id=CONCAT('ftdemo',LPAD(MOD(CRC32(f.foodname),30)+1,2,'0'))
        WHERE NOT EXISTS (SELECT 1 FROM comments c WHERE c.foodname=f.foodname);""")

    for i, (uid, _) in enumerate(users, 1):
        # A few recent searches and meals per person, tied to real food rows.
        for days, meal, tag in [(0, 'morning', 'eaten'), (0, 'afternoon', 'eaten'),
                                (1, 'night', 'eaten'), (3, 'afternoon', 'eaten'),
                                (0, '', 'search'), (2, '', 'search')]:
            food = '(SELECT foodname FROM fooddata ORDER BY foodnum LIMIT 1 OFFSET %d)' % ((i * 17 + days * 11 + len(meal)) % 160)
            statements.append("INSERT INTO search_history(user_id,tag,foodname,search_date,eaten_tag) "
                "SELECT %s,%s,%s,DATE_SUB(CURDATE(),INTERVAL %d DAY),%s "
                "WHERE NOT EXISTS (SELECT 1 FROM search_history h WHERE h.user_id=%s AND h.tag=%s "
                "AND h.foodname=%s AND h.search_date=DATE_SUB(CURDATE(),INTERVAL %d DAY) AND h.eaten_tag=%s);" %
                (literal(uid), literal(tag), food, days, literal(meal), literal(uid), literal(tag), food, days, literal(meal)))
        statements.append("INSERT INTO game_scores(player_id,score) SELECT %s,%d WHERE NOT EXISTS "
                          "(SELECT 1 FROM game_scores WHERE player_id=%s);" %
                          (literal(uid), 8 + (i * 7) % 48, literal(uid)))

    # demo01 is the documented demo login; link five new friends to that user.
    conversations = [
        ('오늘 점심 뭐 먹었어요?', '비빔밥 먹었어요. 생각보다 맛있더라고요!', '오 저도 다음에 먹어봐야겠어요.', '좋아요, 먹고 평가도 남겨주세요!'),
        ('요즘 어떤 음식 자주 먹어요?', '요즘은 국밥을 자주 먹어요.', '저도 따뜻한 국물이 좋더라고요.', '다음에 맛있는 곳 찾아봐요!'),
        ('오늘 저녁 메뉴 정했어요?', '아직 고민 중이에요. 추천해 주세요!', '저는 김치볶음밥이 생각나요.', '좋네요! 그걸로 정해야겠어요.'),
        ('앱에서 음식 평가해 봤어요?', '네, 먹어본 음식 몇 개 평가했어요.', '서로 취향 비교해 보면 재밌겠어요.', '좋아요. 다음에도 같이 찾아봐요!'),
        ('주말에 맛있는 거 먹었어요?', '친구랑 맛있는 점심 먹었어요!', '부럽네요. 저도 주말에 먹으러 가야겠어요.', '메뉴 정하면 알려주세요!'),
    ]
    for i, conversation in enumerate(conversations, 1):
        friend = 'ftdemo%02d' % i
        statements.append("INSERT INTO friendships(user_id_1,user_id_2,status) SELECT 'demo01',%s,'acceptance' "
                          "WHERE NOT EXISTS (SELECT 1 FROM friendships WHERE (user_id_1='demo01' AND user_id_2=%s) "
                          "OR (user_id_1=%s AND user_id_2='demo01'));" % ((literal(friend),) * 3))
        for j, message in enumerate(conversation):
            sender, receiver = ('demo01', friend) if j % 2 == 0 else (friend, 'demo01')
            statements.append("INSERT INTO messages(sender_id,receiver_id,message,send_at) "
                "SELECT %s,%s,%s,DATE_SUB(NOW(),INTERVAL %d HOUR) WHERE NOT EXISTS "
                "(SELECT 1 FROM messages WHERE sender_id=%s AND receiver_id=%s AND message=%s);" %
                (literal(sender), literal(receiver), literal(message), 36 - i * 4 - j,
                 literal(sender), literal(receiver), literal(message)))
    # Fill empty meal slots only; do not change demo01's existing meals.
    for offset, meal in enumerate(('morning', 'afternoon', 'night')):
        food = '(SELECT foodname FROM fooddata ORDER BY foodnum LIMIT 1 OFFSET %d)' % (10 + offset * 7)
        statements.append("INSERT INTO search_history(user_id,tag,foodname,search_date,eaten_tag) "
            "SELECT 'demo01','eaten',%s,CURDATE(),%s WHERE NOT EXISTS "
            "(SELECT 1 FROM search_history WHERE user_id='demo01' AND tag='eaten' AND search_date=CURDATE() AND eaten_tag=%s);" %
            (food, literal(meal), literal(meal)))
    statements.append('COMMIT;')
    run('\n'.join(statements))
    print('Demo data committed successfully.')


if __name__ == '__main__':
    try:
        main()
    except (RuntimeError, OSError, ValueError) as error:
        raise SystemExit('FAILED: ' + str(error))
