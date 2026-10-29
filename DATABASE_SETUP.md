# FoodTasteApp 로컬 MySQL DB 원클릭 설치

1. **MySQL Server 8.x**를 설치하고 실행한다. `mysql.exe` CLI가 필요하다. Python 3도 필요하다(별도의 pip 패키지 불필요).
2. FoodTasteApp **프로젝트 루트**에서 `Install-FoodTasteDB.cmd`를 더블클릭한다. MySQL root 비밀번호가 환경 변수에 없으면 한 번 입력한다.
3. `SUCCESS: tasteapp DB is ready`가 출력되면 완료. 기존 DB의 테이블이나 사용자 데이터는 삭제하지 않는다.
4. 예시 로그인: **demo01 / demo1234** (demo01~demo20, 공통 데모 비밀번호).

> 이 프로젝트는 기존 코드가 MySQL root와 평문 비밀번호에 의존하는 **개발/시연용 프로그램**이다. 외부에 DB 포트를 열지 말고 개인 로컬 PC에서만 사용해야 한다. 코드의 로그인·해시·쿼리 보안은 별도 개선이 필요하다.

## Unity 실행 환경 변수

코드에서 `Environment.GetEnvironmentVariable("FOODTASTE_DB_PASSWORD")`를 사용한다. 설치 도구와 Unity의 비밀번호가 같아야 한다. Unity Hub를 시작하기 **전** 같은 PowerShell에서 다음처럼 설정하면 된다.

```powershell
$env:FOODTASTE_DB_PASSWORD = Read-Host "MySQL root password"
& "$env:ProgramFiles\Unity Hub\Unity Hub.exe"
```

Unity Hub 설치 위치가 다르면 해당 경로로 바꾼다. 이미 Hub/Editor가 실행 중이면 모두 종료하고 다시 실행해야 한다. 일시적인 환경 변수를 사용하므로 세션 종료 후에는 남지 않는다.

MySQL CLI를 찾지 못하면 Python을 직접 실행해서 경로를 지정한다.

```powershell
py -3 .\tools\db\install_demo_db.py --mysql "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
```

연결만 검사하려면 `py -3 .\tools\db\install_demo_db.py --check-only`를 사용한다.

## 테이블 10개

| 테이블 | 설명 |
|---|---|
| `user` | 회원 ID, 비밀번호, 닉네임, 나이, 지역, 프로필 이미지 |
| `fooddata` | 식품 번호, 이름, 열량, 중량 (프로젝트 XML 기반) |
| `foodtastescore` | 사용자별 음식 별점(1~5), 협업 필터링 입력 |
| `search_history` | 음식 검색·식사 이력 및 아침/점심/저녁 구분 |
| `comments` | 음식 상세 화면 댓글 |
| `friend_requests` | 친구 요청 상태(pending / acceptance / rejection) |
| `friendships` | 친구 관계 상태 |
| `messages` | 친구 간 메시지 |
| `chat` | 기존 회원 탈퇴 로직의 구버전 참조를 위한 호환 테이블 |
| `game_scores` | 업다운 게임의 사용자 최고 기록 |

## 데모 데이터

* `demo01`~`demo20` 20명. 모두 52개 음식에 1~5점 점수 → **1,040개 평점**.
* 사용자마다 일부 공통 음식(12개)과 40개 개별 음식을 평가한다. 공통 평점 덕분에 추천 알고리즘의 최소 공동 평가 조건을 충족한다.
* 최근 6일 범위의 검색·식사 이력, 게임 점수, 일부 댓글·친구·메시지도 샘플로 생성한다.
* 재실행해도 데모 회원/음식 평점을 덮어쓰지 않으며, 기존 데이터에 `DROP`, `TRUNCATE`, `DELETE`를 사용하지 않는다.
* 기존에 생성된 **서로 다른 스키마**의 테이블이 있다면 자동 변경하지 않고 SQL 오류를 표시한다. 기존 데이터가 있는 DB에 무조건 맞춰 변경하는 마이그레이션은 제공하지 않는다.

스크립트 경로는 `tools/db/schema.sql`, `tools/db/install_demo_db.py`.