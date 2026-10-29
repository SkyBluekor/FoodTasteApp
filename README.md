# FoodTasteApp

Unity로 만든 음식 취향 앱 프로젝트입니다.

## 실행 환경

- Unity 에디터 버전은 `ProjectSettings/ProjectVersion.txt`를 참고하세요.
- 데이터베이스를 사용하는 기능은 로컬 MySQL의 `tasteapp` 데이터베이스에 연결합니다.
- Unity 에디터 또는 실행 파일을 시작하기 전에 환경 변수 `FOODTASTE_DB_PASSWORD`에 MySQL `root` 사용자의 비밀번호를 설정하세요. 비밀번호를 소스 코드나 Git에 저장하지 마세요.

Windows PowerShell에서 현재 세션에 설정하는 예시:

```powershell
$env:FOODTASTE_DB_PASSWORD = '<MySQL 비밀번호>'
```

환경 변수를 설정한 세션에서 Unity를 시작해야 에디터가 해당 값을 사용할 수 있습니다. 기존에 실행 중인 Unity 에디터는 설정 후 다시 시작하세요.

## 로컬 DB 원클릭 구성 (데모)

MySQL Server 8.x와 Python 3를 설치하고 MySQL 서버를 실행한 후, 프로젝트 루트의 **`Install-FoodTasteDB.cmd`를 더블클릭**하세요. MySQL root 비밀번호를 입력하면 `tasteapp` DB에 필요한 테이블 10개, 프로젝트 XML 음식 목록, 데모 회원 20명과 음식 평점 1,040개를 준비합니다. 기존 테이블/평점을 삭제하지 않으며 동일한 데모 데이터는 중복 생성하지 않도록 작성했습니다.

데모 로그인: `demo01` / `demo1234` (`demo01`~`demo20`). Unity를 실행할 때는 앞서 안내한 `FOODTASTE_DB_PASSWORD`를 동일하게 설정해야 합니다. 앱은 로컬 전용 개발 시연용이며 계정 보안은 운영 서비스 수준이 아닙니다.

테이블의 전체 목록과 설치 문제 해결은 [DATABASE_SETUP.md](DATABASE_SETUP.md)에서 확인하세요.
